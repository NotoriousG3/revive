using System.ComponentModel.DataAnnotations;
using SnapWebModels;

namespace TaskBoard.Models;

public class EnabledModule
{
    [Key] public long Id { get; set; }

    public SnapWebModuleId ModuleId { get; set; }
}