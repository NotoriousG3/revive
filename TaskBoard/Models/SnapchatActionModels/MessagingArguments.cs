namespace TaskBoard.Models.SnapchatActionModels;

public abstract class MessagingArguments: ActionArguments
{
    public bool RandomUsers { get; set; }
    public int RandomTargetAmount { get; set; }
    public string? CountryFilter { get; set; }
    public string? GenderFilter { get; set; }
    public string? RaceFilter { get; set; }
    public List<string> Users { get; set; } = new();
    public bool FriendsOnly { get; set; }
    public int RotateLinkEvery { get; set; }
}