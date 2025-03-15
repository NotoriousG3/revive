namespace TaskBoard.Models;

public class EmailUploadResult
{
    public IEnumerable<EmailModel> Added { get; set; }
    public IEnumerable<EmailModel> Duplicated { get; set; }
    public IEnumerable<EmailRejectedReason> Rejected { get; set; }
}