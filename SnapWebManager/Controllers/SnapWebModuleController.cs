using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapWebManager.Data;
using SnapWebModels;

namespace SnapWebManager.Controllers
{
    public class SnapWebModuleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SnapWebModuleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SnapWebModule
        public async Task<IActionResult> Index()
        {
              return _context.Modules != null ? 
                          View(await _context.Modules.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Modules'  is null.");
        }

        // GET: SnapWebModule/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var snapWebModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.DatabaseId == id);
            if (snapWebModule == null)
            {
                return NotFound();
            }

            return View(snapWebModule);
        }

        // GET: SnapWebModule/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SnapWebModule/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DatabaseId,Id,Category,Name,Description,SnapWebIconClass,Enabled,Purchaseable,Price")] SnapWebModule snapWebModule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(snapWebModule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(snapWebModule);
        }

        // GET: SnapWebModule/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var snapWebModule = await _context.Modules.FindAsync(id);
            if (snapWebModule == null)
            {
                return NotFound();
            }
            return View(snapWebModule);
        }

        // POST: SnapWebModule/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("DatabaseId,Id,Category,Name,Description,SnapWebIconClass,Enabled,Purchaseable,Price")] SnapWebModule snapWebModule)
        {
            if (id != snapWebModule.DatabaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(snapWebModule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SnapWebModuleExists(snapWebModule.DatabaseId))
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
            return View(snapWebModule);
        }

        // GET: SnapWebModule/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var snapWebModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.DatabaseId == id);
            if (snapWebModule == null)
            {
                return NotFound();
            }

            return View(snapWebModule);
        }

        // POST: SnapWebModule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Modules == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Modules'  is null.");
            }
            var snapWebModule = await _context.Modules.FindAsync(id);
            if (snapWebModule != null)
            {
                _context.Modules.Remove(snapWebModule);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SnapWebModuleExists(long id)
        {
          return (_context.Modules?.Any(e => e.DatabaseId == id)).GetValueOrDefault();
        }

        [HttpGet]
        [Route("api/snapwebmodules")]
        public async Task<IActionResult> GetModules()
        {
            return Json(await _context.Modules.ToListAsync());
        }
    }
}
