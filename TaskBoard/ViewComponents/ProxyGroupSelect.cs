using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class ProxyGroupSelectViewModel
{
    public string ControlId;
    public bool ShowLabel;
    public IEnumerable<ProxyGroup> Groups;
}

public class ProxyGroupSelectViewArguments
{
    public string ControlId;
    public bool ShowLabel;
}

public class ProxyGroupSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public ProxyGroupSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(ProxyGroupSelectViewArguments args)
    {
        var items = await _context.ProxyGroups.ToListAsync();
        return View(new ProxyGroupSelectViewModel() { ControlId = args.ControlId, Groups = items, ShowLabel = args.ShowLabel});
    }
}