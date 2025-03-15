using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapWebManager.Data;
using SnapWebManager.Models;

namespace SnapWebManager;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class ClientSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ClientSettings/5
    [HttpGet("{clientId}", Name = "Get")]
    public async Task<IActionResult> Get(string clientId)
    {
        var client = _context.Clients?.Find(clientId);
        if (client == null) return NotFound();
        await _context.Entry(client).Collection(c => c.AllowedModules).LoadAsync();

        return Ok(new ClientSettingsResponse(client));
    }
}