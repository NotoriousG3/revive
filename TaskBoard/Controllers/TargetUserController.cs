using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Asn1.X509;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

public enum TargetUserLineProcessStatus
{
    Ok,
    UnknownError,
    Duplicated,
    EmptyUser
}

public struct TargetUserLineProcessResult
{
    public TargetUserLineProcessStatus Status;
    public TargetUser? TargetUser;
    public int LineNumber;
}

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class TargetUserController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IProxyManager _proxyManager;
    private readonly ILogger<TargetUserController> _logger;
    private readonly UploadManager _uploadManager;

    public TargetUserController(ApplicationDbContext context, ILogger<TargetUserController> logger, UploadManager uploadManager)
    {
        _context = context;
        _logger = logger;
        _uploadManager = uploadManager;
    }

    private bool MarkAsSearched(TargetUser target, string searchValue)
    {
        if (target.Username.ToLowerInvariant().Contains(searchValue))
        {
            target.Searched = true;
            _context.SaveChanges();
            return true;
        }

        return false;
    }
    
    [HttpPost("data")]
    public async Task<IActionResult> Index(DataTableAjaxModel model)
    {
        var results = SearchUtilities.SearchDataTablesEntities(model, _context.TargetUsers, (e => MarkAsSearched(e,model.search.value)), out var filteredCount, out var totalCount);
        return Ok(new DataTablesResponse
        {
            Draw = model.draw,
            RecordsFiltered = filteredCount,
            RecordsTotal = totalCount,
            Data = results
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostUser(TargetUser targetUser)
    {
        try
        {
            TargetUser.Validate(targetUser);
            var users = await _context.TargetUsers.ToListAsync();
            var match = users.Any(e => string.Equals(e.Username, targetUser.Username, StringComparison.InvariantCultureIgnoreCase));
            if (match)
                return BadRequestApi($"Target user \"{targetUser.Username}\" already exists");
            
            _context.Add(targetUser);
            await _context.SaveChangesAsync();
        }
        catch (ArgumentException e)
        {
            return BadRequestApi(e.Message);
        }

        return OkApi();
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var entity = await _context.TargetUsers.FindAsync(id);

        if (entity == null) return NotFound();
        _context.Remove(entity);
        await _context.SaveChangesAsync();

        return OkApi("User Deleted");
    }

    public async Task<UploadResult<TargetUserLineProcessResult>> ImportProcess(string filePath)
    {
        await using var stream = System.IO.File.OpenRead(filePath);
        var processResults = new List<TargetUserLineProcessResult>();
        var reader = new StreamReader(stream);
        var added = new Dictionary<string, TargetUser>();
        
        //TODO: Fix this enumeration
        if (_context.TargetUsers != null)
        {
            Dictionary<string, TargetUser> currentUsers = new();
            
            foreach(var person in _context.TargetUsers)
            {
                if(!currentUsers.ContainsKey(person.Username))
                    currentUsers.Add(person.Username, person);
            }

            var lineNumber = 0;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                var fields = line.Split(':');

                if (fields.Length == 5)
                {
                    var result = new TargetUserLineProcessResult() {LineNumber = lineNumber};
            
                    TargetUser user;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(fields[0]))
                        {
                            result.Status = TargetUserLineProcessStatus.EmptyUser;
                            processResults.Add(result);
                            continue;
                        }

                        user = new TargetUser() {Username = fields[0], UserID = fields[1], CountryCode = fields[2], Gender = fields[3], Race = fields[4]};

                        var exists = currentUsers.ContainsKey(user.Username) || added.ContainsKey(user.Username); 
                        if (exists)
                        {
                            result.Status = TargetUserLineProcessStatus.Duplicated;
                            processResults.Add(result);
                            continue;
                        }

                        processResults.Add(result);

                        if (!exists)
                        {
                            added.Add(user.Username, user);
                        }
                    }
                    catch (Exception)
                    {
                        result.Status = TargetUserLineProcessStatus.UnknownError;
                    }
                }
                else
                {
                    var result = new TargetUserLineProcessResult() {LineNumber = lineNumber};
            
                    TargetUser user;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            result.Status = TargetUserLineProcessStatus.EmptyUser;
                            processResults.Add(result);
                            continue;
                        }

                        user = new TargetUser() {Username = line, UserID = "Unknown", CountryCode = "Unknown", Gender = "Unknown", Race = "Unknown"};

                        var exists = currentUsers.ContainsKey(user.Username) || added.ContainsKey(user.Username); 
                        if (exists)
                        {
                            result.Status = TargetUserLineProcessStatus.Duplicated;
                            processResults.Add(result);
                            continue;
                        }

                        processResults.Add(result);

                        if (!exists)
                        {
                            added.Add(user.Username, user);
                        }
                    }
                    catch (Exception)
                    {
                        result.Status = TargetUserLineProcessStatus.UnknownError;
                    }
                }
            }
        }

        if (added.Count > 0)
        {
            _context.TargetUsers?.AddRange(added.Values);
            await _context.SaveChangesAsync();
        }

        return new UploadResult<TargetUserLineProcessResult>() { Results = processResults };
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] long uploadId)
    {
        if (uploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(uploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");

        UploadResult<TargetUserLineProcessResult> result;
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
        var count = _context.TargetUsers.Count();

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM TargetUsers;");

        return OkApi($"Target users purged: {count}");
    }
    
    [HttpPost("purge_added")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeAdded()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM TargetUsers WHERE Added='1';");

        return OkApi($"Added target users purged");
    }

    [HttpPost("purge_filtered")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeFiltered(PurgeFilteredArguments arguments)
    {
        IQueryable<TargetUser> targets = _context.TargetUsers;
        int filterCount = 0;

        if (!arguments.CountryCode.Equals("0"))
        {
            targets = targets.Where(t => t.CountryCode != null && t.CountryCode.Equals(arguments.CountryCode));
        }
        
        if (!arguments.Gender.Equals("0"))
        {
            targets = targets.Where(t => t.Gender != null && t.Gender.Equals(arguments.Gender));
        }
        
        if (!arguments.Race.Equals("0"))
        {
            targets = targets.Where(t => t.Race != null && t.Race.Equals(arguments.Race));
        }
        
        if (!arguments.Added.Equals("0"))
        {
            targets = targets.Where(t => t.Added != null && t.Added.Equals(Convert.ToBoolean(arguments.Added)));
        }
        
        if (!arguments.Searched.Equals("0"))
        {
            targets = targets.Where(t => t.Searched != null && t.Searched.Equals(Convert.ToBoolean(arguments.Searched)));
        }

        filterCount = targets.Count();
        
        await targets.DeleteFromQueryAsync();

        return OkApi($"{filterCount} Filtered target users purged");
    }

    [HttpGet("export_filtered/{CountryCode}/{Gender}/{Race}/{Added}/{Searched}")]
    public async Task<IActionResult> Export(string CountryCode, string Gender, string Race, string Added, string Searched)
    {
        IQueryable<TargetUser> targets = _context.TargetUsers;
        int filterCount = 0;

        if (!CountryCode.Equals("0"))
        {
            targets = targets.Where(t => t.CountryCode != null && t.CountryCode.Equals(CountryCode));
        }
        
        if (!Gender.Equals("0"))
        {
            targets = targets.Where(t => t.Gender != null && t.Gender.Equals(Gender));
        }
        
        if (!Race.Equals("0"))
        {
            targets = targets.Where(t => t.Race != null && t.Race.Equals(Race));
        }
        
        if (!Added.Equals("0"))
        {
            targets = targets.Where(t => t.Added != null && t.Added.Equals(Convert.ToBoolean(Added)));
        }
        
        if (!Searched.Equals("0"))
        {
            targets = targets.Where(t => t.Searched != null && t.Searched.Equals(Convert.ToBoolean(Searched)));
        }
        
        var lines = targets.Select(a => a.ToExportString());
        var builder = new StringBuilder();
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        

        return File(content, "plain/text", "targetusers.txt");
    }
}