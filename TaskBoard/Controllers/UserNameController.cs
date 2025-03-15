using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/usernames")]
[ApiController]
public class UserNameController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly UserNameManager _usernameManger;
    private readonly ILogger<EmailController> _logger;
    private readonly UploadManager _uploadManager;

    public UserNameController(ApplicationDbContext context, UserNameManager usernameManager, ILogger<EmailController> logger, UploadManager uploadManager)
    {
        _context = context;
        _usernameManger = usernameManager;
        _logger = logger;
        _uploadManager = uploadManager;
    }

    [HttpPost("data")]
    public async Task<IActionResult> Index(DataTableAjaxModel model)
    {
        var results = SearchUtilities.SearchDataTablesEntities(model, _context.UserNames, (e => e.UserName.ToLowerInvariant().Contains(model.search.value)), out var filteredCount, out var totalCount);
        
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
    public async Task<IActionResult> Delete(long id)
    {
        var name = await _context.UserNames.FindAsync(id);

        if (name == null) return NotFound();

        _context.Remove(name);
        await _context.SaveChangesAsync();

        return OkApi("Deleted username", id);
    }

    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        UserNameUploadResult result;
        try
        {
            result = await _usernameManger.Import(file.ServerPath);
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
        var names = _context.UserNames.Where(e => e.Id != null);
        var count = names.Count();
        _context.RemoveRange(names);
        await _context.SaveChangesAsync();
        _usernameManger.Init();

        await _context.SaveChangesAsync();
        
        return OkApi($"Usernames purged: {count}");
    }
}