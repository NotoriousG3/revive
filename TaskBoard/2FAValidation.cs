using System.Net;
using System.Text.RegularExpressions;
using _kopeechka;
using _kopeechka.Objects;
using SnapchatLib;
using TaskBoard.Models;

namespace TaskBoard;

public class TwoFaSingleRequest
{
    private readonly AppSettings _settings;
    private readonly IProxyManager _proxyManager;

    public Api _api;
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
    
    public TwoFaSingleRequest(AppSettings settings, IProxyManager proxyManager)
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
        
        _orderRequest = await _api.GenerateEmail("snapchat.com", "OUTLOOK", "no_reply@snapchat.com",
            @"", "73", "Snapchat Login Verification Code");
    }

    private string GetSnapchatConfirmationLink(string body)
    {
        var regex = new Regex(@"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*");
        return regex.Match(body).Value;
    }

    public string GetConfirmCode(string body)
    {
        var regex = new Regex(@"rial,sans-serif;\"">\r\n                                ......");
        return regex.Match(body).Value.Replace("rial,sans-serif;\">\r\n                                ", "");
    }

    public async Task<SnapchatLib.Extras.ValidationStatus> WaitForValidationEmail(SnapchatClient snapClient)
    {
        var attmpts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attmpts);
        var maxWaitTime = TimeSpan.FromMinutes(2);

        while (true)
        {
            var orderResponse = await _api.FetchEmail("1", Id);

            if (orderResponse.status == "OK")
            {
                if (waitTime >= maxWaitTime)
                {
                    return SnapchatLib.Extras.ValidationStatus.FailedValidation;
                }

                waitTime = TimeSpan.FromSeconds(2 ^ ++attmpts);

                var validationLink = GetSnapchatConfirmationLink(orderResponse.fullmessage);

                if (validationLink.Length > 0)
                {
                    return await snapClient.ValidateEmail(validationLink, false);
                }
            }

            await Task.Delay(waitTime);
        }
    }

    public async Task<string> WaitForValidationCode(SnapchatClient snapClient)
    {
        var attmpts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attmpts);
        var maxWaitTime = TimeSpan.FromMinutes(2);

        while (true)
        {
            var orderResponse = await _api.FetchEmail("1", Id);

            if (orderResponse.status == "OK")
            {
                if (waitTime >= maxWaitTime)
                {
                    return null;
                }

                waitTime = TimeSpan.FromSeconds(2 ^ ++attmpts);

                var validationLink = GetConfirmCode(orderResponse.fullmessage);

                if (validationLink.Length > 0)
                {
                    return validationLink;
                }
            }

            await Task.Delay(waitTime);
        }
    }
}