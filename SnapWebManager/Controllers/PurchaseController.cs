using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SnapWebManager.Data;
using SnapWebModels;
using TaskBoard.PayServerApi;

namespace SnapWebManager.Controllers;

/*public class PurchaseArguments
{
    public string RedirectUrl { get; set; }
}*/

[Route("api/[controller]")]
[ApiController]
public class PurchaseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PayServerClient _payServerClient;

    public PurchaseController(PayServerClient payServerClient, ApplicationDbContext context)
    {
        _payServerClient = payServerClient;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Index(PayServerPurchaseArguments arguments)
    {
        var client = await _context.Clients.FindAsync(arguments.ClientId);
        if (client == null) return NotFound($"Client {arguments.ClientId} not found");

        // Calculate the total
        double total = 0;
        var descriptionLines = new List<string>();
        foreach (var purchaseInfo in arguments.PurchaseInfo)
        {
            var moduleInfo = SnapWebModule.DefaultModules.FirstOrDefault(m => m.Id == purchaseInfo.ModuleId);

            total += moduleInfo.Price * purchaseInfo.Quantity;
            descriptionLines.Add($"{purchaseInfo.Quantity} x {moduleInfo.Id.ToString()}");
        }

        // Create a new invoice for this module and save to db now
        try
        {
            var invoice = await _payServerClient.CreateInvoiceAsync(new InvoiceParameters {Currency = "USD", ItemDesc = string.Join("\n", descriptionLines), Amount = total, Checkout = new CheckOutField(arguments.RedirectUrl)});
            invoice.PurchaseInfoString = JsonConvert.SerializeObject(arguments.PurchaseInfo);
            invoice.Client = client;
            invoice.Amount = total;

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(invoice);
        }
        catch (HttpRequestException e)
        {
            return BadRequest($"An error has occurred when creating the invoice: {e.Message}");
        }
    }
}