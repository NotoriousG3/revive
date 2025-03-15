using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SnapWebModels;

public enum InvoiceStatus
{
    Unknown = 0,
    New = 1,
    Processing = 2,
    Settled = 3,
    Expired = 4,
    Invalid = 5
}

public enum SnapwebStatus
{
    Unprocessed,
    Processed
}

public class InvoiceModel
{
    public string Id { get; set; }
    public string Status { get; set; }
    public double Amount { get; set; }
    public SnapwebStatus SnapwebStatus { get; set; }

    public InvoiceStatus ParsedStatus => Status switch
    {
        "New" => InvoiceStatus.New,
        "Processing" => InvoiceStatus.Processing,
        "Settled" => InvoiceStatus.Settled,
        "Expired" => InvoiceStatus.Expired,
        "Invalid" => InvoiceStatus.Invalid,
        _ => InvoiceStatus.Unknown
    };
    
    public SnapWebClientModel Client { get; set; }
    public string PurchaseInfoString { get; set; }

    [NotMapped] public IEnumerable<PurchaseInfo> PurchaseInfos => JsonSerializer.Deserialize<PurchaseInfo[]>(PurchaseInfoString);
}