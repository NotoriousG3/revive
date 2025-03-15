using System.Text.RegularExpressions;
using _kopeechka;
using _kopeechka.Objects;
using TaskBoard.Models;

namespace TaskBoard;

public class KoopechkaSingleRequest
{
    private readonly AppSettings _settings;
    private readonly IProxyManager _proxyManager;

    private Api _api;
    private OrderRequest? _orderRequest;

    public string Address
    {
        get
        {
            if (_orderRequest == null) CreateRequest().ConfigureAwait(false);
            return _orderRequest?.mail ?? string.Empty;
        }
    }

    public string Id
    {
        get
        {
            if (_orderRequest == null) CreateRequest().ConfigureAwait(false);
            return _orderRequest?.id ?? string.Empty;
        }
    }

    public KoopechkaSingleRequest(AppSettings settings, IProxyManager proxyManager)
    {
        _api = new Api(settings.KopeechkaApiKey, null);
        _settings = settings;
        _proxyManager = proxyManager;
    }

    private async Task CreateRequest()
    {
        if (_orderRequest != null)
            throw new Exception(
                "Multiple usage of the same instance is not allowed. Please create a new KoopechkaSingleRequest instance");
        
        _orderRequest = await _api.GenerateEmail("snapchat.com", "hotmail.com,outlook.com,gmail.com,yahoo.com", "no_reply@snapchat.com",
            @"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*", "73", "Confirm Your Email Address");
    }
    
    private string GetSnapchatConfirmationLink(string body)
    {
        var regex = new Regex(@"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*");
        return regex.Match(body).Value;
    }

    public async Task<ValidationStatus> WaitForValidationEmail(WorkRequest work, SnapchatAccountModel account)
    {
        var attmpts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attmpts);
        var maxWaitTime = TimeSpan.FromMinutes(3);

        await account.SnapClient.ResendVerifyEmail();
        
        Console.WriteLine("Creating Kopeechka Verification Service.");
        
        while (true)
        {
            Console.WriteLine("Checking Kopeechka for Mail.");
            
            var orderResponse = await _api.FetchEmail("1", Id);

            Console.WriteLine($"Kopeechka Order Status: {orderResponse.status} {orderResponse.value}");
            
            if (waitTime >= maxWaitTime)
            {
                return ValidationStatus.FailedValidation;
            }
            
            waitTime = TimeSpan.FromSeconds(Math.Pow(2, ++attmpts));
            
            if (orderResponse.status.Equals("OK"))
            {
                var validationLink = GetSnapchatConfirmationLink(orderResponse.fullmessage);

                if (validationLink.Length > 0)
                {
                    Console.WriteLine(validationLink);
                    
                    return await account.SnapClient.ValidateEmail(validationLink, false);
                }
            }

            await Task.Delay(waitTime, work.CancellationTokenSource.Token);
        }
    }
}