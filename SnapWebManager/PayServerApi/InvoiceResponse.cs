using SnapWebModels;

namespace TaskBoard.PayServerApi;

public class InvoiceResponse
{
    public string Amount { get; set; }
    public string Currency { get; set; }
    public InvoiceModel Data { get; set; }
}