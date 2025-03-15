using TaskBoard.Models;
using _5sim; 

namespace TaskBoard;

public class FiveSimVerificator : IPhoneVerificator
{
    private readonly SnapchatActionRunner _runner;
    private readonly AppSettingsLoader _settingsLoader;

    public FiveSimVerificator(AppSettingsLoader settingsLoader, SnapchatActionRunner runner)
    {
        _settingsLoader = settingsLoader;
        _runner = runner;
    }

    public async Task<ValidationStatus> TryVerification(SnapchatAccountModel account, Country country, ProxyGroup proxyGroup, CancellationToken cancellationToken)
    {
        var settings = await _settingsLoader.Load();

        if (string.IsNullOrWhiteSpace(settings.FiveSimApiKey)) return ValidationStatus.NotValidated;

        var api = new Api(settings.FiveSimApiKey, account.Proxy?.ToWebProxy()); // determine api key from ui input/database

        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(10);
        
        var simNumber = api.buyActivationNumber("snapchat", country.FiveSimId); // we need to figure a way to determine countries via ui
        
        var number = simNumber.phone;

        var cleanNumber = number.Remove(0, country.CodeLength); // remove country code for snap (ony works for country codes with 1 digit we need to measure the length of the number to determine how much to remove from the string)

        await _runner.ChangePhone(account, cleanNumber, country.ISO, proxyGroup, cancellationToken); // and convert to 2 letter ISO code.
        
        while (true)
        {
            var sms = api.checkNumber(simNumber.id)?.sms.FirstOrDefault()?.code;

            if (sms == null)
            {
                if (waitTime >= maxWaitTime)
                {
                    // We need to cancel the number we ordered before we exit
                    api.cancelNumber(simNumber.id);
                    return ValidationStatus.FailedValidation;
                }

                await Task.Delay(waitTime, cancellationToken);
                waitTime = TimeSpan.FromSeconds(2 ^ ++attempts);
                continue;
            }

            await _runner.VerifyPhone(account, sms, proxyGroup, cancellationToken);
            return ValidationStatus.Validated;
        }
    }

    public static FiveSimVerificator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FiveSimVerificator>();
    }
}