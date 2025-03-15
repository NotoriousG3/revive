namespace SnapWebModels;

public class PayServerPurchaseArguments
{
    public IEnumerable<PurchaseInfo> PurchaseInfo { get; set; }
    public string ClientId { get; set; }
    public string? RedirectUrl { get; set; }
}