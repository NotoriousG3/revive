using Microsoft.AspNetCore.Mvc;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

public class MediaManagerController : Controller
{
    private readonly UploadManager _uploadManager;
    private readonly AppSettingsLoader _settingsLoader;

    public MediaManagerController(UploadManager uploadManager, AppSettingsLoader appSettingsLoader)
    {
        _uploadManager = uploadManager;
        _settingsLoader = appSettingsLoader;
    }
    
    // GET
    public async Task<IActionResult> Index()
    {
        var settings = await _settingsLoader.Load();
        
        return View(new MediaManagerViewModel()
        {
            CurrentUsageBytes = await _uploadManager.CurrentDiskUsageBytes(),
            MaxQuotaMb = settings.MaxQuotaMb
        });
    }
}