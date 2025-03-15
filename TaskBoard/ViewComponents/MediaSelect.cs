using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapchatLib;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class MediaSelectViewModel
{
    public string ControlIdPrefix { get; set; } // The id of the media select control
    public int Iteration { get; set; } // Used for generating increasing ids in html elements
    public IEnumerable<MediaFile> Files { get; set; }
    public bool ShowNoMediaLink { get; set; }
    public bool ShowSwipeUpUrlField { get; set; }
    public bool ShowDelayField { get; set; }
}

public class MediaSelectInvokeArguments
{
    public string ControlIdPrefix { get; set; }
    public bool ShowNoMediaLink { get; set; }
    public bool ShowSwipeUpUrlField { get; set; }
    public int Iteration { get; set; }
    public bool ShowDelayField { get; set; }
}

public class MediaSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public MediaSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(MediaSelectInvokeArguments arguments)
    {
        var items = await _context.MediaFiles.ToListAsync();
        return View(new MediaSelectViewModel() { ControlIdPrefix = arguments.ControlIdPrefix, Files = items, ShowNoMediaLink = arguments.ShowNoMediaLink, ShowSwipeUpUrlField = arguments.ShowSwipeUpUrlField, Iteration = arguments.Iteration, ShowDelayField = arguments.ShowDelayField });
    }
}