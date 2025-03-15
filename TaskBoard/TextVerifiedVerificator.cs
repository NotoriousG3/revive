using TaskBoard.Models;
using _textverified;
using Microsoft.IdentityModel.Tokens;

namespace TaskBoard;

public class TextVerifiedActivator : IPhoneVerificator
{
    private readonly SnapchatActionRunner _runner;
    private readonly AppSettingsLoader _settingsLoader;

    public TextVerifiedActivator(AppSettingsLoader settingsLoader, SnapchatActionRunner runner)
    {
        _settingsLoader = settingsLoader;
        _runner = runner;
    }

    public async Task<ValidationStatus> TryVerification(SnapchatAccountModel account, Country country, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var settings = await _settingsLoader.Load();

        if (string.IsNullOrWhiteSpace(settings.SmsActivateApiKey)) return ValidationStatus.NotValidated;
        
        Api api = new Api(settings.TextVerifiedApiKey, account.Proxy.ToWebProxy());

        await api.getToken();
        
        var myOrder = await api.OrderNumber("64");

        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(10);

        await _runner.ChangePhone(account, myOrder.number, country.ISO, proxyGroup, cancellationToken); // and convert to 2 letter ISO code.
        
        while (true)
        {
            var myStatus = await api.CheckOrder(myOrder.id);
            
            if(myStatus.code.IsNullOrEmpty())
            {
                if (waitTime >= maxWaitTime)
                {
                    // We need to cancel the number we ordered before we exit
                    return ValidationStatus.FailedValidation;
                }

                await Task.Delay(waitTime, cancellationToken);
                waitTime = TimeSpan.FromSeconds(2 ^ ++attempts);
                continue;
            }

            await _runner.VerifyPhone(account, myStatus.code, proxyGroup, cancellationToken);
            
            return ValidationStatus.Validated;
        }
    }

    public static TextVerifiedActivator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TextVerifiedActivator>();
    }
}