using SnapWebModels;

namespace TaskBoard.Models;

public class ApiKeyNotSetException : Exception
{
    public ApiKeyNotSetException() : base("An API KEY has not been set")
    {
    }
}

public class ClientIdNotSetException : Exception
{
    public ClientIdNotSetException() : base("A Client ID has not been set")
    {
    }
}

public class SettingsNotSavedException : Exception
{
    public SettingsNotSavedException() : base("A settings object has not been saved. Please set your configuration and press 'Save' in the website")
    {
    }
}

/// <summary>
///     Model containing settings modifying different processes of the website.
///     This is just one part of the system. Changes here need to also be reflected in the
///     javascript Settings class AND the AppSettingsController
/// </summary>
public class AppSettings
{
    public AppSettings()
    {
        Timeout = 100;
        Threads = 5;
        MaxTasks = 1;
        MaxRegisterAttempts = 10;
        MaxManagedAccounts = 100;
        AccountCooldown = 120;
        MaxAddFriendsUsers = 2000;
        MaxQuotaMb = 1024;
    }

    public long Id { get; set; }
    public string? ApiKey { get; set; }
    public static string? ClientId => Environment.GetEnvironmentVariable("SNAPWEB_CLIENTID");
    public string? FiveSimApiKey { get; set; }
    public string? TwilioApiKey { get; set; }
    public string? TextVerifiedApiKey { get; set; }
    public string? SmsActivateApiKey { get; set; }
    public string? SmsPoolApiKey { get; set; }
    public string? NamsorApiKey { get; set; }
    public string? KopeechkaApiKey { get; set; }
    public bool ProxyScraping { get; set; }
    public bool ProxyChecking { get; set; }
    public int MaxRegisterAttempts { get; set; }
    public int Threads { get; set; }
    public int MaxTasks { get; set; }
    public int Timeout { get; set; }
    public bool EnableDebug { get; set; }
    public bool EnableBandwidthSaver { get; set; }
    public bool EnableStealth { get; set; } = true;
    public int MaxManagedAccounts { get; set; }
    public int MaxAddFriendsUsers { get; set; }
    public long MaxQuotaMb { get; set; }
    public int AccountCooldown { get; set; }
    public DateTime? AccessDeadline { get; set; }
    public bool EnableWebRegister { get; set; }
    public ICollection<EnabledModule> EnabledModules { get; set; }
    public SnapWebModuleId? DefaultOs { get; set; } = SnapWebModuleId.Ios;
    public static readonly int MaxFriends = 2000;
    public int MaxCreatedAccounts => MaxManagedAccounts * 10;
    public int MaxRetries { get; set; } = 3;

    public static void Validate(AppSettings? settings)
    {
        if (settings == null) throw new SettingsNotSavedException();
        if (string.IsNullOrWhiteSpace(settings.ApiKey)) throw new ApiKeyNotSetException();
        if (string.IsNullOrWhiteSpace(ClientId)) throw new ClientIdNotSetException();
    }

    public static async Task<AppSettings> GetSettingsFromProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        var loader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
        return await loader.Load();
    }

    public UserSettings ToUserSettings()
    {
        return new UserSettings
        {
            EnableBandwidthSaver = EnableBandwidthSaver,
            EnableWebRegister = EnableWebRegister,
            EnableStealth = EnableStealth,
            Timeout = Timeout,
            FiveSimApiKey = FiveSimApiKey,
            SmsPoolApiKey = SmsPoolApiKey,
            NamsorApiKey = NamsorApiKey,
            KopeechkaApiKey = KopeechkaApiKey,
            TwilioApiKey = TwilioApiKey,
            TextVerifiedApiKey = TextVerifiedApiKey,
            SmsActivateApiKey = SmsActivateApiKey,
            ProxyScraping = ProxyScraping,
            ProxyChecking = ProxyChecking,
            MaxRetries = MaxRetries
        };
    }
}

/// <summary>
///     Settings that are able to be modified by the user
/// </summary>
public class UserSettings
{
    public bool EnableBandwidthSaver { get; set; }
    public bool EnableStealth { get; set; } = true;
    public int Timeout { get; set; } = 100;
    public string? FiveSimApiKey { get; set; }
    public string? SmsPoolApiKey { get; set; }
    public string? NamsorApiKey { get; set; }
    public string? KopeechkaApiKey { get; set; }
    public string? TwilioApiKey { get; set; }
    public string? TextVerifiedApiKey { get; set; }
    public string? SmsActivateApiKey { get; set; }
    public bool ProxyScraping { get; set; }
    public bool ProxyChecking { get; set; }
    public int MaxRetries { get; set; } = 3;
    public bool EnableWebRegister { get; set; }
}