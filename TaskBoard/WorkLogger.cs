using TaskBoard.Models;

namespace TaskBoard;

public class WorkLogger
{
    private readonly ILogger<WorkLogger> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public WorkLogger() {}
    
    public WorkLogger(ILogger<WorkLogger> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    private async Task SaveLog(WorkRequest work, LogLevel logLevel, string msg)
    {
        var entry = new LogEntry
        {
            LogLevel = logLevel,
            Message = msg,
            WorkId = work.Id,
            Time = DateTime.UtcNow
        };

        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.LogEntries!.Add(entry);
        await context.SaveChangesAsync();
    }

    private string CreateLine(WorkRequest? work, string msg, SnapchatAccountModel? account)
    {
        var fields = new List<string>();

        if (work != null)
            fields.Add($"Job {work.Id}");

        if (account != null)
            fields.Add($"[{account.Username}]");

        fields.Add(msg);

        return string.Join(" - ", fields);
    }

    public virtual async Task LogDebug(WorkRequest? work, string msg, SnapchatAccountModel? account = null)
    {
        var line = CreateLine(work, msg, account);
        _logger.LogDebug(line);

        if (work != null)
            await SaveLog(work, LogLevel.Debug, line);
    }

    public async Task LogInformation(WorkRequest? work, string msg, SnapchatAccountModel? account = null)
    {
        var line = CreateLine(work, msg, account);
        _logger.LogInformation(line);

        if (work == null) return;
        
        await SaveLog(work, LogLevel.Information, line);
    }

    public async Task LogError(WorkRequest? work, string msg, SnapchatAccountModel? account = null)
    {
        var line = CreateLine(work, msg, account);
        _logger.LogError(line);

        if (work != null)
            await SaveLog(work, LogLevel.Error, line);
    }
    
    public async Task LogError(WorkRequest? work, Exception e, SnapchatAccountModel? account = null)
    {
        var line = CreateLine(work, e.Message + e.StackTrace, account);
        _logger.LogError(line);

        if (work != null)
            await SaveLog(work, LogLevel.Error, e.Message);
    }

    public static WorkLogger GetFromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<WorkLogger>();
    }
}