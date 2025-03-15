using System.Text;
using Newtonsoft.Json;
using SnapWebModels;

namespace TaskBoard.PayServerApi;

public struct CheckOutField
{
    public string RedirectUrl { get; set; }

    public CheckOutField(string redirectUrl)
    {
        RedirectUrl = redirectUrl;
    }
}
public struct InvoiceParameters
{
    public double Amount { get; set; }
    public string Currency { get; set; }
    public string? ItemDesc { get; set; }
    public CheckOutField Checkout { get; set; }
}

public class PayServerClient
{
    private readonly IHttpClientFactory _clientFactory;

    private readonly Uri _invoicesUri = new($"{ServerUrl}/api/v1/stores/{StoreId}/invoices");
    private readonly ILogger<PayServerClient> _logger;

    public PayServerClient(IHttpClientFactory clientFactory, ILogger<PayServerClient> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    protected static string ServerUrl => Environment.GetEnvironmentVariable("JSNAP_SERVERURL") ?? "https://jsnap.llc";
    protected static string StoreId => Environment.GetEnvironmentVariable("JSNAP_STOREID") ?? "";
    protected static string Auth => Environment.GetEnvironmentVariable("JSNAP_PAYAUTH");

    public async Task<InvoiceModel> CreateInvoiceAsync(InvoiceParameters parameters)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _invoicesUri);
        request.Headers.Add("Authorization", Auth);
        var content = new StringContent(JsonConvert.SerializeObject(parameters, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}), Encoding.UTF8, "application/json");
        request.Content = content;

        var response = await SendRequest(request);
        return response;
    }

    public async Task<InvoiceModel> GetInvoiceAsync(string id)
    {
        var url = $"{_invoicesUri}/{id}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", Auth);

        var response = await SendRequest(request);
        return response;
    }

    private async Task<InvoiceModel> SendRequest(HttpRequestMessage request)
    {
        var client = _clientFactory.CreateClient(nameof(PayServerClient));
        var response = await client.SendAsync(request);

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            _logger.LogError($"Error when trying to execute request to {request.RequestUri}. {responseContent}");

        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<InvoiceModel>(responseContent);
    }
}