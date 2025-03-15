namespace TaskBoard.Models;

public class TargetUserListViewModel
{
    public string Label { get; set; }
    public string ElementId { get; set; }
    public IEnumerable<TargetUser> Users { get; set; }
}