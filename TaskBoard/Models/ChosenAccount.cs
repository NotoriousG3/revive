using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;

public class ChosenAccount
{
    [Key] public long Id { get; set; }
    
    [ForeignKey("SnapchatAccountModel")]
    public long AccountId { get; set; }
    public SnapchatAccountModel Account { get; set; }
    
    [ForeignKey("WorkRequest")]
    public long WorkId { get; set; }
    public WorkRequest Work { get; set; }
}