using Newtonsoft.Json;
using SnapWebManager.Models;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard;

public class NoManagerSetException : Exception
{
    public NoManagerSetException() : base("No manager has been defined for this service. Please set a MANAGER_URL environment variable")
    {
    }
}

public class RemoteSettings : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemoteSettings> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer _checkSettingsTimer;

    private readonly string _remoteManagerUrl;

    public RemoteSettings(HttpClient httpClient, IServiceProvider provider, ILogger<RemoteSettings> logger)
    {
        _logger = logger;
        _serviceProvider = provider;
        _remoteManagerUrl = Environment.GetEnvironmentVariable("MANAGER_URL");
        if (_remoteManagerUrl == null) throw new NoManagerSetException();

        _httpClient = httpClient;
    }

    public void Dispose()
    {
        _checkSettingsTimer?.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Remote Settings service");
        _checkSettingsTimer = new Timer(GetRemoteUpdate, cancellationToken, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Remote Settings service");
        _checkSettingsTimer?.Change(Timeout.Infinite, 0);
    }

    private string ClientSettingsUrl(string clientId)
    {
        return $"{_remoteManagerUrl}/api/clientsettings/{clientId}";
    }

    private void UpdateModules(ApplicationDbContext context, AppSettings settings, ClientSettingsResponse clientSettings)
    {
        context.Entry(settings).Collection(s => s.EnabledModules).Load();

        // Update enabled modules
        var allowedModulesIds = clientSettings.AllowdModulesId.ToList();
        foreach (var module in settings.EnabledModules)
            if (!allowedModulesIds.Contains(module.ModuleId))
            {
                context.Remove(module);
                settings.EnabledModules.Remove(module);
            }

        var missingModules = allowedModulesIds.Where(a => !settings.EnabledModules.Select(e => e.ModuleId).Contains(a));
        foreach (var module in missingModules)
        {
            var enabledModule = new EnabledModule {ModuleId = module};
            settings.EnabledModules.Add(enabledModule);
        }
    }

    public async void GetRemoteUpdate(object? state)
    {
        // Fetch from remote
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var settingsTable = context.AppSettings?.ToList();
        var settings = settingsTable?.FirstOrDefault() ?? new AppSettings();

        _logger.LogInformation($"Fetching remote settings for Client ID: {AppSettings.ClientId}");

        HttpResponseMessage response;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ClientSettingsUrl(AppSettings.ClientId));
            response = await _httpClient.SendAsync(request);
        }
        catch (Exception e)
        {
            _logger.LogError($"An error ocurred while querying for remote settings. {e.Message}");
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Unable to fetch remote settings");
            return;
        }

        ClientSettingsResponse clientSettings;
        try
        {
            // Parse response
            var content = await response.Content.ReadAsStringAsync();
            clientSettings = JsonConvert.DeserializeObject<ClientSettingsResponse>(content);

            if (clientSettings == null)
                _logger.LogError("Remote settings returned a null object");
        }
        catch (Exception e)
        {
            _logger.LogError($"An error ocurred while parsing RemoteSettings response. {e.Message}");
            return;
        }

        // Update settings
        settings.ApiKey = clientSettings.ApiKey;
        settings.Threads = clientSettings.Threads;
        settings.MaxTasks = clientSettings.MaxTasks;
        settings.MaxManagedAccounts = clientSettings.MaxManagedAccounts;
        settings.AccessDeadline = clientSettings.AccessDeadline;
        settings.AccountCooldown = clientSettings.AccountCooldown;
        settings.MaxAddFriendsUsers = clientSettings.MaxAddFriendsUsers;
        settings.MaxQuotaMb = clientSettings.MaxQuotaMb;
        settings.DefaultOs = clientSettings.DefaultOs;

        UpdateModules(context, settings, clientSettings);

        try
        {
            // Now save to the DB
            if (settingsTable.Count == 0)
                context.AppSettings.Add(settings);
            else
                context.Update(settings);

            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"An error ocurred while trying to save settings to database. {e}");
        }
    }
}