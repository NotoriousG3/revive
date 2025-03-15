using System.ComponentModel.DataAnnotations;
using SnapWebModels;

namespace SnapWebManager.Models;

public class ClientInvoice
{
    [Key] public long Id { get; set; }

    public SnapWebModuleId ModuleId { get; set; }
    public InvoiceModel InvoiceData { get; set; }
    public SnapWebClientModel Client { get; set; }
}