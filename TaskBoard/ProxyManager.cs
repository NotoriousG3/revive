using System.Net;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using NumSharp.Utilities;
using SnapchatLib;
using TaskBoard.Models;

namespace TaskBoard;

public enum ProxyLineProcessStatus
{
    Ok,
    UnknownError,
    Duplicated,
    IndexOutOfRange,
    InvalidUrl,
    EmptyAddress,
}

public struct ProxyLineProcessResult
{
    public ProxyLineProcessStatus Status;
    public Proxy? Proxy;
    public int LineNumber;
}

public class NoAvailableProxyException : Exception
{
    public NoAvailableProxyException() : base("There are no available proxies to use with SnapchatClients")
    {
    }
}

public class ProxyGroupNotFoundException : Exception
{
    public ProxyGroupNotFoundException(long id) : base($"A Proxy Group with ID {id} does not exist in the database") {}
}

public interface IProxyManager
{
    Task<IEnumerable<Proxy>> GetProxies();
    int GetProxieCount();
    Task AddProxy(Proxy proxy, bool saveToDb = true, long? groupId = 0L);
    Task<bool> DeleteProxy(int id);
    Task<Proxy> Take(ProxyGroup? group = null);
    Task<bool> IsValidProxy(ISnapchatClient client);
    Task<bool> IsValidProxy(SnapchatClient client);
    Task<Proxy?> GetProxyFromDatabase(Uri address, string username, string password);
    Task<Proxy?> GetProxyFromDatabase(WebProxy proxy);
    Task<UploadResult<ProxyLineProcessResult>> Import(string filePath, long groupId);
    Task<int> Purge();
    Task<int> PurgeByGroup(long id);
    Task ReturnProxyLoan(Proxy proxy);
}

public class ProxyManager : IProxyManager
{
    private readonly ILogger<ProxyManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private ConcurrentHashset<long> _loanedProxies = new();

    public ProxyManager(IServiceScopeFactory scopeFactory, ILogger<ProxyManager> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Proxy>> GetProxies()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if(context.Proxies == null)
        {
            return Enumerable.Empty<Proxy>();
        }

        return await context.Proxies.Include(p => p.Accounts).AsNoTracking().ToListAsync();
    }
    
    public int GetProxieCount()
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return context.Proxies?.Count() ?? 0;
    }

    public async Task AddProxy(Proxy proxy, bool saveToDb, long? groupId = 0)
    {
        var parts = proxy.Address.ToString().Split("://");

        if (parts.Length == 1)
        {
            // missing scheme
            proxy.Address = new Uri($"http://{proxy.Address}");
        }

        var currentEntry = await GetProxyFromDatabase(proxy.ToWebProxy());

        // Return for duplicated proxies
        if (currentEntry != null)
        {
            throw new ArgumentException("Proxy already exists");
        }

        Proxy.Validate(proxy);
            
        if (saveToDb)
        {
            await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
            List<ProxyGroup> proxyGroups = new List<ProxyGroup>();
            if (groupId > 0)
            {
                if (context.ProxyGroups != null)
                {
                    var group = await context.ProxyGroups.FindAsync(groupId);
                    if (group != null)
                            
                        proxyGroups.Add(group);

                    if (proxyGroups.Count > 0)
                    {
                        proxy.Groups = new List<ProxyGroup>();
                        proxy.Groups.AddRange(proxyGroups);
                    }
                }
            }
                
            await context.Proxies.AddAsync(proxy);
            await context.SaveChangesAsync();

            var g = proxyGroups.FirstOrDefault();
            if (g != null)
            {
                context.Attach(g);
                g.Proxies.Add(proxy);
                await context.SaveChangesAsync();
            }
        }
    }

    public async Task<Proxy?> GetProxyFromDatabase(Uri address, string username, string password)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Proxies == null)
            throw new Exception("GetProxyFromDatabase is null parse it properly");

        if (username == null && password == null)
        {
            return await context.Proxies.FirstOrDefaultAsync(p => p.Address == address);
        }
        
        return await context.Proxies.FirstOrDefaultAsync(p => p.Address == address && p.User == username && p.Password == password);
    }

    public async Task<Proxy?> GetProxyFromDatabase(WebProxy proxy)
    {
        if (proxy.Credentials == null)
        {
            return await GetProxyFromDatabase(proxy.Address, null, null);
        }
        
        var credentials = proxy.Credentials.GetCredential(proxy.Address, "Basic");
        return await GetProxyFromDatabase(proxy.Address, credentials.UserName, credentials.Password);
    }

    public async Task<bool> DeleteProxy(int id)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Proxies == null)
            return true;

        var proxy = await context.Proxies.FindAsync(id);
        if (proxy == null) return false;

        await context.Entry(proxy).Collection(p => p.Accounts).LoadAsync();

        foreach (var account in proxy.Accounts)
            account.Proxy = null;

        context.Proxies.Remove(proxy);
        await context.SaveChangesAsync();

        return true;
    }
    
    private async Task<Proxy?> FetchNextFree(ProxyGroup? group)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Proxy proxy = null;
        
        if (group == null)
        {
            proxy = await context.Proxies
                .Where(p => !_loanedProxies.Contains(p.Id))
                .OrderBy(k => k.LastUsed ?? DateTime.MinValue)
                .FirstOrDefaultAsync();
        
            if (proxy != null)
            {
                _loanedProxies.Add(proxy.Id);
            }

            return proxy;
        }

        var dbRecord = await context.ProxyGroups.Include(g => g.Proxies).ThenInclude(p => p.Accounts).FirstOrDefaultAsync(g => g.Id == group.Id);
        if (dbRecord == null) throw new ProxyGroupNotFoundException(group.Id);

        if (dbRecord.ProxyType == ProxyType.Rotating)
        {
            proxy = dbRecord.Proxies.MinBy(k => k.LastUsed ?? DateTime.MinValue);
            
            return proxy;
        }

        proxy = dbRecord.Proxies.Where(p => p.Accounts.Count == 0 && !_loanedProxies.Contains(p.Id)).MinBy(k => k.LastUsed ?? DateTime.MinValue);
    
        if (proxy != null)
        {
            _loanedProxies.Add(proxy.Id);
        }

        return proxy;
    }
    
    public async Task ReturnProxyLoan(Proxy proxy)
    {
        if (_loanedProxies.Contains(proxy.Id))
        {
            _logger.LogDebug($"Removing {proxy.Address} from loaned.");
            _loanedProxies.Remove(proxy.Id);
        }
    }

    public async Task<Proxy> Take(ProxyGroup? group = null)
    {
        Proxy proxy = null;

        _logger.LogDebug($"Taking proxy from database");

        proxy = await FetchNextFree(group);

        if (proxy == null) throw new NoAvailableProxyException();

        await UpdateProxyLastUsed(proxy);
        return proxy;
    }

    private async Task UpdateProxyLastUsed(Proxy proxy)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var latest = await context.Proxies.FindAsync(proxy.Id);
        await context.Entry(latest).Collection(p => p.Accounts).LoadAsync();
        latest.LastUsed = DateTime.UtcNow;
        context.Update(latest);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsValidProxy(ISnapchatClient client)
    {
        if (client.SnapchatConfig.Proxy == null) return false;

        // This means no proxies are set, even the one already in the client
        if (GetProxieCount() == 0)
            return false;

        // We recreate the proxy instance to leverage the equals method
        var proxy = new Proxy(client.SnapchatConfig.Proxy);

        // If the proxy is not in the db anymore, is not valid
        var dbProxy = await GetProxyFromDatabase(proxy.Address, proxy.User, proxy.Password);
        return dbProxy != null;
    }
    
    public async Task<bool> IsValidProxy(SnapchatClient client)
    {
        if (client.SnapchatConfig.Proxy == null) return false;

        // This means no proxies are set, even the one already in the client
        if (GetProxieCount() == 0)
            return false;

        // We recreate the proxy instance to leverage the equals method
        var proxy = new Proxy(client.SnapchatConfig.Proxy);

        // If the proxy is not in the db anymore, is not valid
        var dbProxy = await GetProxyFromDatabase(proxy.Address, proxy.User, proxy.Password);
        return dbProxy != null;
    }

    public async Task<UploadResult<ProxyLineProcessResult>> Import(string filePath, long groupId)
    {
        await using var stream = File.OpenRead(filePath);
        var processResults = new List<ProxyLineProcessResult>();
        var reader = new StreamReader(stream);
        var added = new Dictionary<string, Proxy>();
        
        // We append the group to the list, it should create the reference by itself.
        // And this needs to happen after the account is in the db, otherwise groups were duplicating themselves...
        List<ProxyGroup> proxyGroups = new List<ProxyGroup>();
        if (groupId > 0)
        {
            await using var groupContext = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var group = await groupContext.ProxyGroups.FindAsync(groupId);
            if (group != null)
                proxyGroups.Add(group);
        }

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            if (line != null)
            {
                var fields = line.Split(':');

                var result = new ProxyLineProcessResult() { LineNumber = lineNumber };
                
                Proxy proxy;
                try
                {
                    if (string.IsNullOrWhiteSpace(fields[0]))
                    {
                        result.Status = ProxyLineProcessStatus.EmptyAddress;
                        processResults.Add(result);
                        continue;
                    }

                    Uri address;
                    try
                    {
                        address = new Uri($"http://{fields[0]}:{fields[1]}");
                    }
                    catch (Exception)
                    {
                        result.Status = ProxyLineProcessStatus.InvalidUrl;
                        processResults.Add(result);
                        continue;
                    }

                    if (fields.Length > 2)
                    {
                        proxy = new Proxy
                        {
                            Address = address,
                            User = fields[2],
                            Password = fields[3]
                        };
                    }
                    else
                    {
                        proxy = new Proxy
                        {
                            Address = address
                        };
                    }

                    result.Proxy = proxy;

                    try
                    {
                        Proxy.Validate(proxy);
                    }
                    catch (ArgumentException)
                    {
                        result.Status = ProxyLineProcessStatus.InvalidUrl;
                        processResults.Add(result);
                        continue;
                    }

                    var proxyKey = proxy.ToExportString();
                    if (await GetProxyFromDatabase(proxy.Address, proxy.User, proxy.Password) != null || added.ContainsKey(proxyKey))
                    {
                        result.Status = ProxyLineProcessStatus.Duplicated;
                        processResults.Add(result);
                        continue;
                    }

                    processResults.Add(result);
                    added.Add(proxyKey, proxy);

                    // Add to the list of usable proxies
                }
                catch (IndexOutOfRangeException)
                {
                    result.Status = ProxyLineProcessStatus.IndexOutOfRange;
                }
            }
        }

        // do everthing db related at the end
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await context.Proxies.AddRangeAsync(added.Values);
        await context.SaveChangesAsync();

        var g = proxyGroups.FirstOrDefault();
        if (g != null)
        {
            context.Attach(g);
            g.Proxies = added.Values;
            await context.SaveChangesAsync();
        }

        return new UploadResult<ProxyLineProcessResult>() { Results = processResults };
    }

    public async Task<int> PurgeByGroup(long id)
    {
        Console.WriteLine($"Purging group id {id}");
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Proxies == null)
            return 0;

        var proxies = context.Proxies.Include(e => e.Accounts).Where(e => e.GroupId == id);
        var count = proxies.Count();

        foreach (var proxy in proxies)
        {
            // Break the link between account and proxy

            Console.WriteLine($"{proxy.Id} - {proxy.Address}");
            
            if(proxy.Accounts != null)
            {
                foreach (var snapchatAccountModel in proxy.Accounts)
                {
                    snapchatAccountModel.Proxy = null;
                    snapchatAccountModel.ProxyId = null;
                }

                context.Remove(proxy);
            }
        }
        
        context.ProxyGroups.RemoveRange(context.ProxyGroups);
        
        await context.SaveChangesAsync();

        return count;
    }
    
    public async Task<int> Purge()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Proxies == null)
            return 0;

        var proxies = context.Proxies.Include(e => e.Accounts);
        var count = proxies.Count();

        foreach (var proxy in proxies)
        {
            // Break the link between account and proxy

            if(proxy.Accounts != null)
            {

                foreach (var snapchatAccountModel in proxy.Accounts)
                {
                    snapchatAccountModel.Proxy = null;
                    snapchatAccountModel.ProxyId = null;
                }

                context.Remove(proxy);
            }
        }
        
        context.ProxyGroups.RemoveRange(context.ProxyGroups);
        
        await context.SaveChangesAsync();

        return count;
    }
}