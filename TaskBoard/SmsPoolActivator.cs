using TaskBoard.Models;
using SMS.NET.Models.Enums;
using SmsPool_Unofficial_Libary;

namespace TaskBoard;

public class SmsPoolActivator : IPhoneVerificator
{
    private readonly SnapchatActionRunner _runner;
    private readonly AppSettingsLoader _settingsLoader;

    public SmsPoolActivator(AppSettingsLoader settingsLoader, SnapchatActionRunner runner)
    {
        _settingsLoader = settingsLoader;
        _runner = runner;
    }

    public async Task<ValidationStatus> TryVerification(SnapchatAccountModel account, Country country, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var settings = await _settingsLoader.Load();

        if (string.IsNullOrWhiteSpace(settings.SmsPoolApiKey)) return ValidationStatus.NotValidated;
        
        SMSClient _client = new SMSClient(settings.SmsPoolApiKey);
        
        var order = await _client.Order(country.SmsPoolId.ToString(), "846", ""); // we need to figure a way to determine countries via ui

        var number = order.number;
        
        await _runner.ChangePhone(account, number, country.ISO, proxyGroup, cancellationToken); // and convert to 2 letter ISO code.

        var code = string.Empty;

        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(3);
        
        while (string.IsNullOrWhiteSpace(code))
        {
            var orders = await _client.GetActiveOrders();
            code = orders?.Find(o => o.phonenumber == number)?.code ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(code)) break;
            if (waitTime >= maxWaitTime) return ValidationStatus.FailedValidation;

            await Task.Delay(waitTime);
            waitTime = TimeSpan.FromSeconds(2 ^ ++attempts);
        }

        await _runner.VerifyPhone(account, code, proxyGroup, cancellationToken);
        
        return ValidationStatus.Validated;
    }

    public static SmsPoolActivator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SmsPoolActivator>();
    }
}