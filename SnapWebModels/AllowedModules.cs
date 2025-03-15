using System.ComponentModel.DataAnnotations;

namespace SnapWebModels;

public class AllowedModules
{
    [Key] public long Id { get; set; }

    public SnapWebClientModel Client { get; set; }
    public SnapWebModuleId ModuleId { get; set; }
}