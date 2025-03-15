namespace TaskBoard.Models;

public class BannedAccountDeletionLog
{
    public long Id { get; set; }
    public DateTime DeletionTime { get; set; }
    public string Username { get; set; } = null!;
}