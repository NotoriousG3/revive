namespace TaskBoard.Models;

public class ImportAccountWithGroupArguments
{
    public long UploadId { get; set; }
    public string? GroupName { get; set; }
    public long? GroupId { get; set; }
}