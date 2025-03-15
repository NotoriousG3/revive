using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Security;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class ProxyController : ApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IProxyManager _proxyManager;
    private readonly ILogger<ProxyController> _logger;
    private readonly UploadManager _uploadManager;

    public ProxyController(ApplicationDbContext context, IProxyManager proxyManager, ILogger<ProxyController> logger, UploadManager uploadManager)
    {
        _context = context;
        _proxyManager = proxyManager;
        _logger = logger;
        _uploadManager = uploadManager;
    }
    
    [HttpPost("data")]
    public async Task<IActionResult> Index(DataTableAjaxModel model)
    {
        var results = SearchUtilities.SearchDataTablesEntities(model, _context.Proxies
            , (e => e.Address.ToString().ToLowerInvariant().Contains(model.search.value) || e.User.ToLowerInvariant().Contains(model.search.value)), out var filteredCount, out var totalCount).ToArray();
        foreach (var p in results)
        {
            var g = _context.ProxyGroups.Where(g => g.Proxies.Contains(p));
            p.Groups = g.ToList();
        }
        
        return Ok(new DataTablesResponse
        {
            Draw = model.draw,
            RecordsFiltered = filteredCount,
            RecordsTotal = totalCount,
            Data = results
        });
    }
    
    [HttpPut("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProxy(int id, Proxy proxy)
    {
        if (id != proxy.Id) return BadRequestApi("Proxy not found");
        _context.Entry(proxy).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return OkApi("Proxy Updated");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostProxy(PostProxyData proxyData)
    {
        if (string.IsNullOrWhiteSpace(proxyData.GroupData.GroupName) && proxyData.GroupData.GroupId == 0)
        {
            return BadRequestApi("Group name cannot be empty");
        }
        
        try
        {
            var group = await GetOrCreateGroup(proxyData.GroupData);

            if (group == null)
            {
                return BadRequestApi(
                    $"The requested group was not found or could not be created with name {proxyData.GroupData.GroupName}");
            }

            var proxy = new Proxy()
            {
                Address = proxyData.Address,
                User = proxyData.User,
                Password = proxyData.Password
            };
                
            await _proxyManager.AddProxy(proxy, true, group?.Id);
        }
        catch (ArgumentException e)
        {
            return BadRequestApi(e.Message);
        }

        return OkApi();
    }

    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProxy(int id)
    {
        if (!await _proxyManager.DeleteProxy(id)) return NotFound();

        return OkApi("Proxy Deleted");
    }
    
    private async Task<ProxyGroup?> GetOrCreateGroup(GroupData groupData)
    {
        try
        {
            if (groupData.GroupId > 0)
            {
                return await _context.ProxyGroups.FindAsync(groupData.GroupId);
            }

            if (string.IsNullOrWhiteSpace(groupData.GroupName)) return null;

            var match = _context.ProxyGroups.Where(e => e.Name.ToLower() == groupData.GroupName.ToLower());

            if (await match.AnyAsync()) return await match.FirstAsync();

            // create a new group with the given name
            var newGroup = new ProxyGroup() { Name = groupData.GroupName, ProxyType = groupData.ProxyType };
            await _context.ProxyGroups.AddAsync(newGroup);
            await _context.SaveChangesAsync();
            return newGroup;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex}");
        }

        return null;
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ImportWithGroupArguments arguments)
    {
        if (arguments.UploadId == 0) return BadRequestApi("Invalid file id");
        var file = await _uploadManager.GetFile(arguments.UploadId);
        if (file == null) return BadRequestApi("The requested file does not exist. Please try again.");
        
        // fetch the appropriate group depending on name or id
        if (string.IsNullOrWhiteSpace(arguments.GroupData.GroupName) && arguments.GroupData.GroupId == 0)
        {
            return BadRequestApi("Group name cannot be empty");
        }
        
        var group = await GetOrCreateGroup(arguments.GroupData);

        UploadResult<ProxyLineProcessResult> result;
        try
        {
            result = await _proxyManager.Import(file.ServerPath, group?.Id ?? 0);
        }
        catch (LineParseErrorException e)
        {
            _logger.LogError(e.Message);
            return BadRequestApi(e.Message);
        }
        finally
        {
            await _uploadManager.DeleteFiles(new List<MediaFile>() { file });
        }

        return OkApi("", result);
    }
    
    [HttpPost("purge")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purge()
    {
        var deletedProxies = await _proxyManager.Purge();
        
        return OkApi($"Proxies purged: {deletedProxies}");
    }

    [HttpGet("export")]
    public Task<IActionResult> Export()
    {
        return Task.Run<IActionResult>(() =>
        {
            var lines = _context.Proxies.Select(a => a.ToExportString());
            var builder = new StringBuilder();
            foreach (var line in lines)
                builder.AppendLine(line);

            var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
            return File(content, "plain/text", "proxies.txt");
        });
    }
    
    [HttpGet("groups/{id}")]
    public async Task<IActionResult> GetProxyGroups(int id)
    {
        var proxy = await _context.Proxies.FindAsync(id);

        if (proxy == null) return NotFound("Proxy with id {id} could not be found");
        await _context.Entry(proxy).Collection(a => a.Groups).LoadAsync();
        
        // for some reason, just returning the groups yields errors. Return everything instead
        return OkApi(data: proxy);
    }

    [HttpPost("groups/add")]
    public async Task<IActionResult> AddProxyToGroup(AddProxyToGroupArguments args)
    {
        if (args.ProxyId == 0 || args.GroupId == 0) return BadRequestApi("Invalid ProxyId or GroupId");
        
        var proxy = await _context.Proxies.FindAsync(args.ProxyId);
        var group = await _context.ProxyGroups.FindAsync(args.GroupId);

        if (proxy == null) return NotFound($"Proxy with id {args.ProxyId} not found");
        if (group == null) return NotFound($"Group with id {args.GroupId} not found");

        await _context.Entry(proxy).Collection(e => e.Groups).LoadAsync();
        await _context.Entry(group).Collection(e => e.Proxies).LoadAsync();

        if (proxy.Groups.Any(g => g.Id == args.GroupId))
            return BadRequestApi($"Proxy is already part of group {group.Name}");
        
        if (proxy.Groups == null)
        {
            proxy.Groups = new List<ProxyGroup>() { group };
        }
        else
        {
            proxy.Groups.Add(group);
        }

        await _context.SaveChangesAsync();

        return OkApi(data: group);
    }
    
    [HttpPost("groups/remove")]
    public async Task<IActionResult> RemoveProxyFromGroup(RemoveProxyFromGroupArguments args)
    {
        if (args.ProxyId == 0 || args.GroupId == 0) return BadRequestApi("Invalid ProxyId or GroupId");
        var proxy = await _context.Proxies.FindAsync(args.ProxyId);

        if (proxy == null) return NotFound($"Proxy with id {args.ProxyId} not found");

        await _context.Entry(proxy).Collection(e => e.Groups).LoadAsync();

        var match = proxy.Groups.FirstOrDefault(g => g.Id == args.GroupId); 
        if (match == null)
            return BadRequestApi($"Could not find the indicated group");

        if (proxy.Groups.Count == 1)
            return BadRequestApi("Cannot remove last group of proxy");

        proxy.Groups.Remove(match);

        await _context.SaveChangesAsync();

        return OkApi();
    }
}