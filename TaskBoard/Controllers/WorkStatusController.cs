using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
public class WorkStatusController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly WorkScheduler _scheduler;
    private readonly WorkRequestTracker _tracker;

    public WorkStatusController(ApplicationDbContext context, WorkScheduler scheduler, WorkRequestTracker tracker)
    {
        _context = context;
        _scheduler = scheduler;
        _tracker = tracker;
    }

    // GET
    [Route("[controller]/{id}")]
    public async Task<IActionResult> Index(long id)
    {
        var work = await _context.WorkRequests.FindAsync(id);
        if (work == null) return NotFound();

        return View(work);
    }

    [HttpGet]
    [Route("api/[controller]")]
    public async Task<IActionResult> List()
    {
        var requests = UIWorkRequest.ToEnumerable(await _context.WorkRequests.ToListAsync(), _tracker);
        return Ok(requests);
    }

    [HttpGet]
    [Route("api/[controller]/{id}/logs")]
    public async Task<IActionResult> Logs(long id, int page = 0, int number = 100)
    {
        var logs = _context.LogEntries.Where(l => l.WorkId == id).OrderByDescending(l => l.Time).ThenByDescending(l => l.Id);
        var response = new GetWorkLogsResponse(await logs.ToListAsync(), page, number);
        return Ok(response);
    }

    [HttpPost]
    [Route("api/[controller]/{id}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var work = await _context.WorkRequests.FindAsync(id);
        if (work == null) return NotFound();

        await _scheduler.CancelWork(work);
        return Ok();
    }
}