using TaskBoard.Models;

namespace TaskBoard;

public class UploadCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<UploadCleanupService> _logger;
    private readonly WorkRequestTracker _workRequestTracker;
    private readonly UploadManager _uploadManager;
    private Timer? _timer;

    public UploadCleanupService(ILogger<UploadCleanupService> logger, UploadManager manager, WorkRequestTracker workRequestTracker)
    {
        _logger = logger;
        _uploadManager = manager;
        _workRequestTracker = workRequestTracker;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            _logger.LogInformation("Starting Upload Cleanup Service");
            
            DeleteDanglingFiles((await _uploadManager.GetFiles()).ToList());

            _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }, stoppingToken);
    }

    private void DeleteDanglingFiles(List<MediaFile> dbFiles)
    {
        foreach (var entry in Directory.GetFileSystemEntries(UploadManager.StoragePath, "*",
                     SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(entry);
            if (dbFiles.Exists(db => db.ServerPath == fileInfo.FullName) || (fileInfo.Attributes & FileAttributes.Directory) != 0) continue;
            
            try
            {
                File.Delete(entry);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogDebug($"Access denied to delete file {entry}");
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Upload Cleanup Service is stopping.");
        DeleteDanglingFiles((await _uploadManager.GetFiles()).ToList());
        _timer?.Change(Timeout.Infinite, 0);
    }

    private async Task CleanupOldFiles()
    {
        var files = await _uploadManager.GetFiles();
        
        //TODO: Change FromDays hardcoded value to a setting?
        var toDelete = files.Where(entry => DateTime.UtcNow - entry.LastAccess > TimeSpan.FromDays(14) || !File.Exists(entry.ServerPath)).ToList();

        if (toDelete.Any())
            await _uploadManager.DeleteFiles(toDelete);
    }

    private async void DoWork(object? state)
    {
        if (_workRequestTracker.HasRunningWork()) return;
        await CleanupOldFiles();
    }
}