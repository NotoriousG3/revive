using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class UploadManager
{
    private readonly ILogger<UploadManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    public static string StoragePath = "/db/media";
    public static string TempStoragePath = "/db/media/temp";

    public UploadManager(IServiceProvider serviceProvider, ILogger<UploadManager> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        if (!Directory.Exists(StoragePath))
            Directory.CreateDirectory(StoragePath);

        if (!Directory.Exists(TempStoragePath))
            Directory.CreateDirectory(TempStoragePath);
    }

    public async Task<MediaFile> AddFile(IFormFile inputFile, bool useCache = true)
    {
        var existingFile = await GetFile(inputFile);

        // Try to use the cache if requested and the file exist
        if (existingFile != null && useCache)
        {
            await UpdateLastAccess(existingFile);
            return existingFile;
        }
        
        var ext = Path.GetExtension(inputFile.FileName);
        var isTextFile = ext == ".txt";
        
        var fileInfo = await WriteToDisk(inputFile, ext, isTextFile);
        var mediaFile = await SaveMediaEntry(inputFile, fileInfo);
        return mediaFile;
    }

    public async Task UpdateLastAccess(MediaFile entry)
    {
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        entry.LastAccess = DateTime.UtcNow;
        context.Update(entry);
        await context.SaveChangesAsync();
    }

    public async Task<MediaFile?> GetFile(IFormFile inputFile)
    {
        return await GetFile(inputFile.FileName);
    }

    public async Task<MediaFile?> GetFile(long id)
    {
        if (id == 0) return null;
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.MediaFiles.FindAsync(id);
    }

    public async Task<MediaFile?> GetFile(string fileName)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var files = await context.MediaFiles.Include(e => e.WorkRequests).ToListAsync();
        return files.FirstOrDefault(f => f.Name == fileName, null);
    }

    private async Task<MediaFile> SaveMediaEntry(IFormFile inputFile, FileInfo fileInfo)
    {
        // Save to DB
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entry = new MediaFile()
        {
            Name = inputFile.FileName,
            SizeBytes = fileInfo.Length,
            ServerPath = fileInfo.FullName,
            LastAccess = DateTime.UtcNow
        };
        
        await context.MediaFiles.AddAsync(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    private async Task<FileInfo> WriteToDisk(IFormFile inputFile, string originalExtension, bool isTempFile)
    {
        _logger.LogDebug($"Receiving file: {inputFile.FileName} - Size: {inputFile.Length}");

        // Get a random temp file name to rename it in the server
        var targetFileName = Path.GetFileName(Path.GetTempFileName());
        
        // Keep the original extension
        targetFileName = targetFileName.Replace(Path.GetExtension(targetFileName), originalExtension); 

        var targetPath = Path.Combine(isTempFile ? TempStoragePath : StoragePath, targetFileName);

        await using var stream = File.Create(targetPath);
        _logger.LogDebug($"Saving to {targetPath}");
        await inputFile.CopyToAsync(stream);
        var info = new FileInfo(targetPath);

        return info;
    }

    public async Task DeleteFile(string filename)
    {
        var entry = await GetFile(filename);
        if (entry == null) return;
        await DeleteFile(entry);
        
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var work in entry.WorkRequests)
        {
            work.MediaFileId = null;

            if (work.IsScheduled)
            {
                work.Status = WorkStatus.Cancelled;
            }

            context.Update(work);
        }
        
        await context.SaveChangesAsync();
        context.MediaFiles.Remove(entry);
        await context.SaveChangesAsync();
    }

    private async Task DeleteFile(MediaFile file)
    {
        if (File.Exists(file.ServerPath))
            File.Delete(file.ServerPath);
    }

    public async Task DeleteFiles(List<MediaFile> files)
    {
        if (!files.Any()) return;
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var saveTwice = false;
        foreach (var entry in files)
        {
            await DeleteFile(entry);
            
            if (entry.WorkRequests == null) continue;
            
            foreach (var work in entry.WorkRequests)
            {
                work.MediaFileId = null;
                if (work.IsScheduled)
                    work.Status = WorkStatus.Cancelled;
                
                context.Update(work);
            }

            saveTwice = true;
        }
        
        // Save first so that any work request MediaFileId is set to null
        if (saveTwice)
            await context.SaveChangesAsync();
        
        context.MediaFiles.RemoveRange(files);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MediaFile>> GetFiles()
    {
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.MediaFiles.Include(e => e.WorkRequests).ToListAsync();
    }

    public async Task<long> CurrentDiskUsageBytes()
    {
        var files = await GetFiles();
        return files.Sum(f => f.SizeBytes);
    }
}