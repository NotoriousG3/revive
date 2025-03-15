using System.ComponentModel.DataAnnotations.Schema;
using SnapWebModels;

namespace SnapWebManager.Models;

[NotMapped]
public class ClientSettingsResponse
{
    public ClientSettingsResponse()
    {
    }

    public ClientSettingsResponse(SnapWebClientModel client)
    {
        ClientId = client.ClientId;
        ApiKey = client.ApiKey ?? Environment.GetEnvironmentVariable("JSNAP_DEFAULT_APIKEY");
        MaxManagedAccounts = client.MaxManagedAccounts;
        Threads = client.Threads;
        MaxTasks = client.MaxTasks;
        MaxAddFriendsUsers = client.MaxAddFriendsUsers;
        AccessDeadline = client.AccessDeadline;
        MaxQuotaMb = client.MaxQuotaMb;
        DefaultOs = client.DefaultOS;

        // We only care about the Ids for our response. Otherwise we could end up with circular references on the json serialization due to
        // the ref back to client
        AllowdModulesId = client.AllowedModules.Select(a => a.ModuleId);

        AccountCooldown = client.AccountCooldown;
    }

    public string ClientId { get; set; }
    public string? ApiKey { get; set; }
    public int MaxManagedAccounts { get; set; }
    public int Threads { get; set; }
    public int MaxTasks { get; set; }
    public DateTime AccessDeadline { get; set; }
    public IEnumerable<SnapWebModuleId> AllowdModulesId { get; set; }
    public int AccountCooldown { get; set; }
    public int MaxAddFriendsUsers { get; set; }
    public long MaxQuotaMb { get; set; }
    public SnapWebModuleId DefaultOs { get; set; }
}