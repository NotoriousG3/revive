using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class UserNameManager
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<UserNameManager> _logger;
    public BlockingCollection<UserNameModel> _names = new();
    private readonly List<UserNameModel> _namesToRemove = new();

    public UserNameManager(IServiceProvider provider, ILogger<UserNameManager> logger)
    {
        _provider = provider;
        _logger = logger;

        Init().Wait();
    }
    public static Regex validCharactersRegex = new(@"['/\\<>%\$]");

    public static bool IsValidUserName(string name)
    {
        if (validCharactersRegex.IsMatch(name))
            return false;
        
        return true;
    }
    public async Task<IEnumerable<UserNameModel>> GetNames()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.UserNames.AsNoTracking().ToListAsync();
    }
    public async Task Init()
    {
        _names = new BlockingCollection<UserNameModel>(new ConcurrentQueue<UserNameModel>(await GetNames()));
    }
    public async Task<UserNameModel> Take()
    {
        var count = 0;
        _logger.LogDebug($"Taking name from queue. Current queue length is {_names.Count}");
        
        while (true)
        {
            if (!_names.TryTake(out var p, TimeSpan.FromSeconds(10))) throw new Exception("No available names.");

            _logger.LogDebug($"Using name {p.Id} - {p.UserName}");
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
    public async Task<bool> Delete(UserNameModel id)
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Entry(id).ReloadAsync();
        
        context.UserNames.Remove(id);
        await context.SaveChangesAsync();

        _logger.LogDebug($"Name flagged for removal: {id.Id}");

        _namesToRemove.Add(id);

        return true;
    }
    public async Task<IEnumerable<UserNameModel>> GetAllNames()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.UserNames.ToListAsync();
    }
    public async Task<UserNameUploadResult> Import(string filePath)
    {
        using var scope = _provider.CreateScope();
        var names = (await GetAllNames()).ToList();

        await using var stream = File.OpenRead(filePath);
        var reader = new StreamReader(stream);
        var addedNames = new List<UserNameModel>();
        var duplicated = new List<UserNameModel>();
        var rejected = new List<UserNameRejectedReason>();

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            UserNameModel name;
            try
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line))
                {
                    rejected.Add(new UserNameRejectedReason {userName = line, Reason = $"Line {lineNumber} - Username is empty"});
                    continue;
                }
                
                if (!IsValidUserName(line))
                {
                    rejected.Add(new UserNameRejectedReason {userName = line, Reason = "Username is not valid"});
                    continue;
                }

                name = new UserNameModel
                {
                    UserName = line
                };

                if (names.Exists(e => e.UserName == name.UserName) || addedNames.Exists(e => e.UserName == name.UserName))
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

        if (addedNames.Count > 0)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            context.UserNames.AddRange(addedNames);
            
            await context.SaveChangesAsync();
            
            foreach (var added in addedNames)
                _names.TryAdd(added);
        }

        return new UserNameUploadResult { Added = addedNames, Duplicated = duplicated, Rejected = rejected };
    }
}