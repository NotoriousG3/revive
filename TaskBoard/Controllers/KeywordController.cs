using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

public enum KeywordLineProcessStatus
{
    Ok,
    UnknownError,
    Duplicated,
    EmptyKeyword,
    InvalidKeyword
}

public struct KeywordLineProcessResult
{
    public KeywordLineProcessStatus Status;
    public Keyword? KWord;
    public int LineNumber;
}

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class KeywordController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KeywordController> _logger;
    private readonly UploadManager _uploadManager;
    private readonly KeywordManager _keywordManager;

    public KeywordController(ApplicationDbContext context, ILogger<KeywordController> logger, UploadManager uploadManager, KeywordManager keywordManager)
    {
        _context = context;
        _logger = logger;
        _uploadManager = uploadManager;
        _keywordManager = keywordManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return OkApi("", await _context.Keywords.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostKeyword(Keyword word)
    {
        try
        {
            Keyword.Validate(word);

            var keywords = await _context.Keywords.ToListAsync();
            if (keywords.FirstOrDefault(u =>
                    string.Compare(u.Name, word.Name, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null)
                return BadRequestApi($"Keyword \"{word.Name}\" already exists");
            
            _context.Add(word);
            await _context.SaveChangesAsync();
        }
        catch (ArgumentException e)
        {
            return BadRequestApi(e.Message);
        }

        return OkApi("", await _context.Keywords.ToListAsync());
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteKeyword(long id)
    {
        var entity = await _context.Keywords.FindAsync(id);

        if (entity == null) return NotFound();
        _context.Remove(entity);
        await _context.SaveChangesAsync();

        return OkApi("Keyword Deleted", await _context.Keywords.ToListAsync());
    }

    public async Task<UploadResult<KeywordLineProcessResult>> ImportProcess(string filePath)
    {
        await using var stream = System.IO.File.OpenRead(filePath);
        var processResults = new List<KeywordLineProcessResult>();
        var reader = new StreamReader(stream);
        var added = new Dictionary<string, Keyword>();
        Dictionary<string, Keyword> currentUsers = new();
            
        foreach(var person in _context.Keywords)
        {
            if(!currentUsers.ContainsKey(person.Name))
                currentUsers.Add(person.Name, person);
        }
        
        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            var result = new KeywordLineProcessResult() {LineNumber = lineNumber};

            try
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Status = KeywordLineProcessStatus.EmptyKeyword;
                    processResults.Add(result);
                    continue;
                }

                var word = new Keyword() {Name = line};

                if (Keyword.validCharactersRegex.IsMatch(word.Name))
                {
                    result.Status = KeywordLineProcessStatus.InvalidKeyword;
                    processResults.Add(result);
                    continue;
                }
                
                var exists = currentUsers.ContainsKey(word.Name) || added.ContainsKey(word.Name); 
                if (exists)
                {
                    result.Status = KeywordLineProcessStatus.Duplicated;
                    processResults.Add(result);
                    continue;
                }

                processResults.Add(result);
                added.Add(word.Name, word);
            }
            catch (Exception)
            {
                result.Status = KeywordLineProcessStatus.UnknownError;
            }
        }

        if (added.Count > 0)
        {
            _context.Keywords.AddRange(added.Values);
            await _context.SaveChangesAsync();
        }

        return new UploadResult<KeywordLineProcessResult>() { Results = processResults };
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        UploadResult<KeywordLineProcessResult> result;
        try
        {
            result = await ImportProcess(file.ServerPath);
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
        var users = await _context.Keywords.ToListAsync();
        var count = users.Count;

        _context.RemoveRange(users);
        await _context.SaveChangesAsync();
        
        return OkApi($"Keywords purged: {count}");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var users = await _context.Keywords.ToListAsync();

        var lines = users.Select(a => a.ToExportString());
        var builder = new StringBuilder();
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        return File(content, "plain/text", "keywords.txt");
    }
}