using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

public enum PhoneLineProcessStatus
{
    Ok,
    UnknownError,
    Duplicated,
    EmptyPhone,
    InvalidPhone
}

public struct PhoneLineProcessResult
{
    public PhoneLineProcessStatus Status;
    public PhoneListModel? PWord;
    public int LineNumber;
}

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class PhoneScrapeController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PhoneScrapeController> _logger;
    private readonly UploadManager _uploadManager;
    private readonly PhoneNumberManager _phoneManager;

    public PhoneScrapeController(ApplicationDbContext context, ILogger<PhoneScrapeController> logger, UploadManager uploadManager, PhoneNumberManager phoneManager)
    {
        _context = context;
        _logger = logger;
        _uploadManager = uploadManager;
        _phoneManager = phoneManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return OkApi("", await _context.PhoneList.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostPhone(PhoneListModel phone)
    {
        try
        {
            PhoneListModel.Validate(phone);

            var phones = await _context.PhoneList.ToListAsync();
            if (phones.FirstOrDefault(u =>
                    string.Compare(u.CountryCode, phone.CountryCode, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null && phones.FirstOrDefault(u =>
                    string.Compare(u.Number, phone.Number, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null)
                return BadRequestApi($"Phone \"{phone.Number}\" already exists");
            
            _context.Add(phone);
            await _context.SaveChangesAsync();
        }
        catch (ArgumentException e)
        {
            return BadRequestApi(e.Message);
        }

        return OkApi("", await _context.PhoneList.ToListAsync());
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhone(long id)
    {
        var entity = await _context.PhoneList.FindAsync(id);

        if (entity == null) return NotFound();
        _context.Remove(entity);
        await _context.SaveChangesAsync();

        return OkApi("Phone Deleted", await _context.PhoneList.ToListAsync());
    }

    public async Task<UploadResult<PhoneLineProcessResult>> ImportProcess(string filePath)
    {
        await using var stream = System.IO.File.OpenRead(filePath);
        var processResults = new List<PhoneLineProcessResult>();
        var reader = new StreamReader(stream);
        var added = new List<PhoneListModel>();
        var currentNumbers = await _context.PhoneList.ToListAsync();

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            var result = new PhoneLineProcessResult() {LineNumber = lineNumber};

            try
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Status = PhoneLineProcessStatus.EmptyPhone;
                    processResults.Add(result);
                    continue;
                }

                var phone = new PhoneListModel() {Number = line.Split(':')[1], CountryCode = line.Split(':')[0]};

                if (!PhoneListModel.validCharactersRegex.IsMatch(phone.Number))
                {
                    result.Status = PhoneLineProcessStatus.InvalidPhone;
                    processResults.Add(result);
                    continue;
                }

                var exists = currentNumbers.FirstOrDefault(u =>
                    string.Compare(u.CountryCode, phone.CountryCode, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null && currentNumbers.FirstOrDefault(u =>
                    string.Compare(u.Number, phone.Number, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null || added.FirstOrDefault(u =>
                    string.Compare(u.CountryCode, phone.CountryCode, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null && added.FirstOrDefault(u =>
                    string.Compare(u.Number, phone.Number, StringComparison.InvariantCultureIgnoreCase) ==
                    0) != null;
                if (exists)
                {
                    result.Status = PhoneLineProcessStatus.Duplicated;
                    processResults.Add(result);
                    continue;
                }

                processResults.Add(result);
                added.Add(phone);
            }
            catch (Exception)
            {
                result.Status = PhoneLineProcessStatus.UnknownError;
            }
        }

        if (added.Count > 0)
        {
            _context.PhoneList.AddRange(added);
            await _context.SaveChangesAsync();
        }

        return new UploadResult<PhoneLineProcessResult>() { Results = processResults };
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        UploadResult<PhoneLineProcessResult> result;
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
        var users = await _context.PhoneList.ToListAsync();
        var count = users.Count;

        _context.RemoveRange(users);
        await _context.SaveChangesAsync();
        
        return OkApi($"Phones purged: {count}");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var users = await _context.PhoneList.ToListAsync();

        var lines = users.Select(a => a.ToExportString());
        var builder = new StringBuilder();
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        return File(content, "plain/text", "phones.txt");
    }
}