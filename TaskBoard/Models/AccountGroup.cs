using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models;

public class AccountGroup
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; }
    public virtual ICollection<SnapchatAccountModel> Accounts { get; set; }
}