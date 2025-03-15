using TaskBoard.Models;

namespace TaskBoard;

public interface IPhoneVerificator
{
    public Task<ValidationStatus> TryVerification(SnapchatAccountModel account, Country country, ProxyGroup? proxyGroup, CancellationToken cancellationToken);
}