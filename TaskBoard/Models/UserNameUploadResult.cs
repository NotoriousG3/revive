namespace TaskBoard.Models;

public class UserNameUploadResult
{
    public IEnumerable<UserNameModel> Added { get; set; }
    public IEnumerable<UserNameModel> Duplicated { get; set; }
    public IEnumerable<UserNameRejectedReason> Rejected { get; set; }
}