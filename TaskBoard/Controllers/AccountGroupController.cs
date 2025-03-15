using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TaskBoard.Models;

namespace TaskBoard.Controllers
{
    public class AccountGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AccountGroup
        public async Task<IActionResult> Index()
        {
              return _context.AccountGroups != null ? 
                          View(await _context.AccountGroups.Include(e => e.Accounts).ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AccountGroups'  is null.");
        }

        // GET: AccountGroup/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.AccountGroups == null)
            {
                return NotFound();
            }

            var accountGroup = await _context.AccountGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountGroup == null)
            {
                return NotFound();
            }

            return View(accountGroup);
        }

        // GET: AccountGroup/Create
        public IActionResult Create()
        {
            return View();
        }

        private async Task LinkAccountsToGroup(AccountGroup group, List<AccountGroupSelectedAccount> selected)
        {
            // first, remove accounts that no longer should be there
            var accountIds = selected.Select(a => a.Id).ToHashSet();
            var cleanAccounts = group.Accounts.Where(a => accountIds.Any(id => id == a.Id)).ToHashSet();
            foreach (var id in accountIds)
            {
                var account = await _context.Accounts.FindAsync(id);
                
                // Make sure we don't add something that doesn't exist
                if (account == null) continue;
                
                // We don't want to add duplicate entries
                if (cleanAccounts.Any(a => a.Id == id)) continue;
                cleanAccounts.Add(account);
            }

            group.Accounts = cleanAccounts;

            await _context.SaveChangesAsync();
        }
        
        // POST: AccountGroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SelectedAccounts")] AccountGroupChanges changes)
        {
            if (ModelState.IsValid)
            {
                // Check if a group with the same name already exists
                if (await _context.AccountGroups.AnyAsync(g => g.Name.ToLower() == changes.Name.ToLower()))
                {
                    ModelState.TryAddModelError("Name", $"A group with name {changes.Name} already exists");
                    return View(changes);
                } 
                
                var group = new AccountGroup() { Name = changes.Name, Accounts = new List<SnapchatAccountModel>() };
                
                _context.Add(group);
                await _context.SaveChangesAsync();
                
                // now we look for the accounts that we need to link to the group
                if (!string.IsNullOrWhiteSpace(changes.SelectedAccounts))
                {
                    var selected = JsonConvert.DeserializeObject<List<AccountGroupSelectedAccount>>(changes.SelectedAccounts);
                    if (selected.Count > 0) await LinkAccountsToGroup(group, selected);
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(changes);
        }

        // GET: AccountGroup/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.AccountGroups == null)
            {
                return NotFound();
            }

            var accountGroup = await _context.AccountGroups.FindAsync(id);
            if (accountGroup == null)
            {
                return NotFound();
            }

            await _context.Entry(accountGroup).Collection(e => e.Accounts).LoadAsync();

            var selected = accountGroup.Accounts.Select(a => new AccountGroupSelectedAccount()
                { Id = a.Id, Name = a.Username });
            
            return View(new AccountGroupChanges() { Id = accountGroup.Id, Name = accountGroup.Name, SelectedAccounts = JsonConvert.SerializeObject(selected)});
        }

        // POST: AccountGroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,SelectedAccounts")] AccountGroupChanges changes)
        {
            if (id != changes.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var group = await _context.AccountGroups.FindAsync(changes.Id);
                    await _context.Entry(group).Collection(e => e.Accounts).LoadAsync();

                    group.Name = changes.Name;
                    _context.Update(group);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(changes.SelectedAccounts))
                    {
                        var selected =
                            JsonConvert.DeserializeObject<List<AccountGroupSelectedAccount>>(changes.SelectedAccounts);
                        await LinkAccountsToGroup(group, selected);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountGroupExists(changes.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(changes);
        }

        // GET: AccountGroup/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.AccountGroups == null)
            {
                return NotFound();
            }

            var accountGroup = await _context.AccountGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountGroup == null)
            {
                return NotFound();
            }

            return View(accountGroup);
        }

        // POST: AccountGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.AccountGroups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AccountGroups'  is null.");
            }
            var accountGroup = await _context.AccountGroups.FindAsync(id);
            if (accountGroup != null)
            {
                _context.AccountGroups.Remove(accountGroup);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountGroupExists(long id)
        {
            return (_context.AccountGroups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
