using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;
public class UserNameModel : IDisposable
{
    [NotMapped] public UserNameManager? UserNameManager;

    [Key] public long Id { get; set; }
    public string UserName { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}