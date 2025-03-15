using TaskBoard.Models;
using SMS.NET.Models.Enums;

namespace TaskBoard;

public class SmsActivateActivator : IPhoneVerificator
{
    private readonly SnapchatActionRunner _runner;
    private readonly AppSettingsLoader _settingsLoader;

    public SmsActivateActivator(AppSettingsLoader settingsLoader, SnapchatActionRunner runner)
    {
        _settingsLoader = settingsLoader;
        _runner = runner;
    }

    public async Task<ValidationStatus> TryVerification(SnapchatAccountModel account, Country country, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var settings = await _settingsLoader.Load();

        if (string.IsNullOrWhiteSpace(settings.SmsActivateApiKey)) return ValidationStatus.NotValidated;
        
        SMS.NET.SMS Sms = new SMS.NET.SMS(settings.SmsActivateApiKey);
        
        var myNumber = await Sms.Activation.GetNumber("fu", country.SmsActivateId); 
        string number = myNumber.ToString();

        var cleanNumber = number.Remove(0, country.CodeLength); // remove country code for snap (ony works for country codes with 1 digit we need to measure the length of the number to determine how much to remove from the string)

        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(10);

        await _runner.ChangePhone(account, cleanNumber, country.ISO, proxyGroup, cancellationToken); // and convert to 2 letter ISO code.
        
        while (true)
        {
            var myStatus = await myNumber.GetActivationStatus();
            
            if(myStatus.Key != ActivationStatus.STATUS_OK)
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

            await _runner.VerifyPhone(account, myStatus.Value, proxyGroup, cancellationToken);
            
            return ValidationStatus.Validated;
        }
    }

    public static SmsActivateActivator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SmsActivateActivator>();
    }
}