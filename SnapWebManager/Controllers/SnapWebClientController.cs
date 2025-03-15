using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SnapWebManager.Data;
using SnapWebModels;

namespace SnapWebManager.Views;

[Authorize(Roles = "Admin")]
public class SnapWebClientController : Controller
{
    private readonly ApplicationDbContext _context;

    public SnapWebClientController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SnapWebClient
    public async Task<IActionResult> Index()
    {
        return _context.Clients != null ? View(await _context.Clients.ToListAsync()) : Problem("Entity set 'ApplicationDbContext.Clients'  is null.");
    }

    // GET: SnapWebClient/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null || _context.Clients == null) return NotFound();

        var snapWebClientModel = await _context.Clients
            .FirstOrDefaultAsync(m => m.ClientId == id);
        if (snapWebClientModel == null) return NotFound();

        return View(snapWebClientModel);
    }

    // GET: SnapWebClient/Create
    public IActionResult Create()
    {
        return View();
    }

    private void SetAllowedModules(SnapWebClientModel snapWebClientModel)
    {
        if (snapWebClientModel.AllowedModules == null)
            snapWebClientModel.AllowedModules = new List<AllowedModules>();

        // Make sure our default OS is always enabled
        if (!snapWebClientModel.EnabledModules.Contains(snapWebClientModel.DefaultOS))
                snapWebClientModel.EnabledModules.Add(snapWebClientModel.DefaultOS);
        
        // Remove existing modules that are now not allowed
        foreach (var allowedModule in snapWebClientModel.AllowedModules)
            if (!snapWebClientModel.EnabledModules.Contains(allowedModule.ModuleId))
            {
                snapWebClientModel.AllowedModules.Remove(allowedModule);
                _context.Remove(allowedModule);
            }

        // Now create the allowed modules
        var missingModules = snapWebClientModel.EnabledModules.Where(e => !snapWebClientModel.AllowedModules.Select(a => a.ModuleId).Contains(e));
        foreach (var moduleId in missingModules)
        {
            var allowed = new AllowedModules {Client = snapWebClientModel, ModuleId = moduleId};
            snapWebClientModel.AllowedModules.Add(allowed);
        }
    }

    // POST: SnapWebClient/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClientId,ApiKey,DefaultOS,MaxManagedAccounts,Threads,MaxTasks,MaxAddFriendsUsers,MaxQuotaMb,AccountCooldown,AccessDeadline,EnabledModules")] SnapWebClientModel snapWebClientModel)
    {
        // Transform the model datetime to utc first
        snapWebClientModel.AccessDeadline = snapWebClientModel.AccessDeadline.ToUniversalTime();

        SetAllowedModules(snapWebClientModel);

        _context.Add(snapWebClientModel);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: SnapWebClient/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null || _context.Clients == null) return NotFound();

        var snapWebClientModel = await _context.Clients.FindAsync(id);
        if (snapWebClientModel == null) return NotFound();

        await _context.Entry(snapWebClientModel).Collection(c => c.AllowedModules).LoadAsync();
        return View(snapWebClientModel);
    }

    // POST: SnapWebClient/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("ClientId,ApiKey,DefaultOS,MaxManagedAccounts,Threads,MaxTasks,MaxAddFriendsUsers,MaxQuotaMb,AccountCooldown,AccessDeadline,EnabledModules")] SnapWebClientModel snapWebClientModel)
    {
        if (id != snapWebClientModel.ClientId) return NotFound();

        _context.Clients.Attach(snapWebClientModel);

        await _context.Entry(snapWebClientModel).Collection(m => m.AllowedModules).LoadAsync();
        snapWebClientModel.AccessDeadline = snapWebClientModel.AccessDeadline.ToUniversalTime();

        SetAllowedModules(snapWebClientModel);

        try
        {
            _context.Update(snapWebClientModel);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SnapWebClientModelExists(snapWebClientModel.ClientId))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: SnapWebClient/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null || _context.Clients == null) return NotFound();

        var snapWebClientModel = await _context.Clients
            .FirstOrDefaultAsync(m => m.ClientId == id);
        if (snapWebClientModel == null) return NotFound();

        return View(snapWebClientModel);
    }

    // POST: SnapWebClient/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (_context.Clients == null) return Problem("Entity set 'ApplicationDbContext.Clients'  is null.");
        var snapWebClientModel = _context.Clients.Include(e => e.Invoices).Include(e => e.AllowedModules).FirstOrDefault(e => e.ClientId == id);
        if (snapWebClientModel != null) _context.Clients.Remove(snapWebClientModel);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SnapWebClientModelExists(string id)
    {
        return (_context.Clients?.Any(e => e.ClientId == id)).GetValueOrDefault();
    }
}