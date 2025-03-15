using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/macros")]
[ApiController]
public class MacroController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly MacroManager _macroManager;
    private readonly ILogger<MacroController> _logger;
    private readonly UploadManager _uploadManager;

    public MacroController(ApplicationDbContext context, MacroManager macroManager, ILogger<MacroController> logger, UploadManager uploadManager)
    {
        _context = context;
        _macroManager = macroManager;
        _logger = logger;
        _uploadManager = uploadManager;
    }

    [HttpPost("data")]
    public async Task<IActionResult> Index(DataTableAjaxModel model)
    {
        var results = SearchUtilities.SearchDataTablesEntities(model, _context.Macros, (e => e.Text.ToLowerInvariant().Contains(model.search.value)), out var filteredCount, out var totalCount);
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
        var macro = _context.Macros.Find(id);

        if (macro == null) return NotFound();

        _context.Remove(macro);
        await _context.SaveChangesAsync();

        return OkApi("Deleted macro", id);
    }

    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        MacroUploadResult result;
        try
        {
            result = await _macroManager.Import(file.ServerPath);
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
        var macros = _context.Macros.Where(e => e.Id != null);
        var count = macros.Count();
        _context.RemoveRange(macros);
        await _context.SaveChangesAsync();
        await _macroManager.Init();

        await _context.SaveChangesAsync();
        
        return OkApi($"Macros purged: {count}");
    }
}