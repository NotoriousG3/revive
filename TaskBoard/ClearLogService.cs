namespace TaskBoard;

public class LogClearService : IHostedService, IDisposable
{
    private readonly ILogger<LogClearService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer _checkLogTimer;
    private int daysToDelete = 3;

    public LogClearService(IServiceScopeFactory scopeFactory, ILogger<LogClearService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Dispose()
    {
        _checkLogTimer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Log Clearing Service");
        _checkLogTimer = new Timer(ClearLogs, cancellationToken, TimeSpan.Zero, TimeSpan.FromHours(3));
        return Task.FromResult("Started Log Clearing Service");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Log Clearing Service");
        _checkLogTimer?.Change(Timeout.Infinite, 0);
        return Task.FromResult("Stopped Log Clearing Service");
    }

    public async void ClearLogs(object? state)
    {
        try
        {
            await using var context =
                _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.LogEntries != null)
            {
                var query = context.LogEntries.Where(l => DateTime.UtcNow > l.Time.AddDays(daysToDelete));

                var count = query.Count();
                if (count == 0) return;
                
                Console.WriteLine($"Clearing {count} old logs.");
                context.LogEntries.RemoveRange(query);
             
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
        }
    }
}