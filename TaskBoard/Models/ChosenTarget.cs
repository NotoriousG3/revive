using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;

public class ChosenTarget
{
    [Key]
    public long Id { get; set; }
    
    [ForeignKey("TargetUser")]
    public long TargetUserId { get; set; }
    public TargetUser TargetUser { get; set; }
    
    [ForeignKey("WorkRequest")]
    public long WorkId { get; set; }
    public WorkRequest Work { get; set; }
}