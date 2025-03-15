using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class NameManager
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<NameManager> _logger;
    private BlockingCollection<NameModel> _names = new();
    private readonly List<NameModel> _namesToRemove = new();

    public NameManager(IServiceProvider provider, ILogger<NameManager> logger)
    {
        _provider = provider;
        _logger = logger;

        Init().Wait();
    }
    public static Regex validCharactersRegex = new(@"['/\\<>%\$]");

    public static bool IsValidName(string name)
    {
        if (validCharactersRegex.IsMatch(name))
            return false;
        
        return true;
    }
    public async Task<IEnumerable<NameModel>> GetNames()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Names == null)
            throw new Exception("NameManager.GetNames is null parse it properly");

        return await context.Names.AsNoTracking().ToListAsync();
    }
    public async Task Init()
    {
        _names = new BlockingCollection<NameModel>(new ConcurrentQueue<NameModel>(await GetNames()));
    }
    public async Task<NameModel> Take()
    {
        _logger.LogDebug($"Taking name from queue. Current queue length is {_names.Count}");
        
        while (true)
        {
            if (!_names.TryTake(out var p, TimeSpan.FromSeconds(10))) throw new Exception("No available names.");

            _logger.LogDebug($"Using name {p.Id}");
            // This proxy has been marked for deletion, so we should ask for another and should not requeue it
            if (_namesToRemove.Any(name => name.Equals(p)))
            {
                _logger.LogDebug($"Name {p.Id} has been marked for deletion and will not be requeued");
                continue;
            }

            // put it back at the end of the queue
            _names.TryAdd(p);
            await Delete(p);
            return p;
        }
    }
    public async Task<bool> Delete(NameModel id)
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Entry(id).ReloadAsync();


        if (context.Names == null)
            throw new Exception("NameManager.Delete is null parse it properly");

        context.Names.Remove(id);
        await context.SaveChangesAsync();

        _logger.LogDebug($"Name flagged for removal: {id.Id}");

        _namesToRemove.Add(id);

        return true;
    }
    public async Task<IEnumerable<NameModel>> GetAllNames()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Names == null)
            throw new Exception("NameManager.GetAllNames is null parse it properly");

        return await context.Names.ToListAsync();
    }
    public async Task<NameUploadResult> Import(string filePath)
    {
        using var scope = _provider.CreateScope();
        var names = (await GetAllNames()).ToList();

        await using var stream = File.OpenRead(filePath);
        var reader = new StreamReader(stream);
        var addedNames = new List<NameModel>();
        var duplicated = new List<NameModel>();
        var rejected = new List<NameRejectedReason>();

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            if (line != null)
            {
                var fields = line.Split(':');

                NameModel name;
                try
                {
                    if (string.IsNullOrWhiteSpace(fields[0]) || string.IsNullOrWhiteSpace(fields[1]))
                    {
                        rejected.Add(new NameRejectedReason { firstName = fields[0], lastName = fields[1], Reason = $"Line {lineNumber} - first or last name is empty" });
                        continue;
                    }

                    if (!IsValidName(fields[0]) || !IsValidName(fields[1]))
                    {
                        rejected.Add(new NameRejectedReason { firstName = fields[0], lastName = fields[1], Reason = "Name is not valid" });
                        continue;
                    }

                    name = new NameModel
                    {
                        FirstName = fields[0],
                        LastName = fields[1]
                    };

                    if (names.Exists(e => e.FirstName == name.FirstName && e.LastName == name.LastName) || addedNames.Exists(e => e.FirstName == name.FirstName && e.LastName == name.LastName))
                    {
                        duplicated.Add(name);
                        continue;
                    }

                    addedNames.Add(name);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new LineParseErrorException(lineNumber);
                }
            }
        }

        if (addedNames.Count > 0)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Names == null)
                throw new Exception("NameManager.Import is null parse it properly");

            context.Names.AddRange(addedNames);
            
            await context.SaveChangesAsync();
            
            foreach (var added in addedNames)
                _names.TryAdd(added);
        }

        return new NameUploadResult { Added = addedNames, Duplicated = duplicated, Rejected = rejected };
    }
}