using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class MacroManager
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<MacroManager> _logger;
    public BlockingCollection<MacroModel> _macros = new();

    public MacroManager(IServiceProvider provider, ILogger<MacroManager> logger)
    {
        _provider = provider;
        _logger = logger;

        Init().Wait();
    }

    public async Task<IEnumerable<MacroModel>> GetMacros()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Macros == null)
            throw new Exception("MacroManager.GetMacros is null parse it properly");

        return await context.Macros.AsNoTracking().ToListAsync();
    }
    public async Task Init()
    {
        _macros = new BlockingCollection<MacroModel>(new ConcurrentQueue<MacroModel>(await GetMacros()));
    }

    public async Task<bool> DeleteMacro(MacroModel id)
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Entry(id).ReloadAsync();

        if (context.Macros == null)
            throw new Exception("MacroManager.DeleteMacro is null parse it properly");

        context.Macros.Remove(id);
        await context.SaveChangesAsync();

        _logger.LogDebug($"Macro flagged for removal: {id.Id}");

        return true;
    }
    public async Task<IEnumerable<MacroModel>> GetAllMacros()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Macros == null)
            throw new Exception("MacroManager.GetAllMacros is null parse it properly");

        return await context.Macros.ToListAsync();
    }
    public async Task<MacroUploadResult> Import(string filePath)
    {
        using var scope = _provider.CreateScope();
        var macros = (await GetAllMacros()).ToList();

        await using var stream = File.OpenRead(filePath);
        var reader = new StreamReader(stream);
        var addedMacros = new List<MacroModel>();
        var duplicated = new List<MacroModel>();
        var rejected = new List<MacroRejectedReason>();

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            if (line != null)
            {
                var fields = line.Split(':');

                MacroModel macro;
                try
                {
                    if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line))
                    {
                        rejected.Add(new MacroRejectedReason { Text = line, Reason = $"Line {lineNumber} - Macro is empty" });
                        continue;
                    }

                    macro = new MacroModel
                    {
                        Text = "#" + fields[0],
                        Replacement = fields[1]
                    };

                    if (macros.Exists(e => e.Text == macro.Text) || addedMacros.Exists(e => e.Text == macro.Text))
                    {
                        duplicated.Add(macro);
                        continue;
                    }

                    addedMacros.Add(macro);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new LineParseErrorException(lineNumber);
                }
            }

        }

        if (addedMacros.Count > 0)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Macros == null)
                throw new Exception("Import is null parse it properly");

            await context.Macros.AddRangeAsync(addedMacros);
            
            await context.SaveChangesAsync();
            
            foreach (var added in addedMacros)
                _macros.TryAdd(added);
        }

        return new MacroUploadResult() { Added = addedMacros, Duplicated = duplicated, Rejected = rejected };
    }
}