using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/[controller]")]
[ApiController]
public class UploadController : ApiController
{
    private readonly UploadManager _uploadManager;
    private readonly ApplicationDbContext _context;
    private readonly AppSettingsLoader _settingsLoader;
    private readonly WorkRequestTracker _workRequestTracker;

    private static readonly List<string> AllowedContentTypes = new()
    {
        "video/quicktime",
        "video/mp4",
        "image/png",
        "image/jpeg",
        "text/plain"
    };

    public UploadController(UploadManager uploadManager, ApplicationDbContext context, AppSettingsLoader appSettingsLoader, WorkRequestTracker workRequestTracker)
    {
        _uploadManager = uploadManager;
        _context = context;
        _settingsLoader = appSettingsLoader;
        _workRequestTracker = workRequestTracker;
    }

    // POST
    [HttpPost]
    [DisableRequestSizeLimit]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile inputFile, string? skipCache)
    {
        if (inputFile == null) return BadRequest("No input file received");
        if (inputFile.Length == 0) return BadRequest("Size of file is 0!");
        if (!AllowedContentTypes.Contains(inputFile.ContentType))
            return BadRequest($"File of type {inputFile.ContentType} is not allowed");

        var settings = await _settingsLoader.Load();
        var currentUsage = await _uploadManager.CurrentDiskUsageBytes();
        var currentMb = Utilities.BytesToMb(currentUsage);

        if (currentMb >= settings.MaxQuotaMb)
            return UnauthorizedApi($"You have exceeded your maximum storage quota of {Utilities.BytesToString(settings.MaxQuotaMb * Utilities.BytesToMbConversionLiteral)}");

        if (Utilities.BytesToMb(currentUsage + inputFile.Length) >= settings.MaxQuotaMb)
            return UnauthorizedApi(
                $"The file is too large and would exceed your maximum storage quota of {Utilities.BytesToString(settings.MaxQuotaMb * Utilities.BytesToMbConversionLiteral)}");

        var useCache = string.IsNullOrWhiteSpace(skipCache);

        var file = await _uploadManager.AddFile(inputFile, useCache);

        return OkApi(data: file.Id);
    }

    [HttpGet]
    public async Task<IActionResult> FileInServer(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) return BadRequest("filename needs to be provided");
        var file = await _uploadManager.GetFile(filename);
        if (file == null) return NotFound();
        return OkApi(data: file.Id);
    }

    [HttpPost("getfiles")]
    public async Task<IActionResult> GetFiles(DataTableAjaxModel model)
    {
        var files = SearchUtilities.SearchDataTablesEntities(model, _context.MediaFiles.Include(e => e.WorkRequests), (e => e.Name.ToLowerInvariant().Contains(model.search.value)), out var filteredCount, out var totalCount);
        var results = UIMediaFile.ToEnumerable(files, _workRequestTracker);
        return Ok(new DataTablesResponse
        {
            Draw = model.draw,
            RecordsFiltered = filteredCount,
            RecordsTotal = totalCount,
            Data = results
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var file = await _context.MediaFiles.FindAsync(id);
        if (file == null) return BadRequestApi("File not found");
        await _uploadManager.DeleteFile(file.Name);

        var files = await _context.MediaFiles.ToListAsync();
        return OkApi(data: files);
    }

    [HttpPost("purge")]
    public async Task<IActionResult> Purge()
    {
        var files = await _context.MediaFiles.ToListAsync();
        await _uploadManager.DeleteFiles(files);

        return OkApi(data: new List<MediaFile>());
    }
}