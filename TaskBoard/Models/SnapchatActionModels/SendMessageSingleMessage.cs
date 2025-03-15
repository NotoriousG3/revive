namespace TaskBoard.Models.SnapchatActionModels;

public class SendMessageSingleMessage
{
    public float SecondsBeforeStart { get; set; }
    public string Message { get; set; }
    public bool IsLink { get; set; }
    public int AmountOfSnaps { get; set; }
}