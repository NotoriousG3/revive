using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class TargetExportFilterSelectViewModel
{
    public string ControlId;
    public bool ShowLabel;
    public IEnumerable<string> FilterTarget;
}

public class TargetExportFilterSelectViewArguments
{
    public string ControlId;
    public bool ShowLabel;
}

public class TargetExportFilterSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public TargetExportFilterSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(TargetExportFilterSelectViewArguments args)
    {
        List<string> items;
        
        switch (args.ControlId)
        {
            case "filterCountryCode":
                items = await _context.TargetUsers.Where(t => t.CountryCode != null).Select(t => t.CountryCode).Distinct().ToListAsync();
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterGender":
                items = await _context.TargetUsers.Where(t => t.Gender != null).Select(t => t.Gender).Distinct().ToListAsync();
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterRace":
                items = await _context.TargetUsers.Where(t => t.Race != null).Select(t => t.Race).Distinct().ToListAsync();
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterAdded":
                items = await _context.TargetUsers.Where(t => t.Added != null).Select(t => t.Added.ToString()).Distinct().ToListAsync();
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterSearched":
                items = await _context.TargetUsers.Where(t => t.Searched != null).Select(t => t.Searched.ToString()).Distinct().ToListAsync();
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            default:
                items = new List<string>(){"US","CA"};
                return View(new TargetExportFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
        }
    }
}