using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class WorkSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public WorkSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(CancellationToken token)
    {
        var tasks = await _context.WorkRequests.Where(w => w.Status == WorkStatus.NotRun || w.Status == WorkStatus.Incomplete || (w.Status == WorkStatus.Waiting && (w.AccountsToUse - w.AccountsFail - w.AccountsPass) > 0) || w.Status == WorkStatus.Ok).ToListAsync(token);
        
        return View(tasks);
    }
}