using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;
public class NameModel : IDisposable
{
    [NotMapped] public NameManager? NameManager;

    [Key] public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}