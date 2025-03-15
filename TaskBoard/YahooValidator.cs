using TaskBoard.Models;

namespace TaskBoard;

public class YahooValidator : EmailValidator, IEmailValidator
{
    public YahooValidator(ILogger<YahooValidator> logger, IProxyManager proxyManager) : base(logger, proxyManager)
    {
    }

    public async Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        return await ValidateEmail(account,"pop.mail.yahoo.com", 995, true, email, token);
    }

    public static YahooValidator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<YahooValidator>();
    }
}