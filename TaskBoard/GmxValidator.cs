using TaskBoard.Models;

namespace TaskBoard;

public class GmxValidator : EmailValidator, IEmailValidator
{
    public GmxValidator(ILogger<OutlookValidator> logger, IProxyManager proxyManager) : base(logger, proxyManager)
    {
    }

    public async Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        return await ValidateEmail(account,"pop.gmx.com", 995, true, email, token);
    }

    public static GmxValidator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<GmxValidator>();
    }
}