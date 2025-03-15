using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class ProxyScrapeService : IHostedService, IDisposable
{
    private readonly ILogger<ProxyScrapeService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SnapchatActionRunner _runner;
    private Timer _checkProxyTimer;
    private int lastIndex = 0;
    private int proxyLength = 0;
    private int currentProxxyLength = 0;
    public ProxyScrapeService(IServiceScopeFactory scopeFactory, ILogger<ProxyScrapeService> logger, SnapchatActionRunner runner)
    {
        _runner = runner;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Dispose()
    {
        _checkProxyTimer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Proxy Scraping Service");
        _checkProxyTimer = new Timer(ScrapeProxies, cancellationToken, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        return Task.FromResult("Started Proxy Scraping Service");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Proxy Scraping Service");
        _checkProxyTimer?.Change(Timeout.Infinite, 0);
        return Task.FromResult("Stopped Proxy Scraping Service");
    }

    public async void ScrapeProxies(object? state)
    {
        try
        {
            await using var context =
                _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var settings = await context.AppSettings.FirstOrDefaultAsync();

            if (!settings.ProxyScraping) { return; }
            
            var proxyManager = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IProxyManager>();
            string[] proxies;
            WebProxy p;

            using var _client = new HttpClient();


            var request = _client.GetAsync("https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=10000&country=all&ssl=all&anonymity=all");
            
            var response = await request.Result.Content.ReadAsStreamAsync();
            Stream receiveStream = response;
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

            proxies = readStream.ReadToEnd().Split(new[]
                {
                    Environment.NewLine
                },
                StringSplitOptions.None
            );

            if (currentProxxyLength != proxies.Length)
            {
                currentProxxyLength = proxies.Length;
                lastIndex = 0;
            }

            Proxy P = new Proxy
            {
                Address = new Uri($"http://{proxies[lastIndex++]}")
            };
            
            if (proxyManager.GetProxyFromDatabase(P.Address, null, null).Result == null)
            {
                p = P.ToWebProxy();
                
                try
                {
                    if (lastIndex < proxies.Length)
                    {

                        var httpClientHandler = new HttpClientHandler
                        {
                            Proxy = P.ToWebProxy(),
                    };

                        using var _check_client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

                        var response2 = await _check_client.GetAsync("https://api.ipify.org/?format=json");


                        if ((int)response2.StatusCode == 200)
                        {
                            await proxyManager.AddProxy(P, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //await _logger.LogDebug(
                     //   $"Proxy {p.Address} has failed testing and so we continue to look for another proxy.");
                }
            }
        }
        catch (Exception ex)
        {
            //await _logger.LogDebug(
            //    $"{ex}");
        }
    }
}