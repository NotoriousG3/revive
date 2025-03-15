namespace TaskBoard.Models;

public class AccountGroupChanges
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? SelectedAccounts { get; set; }
}