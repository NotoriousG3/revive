using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models;

public class LogEntry
{
    [Key] public int Id { get; set; }

    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }

    public long WorkId { get; set; }
    public WorkRequest Work { get; set; }
    public DateTime Time { get; set; }
}