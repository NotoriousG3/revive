using Microsoft.AspNetCore.Mvc;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class TargetUserList : ViewComponent
{
    private readonly ApplicationDbContext _context;
    
    public TargetUserList(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(string[] args)
    {
        return View(new TargetUserListViewModel() { ElementId = args[1], Label = args[0], Users = new List<TargetUser>() });
    }
}