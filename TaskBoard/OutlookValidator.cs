using TaskBoard.Models;

namespace TaskBoard;

public class OutlookValidator : EmailValidator, IEmailValidator
{
    public OutlookValidator(ILogger<OutlookValidator> logger, IProxyManager proxyManager) : base(logger, proxyManager)
    {
    }

    public async Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        return await ValidateEmail(account,"outlook.office365.com", 995, true, email, token);
    }

    public static OutlookValidator FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<OutlookValidator>();
    }

    public object Validate(EmailModel account)
    {
        throw new NotImplementedException();
    }
}