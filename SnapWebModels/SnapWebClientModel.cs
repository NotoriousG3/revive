using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnapWebModels;

public class SnapWebClientModel
{
    [Key] public string ClientId { get; set; }

    public string ApiKey { get; set; }
    public int MaxManagedAccounts { get; set; }
    
    // Maximum amount of parallel threads/tasks per work to run
    public int Threads { get; set; }
    
    // Maximum amount of parallel works to execute
    public int MaxTasks { get; set; }
    public int MaxAddFriendsUsers { get; set; }
    public DateTime AccessDeadline { get; set; }
    public ICollection<AllowedModules> AllowedModules { get; set; }
    public int AccountCooldown { get; set; }
    public ICollection<InvoiceModel> Invoices { get; set; }
    public long MaxQuotaMb { get; set; } = 1024;
    public SnapWebModuleId DefaultOS { get; set; } = SnapWebModuleId.Ios; 

    // Used for transferring values from the UI
    [NotMapped] public List<SnapWebModuleId> EnabledModules { get; set; } = new();

    public bool Validate()
    {
        if (MaxManagedAccounts == 0) return false;
        return true;
    }
}