using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class AccountGroupSelectViewModel
{
    public string ControlId;
    public bool ShowLabel;
    public IEnumerable<AccountGroup> Groups;
}

public class AccountGroupSelectViewArguments
{
    public string ControlId;
    public bool ShowLabel;
}

public class AccountGroupSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public AccountGroupSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(AccountGroupSelectViewArguments args)
    {
        var items = await _context.AccountGroups.ToListAsync();
        return View(new AccountGroupSelectViewModel() { ControlId = args.ControlId, Groups = items, ShowLabel = args.ShowLabel});
    }
}