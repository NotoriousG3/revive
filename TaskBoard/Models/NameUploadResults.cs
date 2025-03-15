namespace TaskBoard.Models;

public class NameUploadResult
{
    public IEnumerable<NameModel> Added { get; set; }
    public IEnumerable<NameModel> Duplicated { get; set; }
    public IEnumerable<NameRejectedReason> Rejected { get; set; }
}