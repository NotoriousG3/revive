using System.Collections.Concurrent;
using System.Net;
using SnapchatLib;
using TaskBoard.Models;

namespace TaskBoard;

public struct ClientRequest
{
    public ISnapchatClient Client;
    public DateTime LastRequest;
}

public class SnapchatClientFactory : IDisposable
{
    private static readonly ConcurrentDictionary<string, ClientRequest> _clients = new();
    private readonly Timer _clientCleanupTimer;
    private readonly ILogger<SnapchatClientFactory> _logger;
    private readonly IProxyManager _proxyManager;
    private readonly IServiceProvider _serviceProvider;

    public SnapchatClientFactory(IServiceProvider serviceProvider, ILogger<SnapchatClientFactory> logger, IProxyManager proxyManager)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _proxyManager = proxyManager;
        _clientCleanupTimer = new Timer(CleanupClients, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
    }

    public void Dispose()
    {
        _clientCleanupTimer.Dispose();
    }

    /// <summary>
    ///     Clean up cached clients based on the last time they were accessed
    /// </summary>
    /// <param name="stateInfo"></param>
    private void CleanupClients(object? stateInfo)
    {
        var expiredClients = _clients.Where(r => DateTime.UtcNow - r.Value.LastRequest > TimeSpan.FromMinutes(15));
        var count = expiredClients.Count();
        if (count == 0) return;
        
        _logger.LogDebug($"Removing {count} clients from the cache");
        
        foreach (var expired in expiredClients)
        {
            // Make sure to remove the client from the dictionary 
            _clients.TryRemove(expired);
            expired.Value.Client.Dispose();
        }
        
        GC.Collect();
    }

    private async Task<WebProxy?> CreateWebProxy(Proxy? proxyInfo)
    {
        if (proxyInfo == null) return null;

        var proxy = new WebProxy(proxyInfo.Address);

        if (!string.IsNullOrWhiteSpace(proxyInfo.User) && !string.IsNullOrWhiteSpace(proxyInfo.Password))
            proxy.Credentials = new NetworkCredential(proxyInfo.User, proxyInfo.Password);

        return proxy;
    }

    public async Task UpdateProxy(ISnapchatClient client, SnapchatAccountModel account, ProxyGroup? proxyGroup = null)
    {
        var proxyInfo = await _proxyManager.Take(proxyGroup);
        var newProxy = await CreateWebProxy(proxyInfo);
        client.SnapchatConfig.Proxy = newProxy;

        // Since we changed the proxy, we want to save the last used one to the db
        account.Proxy = proxyInfo;
        // We need to set ProxyId for things to work fine
        account.ProxyId = proxyInfo.Id;
        account.WasModified = true;
    }
    
    public async Task UpdateProxy(SnapchatClient client, SnapchatAccountModel account, ProxyGroup? proxyGroup = null)
    {
        var proxyInfo = await _proxyManager.Take(proxyGroup);
        var newProxy = await CreateWebProxy(proxyInfo);
        client.SnapchatConfig.Proxy = newProxy;

        // Since we changed the proxy, we want to save the last used one to the db
        account.Proxy = proxyInfo;
        // We need to set ProxyId for things to work fine
        account.ProxyId = proxyInfo.Id;
        account.WasModified = true;
    }

    private async Task RefreshClientRequest(ClientRequest request, SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken token = default)
    {
        // We update the request last access field and the return the stored client
        request.LastRequest = DateTime.UtcNow;
        _clients[account.Username] = request;
        _logger.LogInformation($"Using cached client for {account.Username}");

        // If our client is in the middle of being initialized, then wait for that to finish
        await request.Client.WaitForInitClient(token);
        
        // Maybe our client now has a proxy that should not be used anymore. So let's check that and update if required
        var changeProxyGroup = false;
        
        if (proxyGroup != null)
        {
            // We flag that we want to change the proxy when the group we received is not part of its groups list already.
            // So, if proxy has group A and B, and our proxy group is C, we change.
            // If proxy has group A and our proxy group is A, then we don't change.
            // If proxy has group A and B and our proxy group is B, then we don't change...?
            if (account.Proxy?.Groups != null)
                changeProxyGroup = account.Proxy?.Groups.FirstOrDefault(g => g.Id == proxyGroup.Id) == null;
        }
        
        if (!await _proxyManager.IsValidProxy(request.Client) || changeProxyGroup) await UpdateProxy(request.Client, account, proxyGroup);
    }

    private SnapchatVersion checkVersion(CreateSnapchatClientOptions options, SnapchatVersion snapchatVersion)
    {
        switch (snapchatVersion)
        {
            case SnapchatVersion.V12_26_0_20:
                return SnapchatVersion.V12_26_0_20;
            default:
                return SnapchatVersion.V12_27_0_8;
        }
    }
    
    /// <summary>
    ///     Create a ISnapchatClient instance. If a client for the provided account already exists, then one from the
    ///     cache is returned instead
    /// </summary>
    /// <param name="options">Options and data used for creation of the client</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ISnapchatClient> Create(CreateSnapchatClientOptions options, SnapchatVersion snapchatVersion, CancellationToken cancellationToken = default, bool initClient = true)
    {
        ClientRequest request;
        
        if (options.Account != null && _clients.TryGetValue(options.Account.Username, out request))
        {
            await RefreshClientRequest(request, options.Account, options.ProxyGroup, cancellationToken);
            return request.Client;
        }

        var settings = await AppSettings.GetSettingsFromProvider(_serviceProvider);
        Proxy proxyData;

        if (options.Account == null)
        {
            proxyData = await _proxyManager.Take(options.ProxyGroup);
        }
        else
        {
            proxyData = options.Account.Proxy;
        }

        var proxy = await CreateWebProxy(proxyData);

        var config = new SnapchatConfig
        {
            Proxy = proxy,
            ApiKey = settings.ApiKey,
            //SnapchatVersion = checkVersion(options,snapchatVersion),
            SnapchatVersion = snapchatVersion,
            Debug = true,
            Timeout = settings.Timeout,
            BandwithSaver = settings.EnableBandwidthSaver,
            OS = options.OS,
            StealthMode = settings.EnableStealth
        };

        /*if (AppSettings.ClientId != null && (AppSettings.ClientId.Equals("GaySex") || AppSettings.ClientId.Equals("thunderz") || AppSettings.ClientId.Equals("goltred")))
        {
            config.Debug = true;
        }*/

        var client = new SnapchatClient(config);

        if (options.Account == null) return new SnapchatClientWrapper(client);
        
        _logger.LogInformation($"Creating client for {options.Account.Username}");
        client.SnapchatConfig.user_id = options.Account.UserId;
        client.SnapchatConfig.AuthToken = options.Account.AuthToken;
        client.SnapchatConfig.Device = options.Account.Device;
        client.SnapchatConfig.Install = options.Account.Install;
        client.SnapchatConfig.Username = options.Account.Username;
        client.SnapchatConfig.SnapchatVersion = checkVersion(options,options.Account.SnapchatVersion);
        client.SnapchatConfig.OS = options.Account.OS;
        client.SnapchatConfig.dtoken1i = options.Account.DToken1I;
        client.SnapchatConfig.dtoken1v = options.Account.DToken1V;
        client.SnapchatConfig.install_time = options.Account.InstallTime;
        client.SnapchatConfig.DeviceProfile = options.Account.DeviceProfile;
        client.SnapchatConfig.Access_Token = options.Account.AccessToken;
        client.SnapchatConfig.BusinessAccessToken = options.Account.BusinessAccessToken;
        client.SnapchatConfig.AccountCountryCode = options.Account.AccountCountryCode;
        client.SnapchatConfig.Horoscope = options.Account.Horoscope;
        client.SnapchatConfig.TimeZone = options.Account.TimeZone;
        client.SnapchatConfig.ClientID = options.Account.ClientID;
        client.SnapchatConfig.Age = options.Account.Age;
        client.SnapchatConfig.refreshToken = options.Account.refreshToken;

        var changeProxyGroup = false;
        if (options.ProxyGroup != null)
        {
            // We flag that we want to change the proxy when the group we received is not part of its groups list already.
            // So, if proxy has group A and B, and our proxy group is C, we change.
            // If proxy has group A and our proxy group is A, then we don't change.
            // If proxy has group A and B and our proxy group is B, then we don't change...?
            if (options.Account.Proxy?.Groups != null)
                changeProxyGroup = options.Account.Proxy?.Groups.FirstOrDefault(g => g.Id == options.ProxyGroup.Id) == null;
        }
        if (!await _proxyManager.IsValidProxy(client) || changeProxyGroup) await UpdateProxy(client, options.Account, options.ProxyGroup);
        
        //var credentials = client.SnapchatConfig.Proxy.Credentials.GetCredential(new Uri(client.SnapchatConfig.Proxy.Address.AbsoluteUri), "Basic");

        //var User = credentials?.UserName;
        //var Password = credentials?.Password;
        
        //_logger.LogInformation($"Using: {client.SnapchatConfig.Proxy.Address}/{User}/{Password}");
        
        var wrapper = new SnapchatClientWrapper(client);

        // We check and update the proxy if required
        //if (!await _proxyManager.IsValidProxy(wrapper)) await UpdateProxy(wrapper, options.Account, options.ProxyGroup);

        var scope = _serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SnapchatActionRunner>();

        // Save the client in the cache
        _clients.TryAdd(options.Account.Username, new ClientRequest { Client = wrapper, LastRequest = DateTime.UtcNow });
        
        // Now start initialization
        if(initClient)
            await runner.InitClient(wrapper, options.Account, options.ProxyGroup, cancellationToken);

        await _proxyManager.ReturnProxyLoan(options.Account.Proxy);

        return wrapper;
    }
}