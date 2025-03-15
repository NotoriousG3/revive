using TaskBoard.Models;

namespace TaskBoard;

public class GmailValidator : EmailValidator, IEmailValidator
{
    public GmailValidator(ILogger<GmailValidator> logger, IProxyManager proxyManager) : base(logger, proxyManager)
    {
    }

    public async Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        return await ValidateEmail(account,"pop.gmail.com", 995, true, email, token);
    }

    public static GmailValidator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<GmailValidator>();
    }
}