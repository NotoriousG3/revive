using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class AccountPurgeFilterSelectViewModel
{
    public string ControlId;
    public bool ShowLabel;
    public IEnumerable<string> FilterTarget;
}

public class AccountPurgeFilterSelectViewArguments
{
    public string ControlId;
    public bool ShowLabel;
}

public class AccountPurgeFilterSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public AccountPurgeFilterSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(AccountPurgeFilterSelectViewArguments args)
    {
        List<string> items;
        
        switch (args.ControlId)
        {
            /*case "filterOS":
                items = await _context.Accounts.Where(t => t.OS != null).Select(t => t.OS.ToString()).Distinct().ToListAsync();
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});*/
            case "filterEmailValidation":
                items = await _context.Accounts.Where(t => t.EmailValidated != null).Select(t => t.EmailValidated.ToString()).Distinct().ToListAsync();
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterPhoneValidation":
                items = await _context.Accounts.Where(t => t.PhoneValidated != null).Select(t => t.PhoneValidated.ToString()).Distinct().ToListAsync();
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterStatus":
                items = await _context.Accounts.Where(t => t.AccountStatus != null).Select(t => t.AccountStatus.ToString()).Distinct().ToListAsync();
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            case "filterHasAdded":
                items = await _context.Accounts.Select(t => t.hasAdded.ToString()).Distinct().ToListAsync();
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
            default:
                items = new List<string>(){"US","CA"};
                return View(new AccountPurgeFilterSelectViewModel() { ControlId = args.ControlId, FilterTarget = items, ShowLabel = args.ShowLabel});
        }
    }
}