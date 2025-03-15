using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/[controller]")]
[ApiController]
public class EmailController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly EmailManager _emailManager;
    private readonly ILogger<EmailController> _logger;
    private readonly UploadManager _uploadManager;

    public EmailController(ApplicationDbContext context, EmailManager emailManager, ILogger<EmailController> logger, UploadManager uploadManager)
    {
        _context = context;
        _emailManager = emailManager;
        _logger = logger;
        _uploadManager = uploadManager;
    }

    [HttpPost("data")]
    public async Task<IActionResult> Index(DataTableAjaxModel model)
    {
        var results = SearchUtilities.SearchDataTablesEntities(model, _context.Emails.Include(e => e.Account), (e => e.Address.ToLowerInvariant().Contains(model.search.value)), out var filteredCount, out var totalCount);
        return Ok(new DataTablesResponse
        {
            Draw = model.draw,
            RecordsFiltered = filteredCount,
            RecordsTotal = totalCount,
            Data = results
        });
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var email = _context.Emails.Find(id);

        if (email == null) return NotFound();

        _context.Remove(email);
        await _context.SaveChangesAsync();

        return OkApi("Deleted email", id);
    }

    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        EmailUploadResult result;
        try
        {
            result = await _emailManager.Import(file.ServerPath);
        }
        catch (LineParseErrorException e)
        {
            _logger.LogError(e.Message);
            return BadRequestApi(e.Message);
        }
        finally
        {
            // We now delete the import file since we won't need it anymore
            await _uploadManager.DeleteFiles(new List<MediaFile>() { file });
        }

        return OkApi("", result);
    }
    
    [HttpPost("purge")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purge()
    {
        var emails = _context.Emails.Where(e => e.AccountId == null);
        var count = emails.Count();
        _context.RemoveRange(emails);
        await _context.SaveChangesAsync();
        _emailManager.Init();

        await _context.SaveChangesAsync();
        
        return OkApi($"E-mails purged: {count}");
    }
}