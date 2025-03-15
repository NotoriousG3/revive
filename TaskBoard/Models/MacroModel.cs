using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;
public class MacroModel : IDisposable
{
    [NotMapped] public MacroModel? MacroManager;

    [Key] public long Id { get; set; }
    public string Text { get; set; }
    public string Replacement { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}