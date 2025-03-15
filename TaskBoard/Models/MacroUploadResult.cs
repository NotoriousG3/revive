namespace TaskBoard.Models;

public class MacroUploadResult
{
    public IEnumerable<MacroModel> Added { get; set; }
    public IEnumerable<MacroModel> Duplicated { get; set; }
    public IEnumerable<MacroRejectedReason> Rejected { get; set; }
}