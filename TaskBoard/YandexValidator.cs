using TaskBoard.Models;

namespace TaskBoard;

public class YandexValidator : EmailValidator, IEmailValidator
{
    public YandexValidator(ILogger<YandexValidator> logger, IProxyManager proxyManager) : base(logger, proxyManager)
    {
    }

    public async Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        return await ValidateEmail(account,"pop.yandex.ru", 995, true, email, token);
    }

    public static YandexValidator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<YandexValidator>();
    }
}