namespace TaskBoard.Models;

public class ProxyRejectedReason
{
    public string Proxy { get; set; }
    public string Reason { get; set; }
}

public class ProxyUploadResult
{
    public IEnumerable<Proxy> Added { get; set; }
    public IEnumerable<Proxy> Duplicated { get; set; }
    public IEnumerable<ProxyRejectedReason> Rejected { get; set; }
}