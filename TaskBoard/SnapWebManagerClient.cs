using System.Text;
using Newtonsoft.Json;
using SnapWebModels;
using TaskBoard.Models;

namespace TaskBoard;

public class SnapWebManagerClient
{
    private static readonly string _remoteManagerUrl = Environment.GetEnvironmentVariable("MANAGER_URL");
    
    private static readonly string _purchaseUrl = $"{_remoteManagerUrl}/api/purchase";
    private static readonly string _modulesUrl = $"{_remoteManagerUrl}/api/snapwebmodules";
    private static readonly string _invoicesUrl = $"{_remoteManagerUrl}/api/invoices/{AppSettings.ClientId}";
    private static readonly string _sendGridInfoUrl = $"{_remoteManagerUrl}/api/sendgridinfo";

    private readonly IHttpClientFactory _clientFactory;

    public SnapWebManagerClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<InvoiceModel?> Purchase(AddonsCart cart, string redirectUrl)
    {
        var client = _clientFactory.CreateClient(nameof(SnapWebManagerClient));
        var message = new HttpRequestMessage(HttpMethod.Post, _purchaseUrl);
        var args = new PayServerPurchaseArguments {PurchaseInfo = cart.PurchaseInfo, ClientId = AppSettings.ClientId, RedirectUrl = redirectUrl};
        message.Content = new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(message);

        var content = await response.Content.ReadAsStringAsync();
        return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<InvoiceModel>(content);
    }
    
    public async Task<IEnumerable<SnapWebModule>?> GetModules() {
        var client = _clientFactory.CreateClient(nameof(SnapWebManagerClient));
        var message = new HttpRequestMessage(HttpMethod.Get, _modulesUrl);
        var response = await client.SendAsync(message);
        
        var content = await response.Content.ReadAsStringAsync();
        return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<IEnumerable<SnapWebModule>>(content);
    }

    public async Task<IEnumerable<InvoiceModel>?> GetInvoices()
    {
        var client = _clientFactory.CreateClient(nameof(SnapWebManagerClient));
        var message = new HttpRequestMessage(HttpMethod.Get, _invoicesUrl);
        var response = await client.SendAsync(message);
        
        var content = await response.Content.ReadAsStringAsync();
        return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<IEnumerable<InvoiceModel>>(content);
    }

    public async Task<SendGridInfo?> GetSendGridInfo()
    {
        var client = _clientFactory.CreateClient(nameof(SnapWebManagerClient));
        var message = new HttpRequestMessage(HttpMethod.Get, _sendGridInfoUrl);
        var response = await client.SendAsync(message);
        
        var content = await response.Content.ReadAsStringAsync();
        return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<SendGridInfo>(content);
    }
}