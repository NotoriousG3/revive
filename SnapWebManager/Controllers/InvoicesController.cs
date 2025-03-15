using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapWebManager.Data;

namespace SnapWebManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{clientid}")]
    public async Task <IActionResult> Index(string clientid)
    {
        var iList = await _context.Invoices.Include(e => e.Client).ToListAsync();
        var invoices = iList.Where(i => i.Client.ClientId == clientid);
        return Ok(invoices);
    }
}