using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class LockedAccountsGraphViewModel
{
    public int okayAccounts;
    public int lockedAccounts;
    public int needsChecked;
    public int rateLimited;
    public int bannedAccounts;
}

public class LockedAccountsGraph: ViewComponent
{
    private readonly ApplicationDbContext _context;

    public LockedAccountsGraph(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var okayCount = await _context.Accounts.CountAsync(a => a.AccountStatus == AccountStatus.OKAY);
        var lockedCount = await _context.Accounts.CountAsync(a => a.AccountStatus == AccountStatus.LOCKED);
        var rateLimitedCount = await _context.Accounts.CountAsync(a => a.AccountStatus == AccountStatus.RATE_LIMITED);
        var NeedsCheckedCount = await _context.Accounts.CountAsync(a => a.AccountStatus == AccountStatus.NEEDS_CHECKED);
        var bannedCount = await _context.Accounts.CountAsync(a => a.AccountStatus == AccountStatus.BANNED);
        return View(new LockedAccountsGraphViewModel() { okayAccounts = okayCount, lockedAccounts = lockedCount, needsChecked = NeedsCheckedCount, rateLimited = rateLimitedCount, bannedAccounts = bannedCount});
    }
}