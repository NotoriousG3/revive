using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

public enum EmailLineProcessStatus
{
    Ok,
    UnknownError,
    Duplicated,
    EmptyEmail,
    InvalidEmail
}

public struct EmailLineProcessResult
{
    public EmailLineProcessStatus Status;
    public EmailListModel? PWord;
    public int LineNumber;
}

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class EmailScrapeController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailScrapeController> _logger;
    private readonly UploadManager _uploadManager;
    private readonly EmailManager _emailManager;

    public EmailScrapeController(ApplicationDbContext context, ILogger<EmailScrapeController> logger, UploadManager uploadManager, EmailManager emailManager)
    {
        _context = context;
        _logger = logger;
        _uploadManager = uploadManager;
        _emailManager = emailManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return OkApi("", await _context.EmailList.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostEmail(EmailListModel email)
    {
        try
        {
            EmailListModel.Validate(email);

            var emails = _context.EmailList.FirstOrDefault(u =>
                    string.Compare(u.Address, email.Address, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null;
            if (emails)
                return BadRequestApi($"Email \"{email.Address}\" already exists");
            
            _context.Add(email);
            await _context.SaveChangesAsync();
        }
        catch (ArgumentException e)
        {
            return BadRequestApi(e.Message);
        }

        return OkApi("", await _context.EmailList.ToListAsync());
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEmail(long id)
    {
        var entity = await _context.EmailList.FindAsync(id);

        if (entity == null) return NotFound();
        _context.Remove(entity);
        await _context.SaveChangesAsync();

        return OkApi("Email Deleted", await _context.EmailList.ToListAsync());
    }

    public async Task<UploadResult<EmailLineProcessResult>> ImportProcess(string filePath)
    {
        await using var stream = System.IO.File.OpenRead(filePath);
        var processResults = new List<EmailLineProcessResult>();
        var reader = new StreamReader(stream);
        var added = new Dictionary<string, EmailListModel>();
        Dictionary<string, EmailListModel> currentEmails = new();
            
        foreach(var email in _context.EmailList)
        {
            if(!currentEmails.ContainsKey(email.Address))
                currentEmails.Add(email.Address, email);
        }
        
        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            var result = new EmailLineProcessResult() {LineNumber = lineNumber};

            try
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Status = EmailLineProcessStatus.EmptyEmail;
                    processResults.Add(result);
                    continue;
                }

                var email = new EmailListModel() {Address = line};

                if (!EmailListModel.validCharactersRegex.IsMatch(email.Address))
                {
                    result.Status = EmailLineProcessStatus.InvalidEmail;
                    processResults.Add(result);
                    continue;
                }
                
                var exists = currentEmails.ContainsKey(email.Address) || added.ContainsKey(email.Address); 
                if (exists)
                {
                    result.Status = EmailLineProcessStatus.Duplicated;
                    processResults.Add(result);
                    continue;
                }

                processResults.Add(result);
                added.Add(email.Address, email);
            }
            catch (Exception)
            {
                result.Status = EmailLineProcessStatus.UnknownError;
            }
        }

        if (added.Count > 0)
        {
            _context.EmailList.AddRange(added.Values);
            await _context.SaveChangesAsync();
        }

        return new UploadResult<EmailLineProcessResult>() { Results = processResults };
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        UploadResult<EmailLineProcessResult> result;
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
        var users = await _context.EmailList.ToListAsync();
        var count = users.Count;

        _context.RemoveRange(users);
        await _context.SaveChangesAsync();
        
        return OkApi($"Emails purged: {count}");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var users = await _context.EmailList.ToListAsync();

        var lines = users.Select(a => a.ToExportString());
        var builder = new StringBuilder();
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        return File(content, "plain/text", "emails.txt");
    }
}