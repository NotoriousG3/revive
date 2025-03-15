using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TaskBoard.Models;

namespace TaskBoard.Controllers
{
    public class ProxyGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProxyGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProxyGroup
        public async Task<IActionResult> Index()
        {
              return _context.ProxyGroups != null ? 
                          View(await _context.ProxyGroups.Include(e => e.Proxies).ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AccountGroups' is null.");
        }

        // GET: ProxyGroup/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.AccountGroups == null)
            {
                return NotFound();
            }

            var group = await _context.ProxyGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: ProxyGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.ProxyGroups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ProxyGroups' is null.");
            }
            
            var group = await _context.ProxyGroups.FindAsync(id);

            if (group != null)
            {
                var deletedProxies = _context.Proxies.Where(p => p.Groups.Contains(group)).ToList();

                foreach (var proxy in deletedProxies)
                {
                    var unbindAccounts = _context.Accounts.Where(a => a.ProxyId == proxy.Id).ToList();

                    foreach (var account in unbindAccounts)
                    {
                        account.Proxy = null;
                        account.ProxyId = null;
                    }
                    
                    
                    _context.Proxies.Remove(proxy);
                }
                
                _context.Remove(group);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
