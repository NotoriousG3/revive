using NuGet.Packaging;
using NumSharp.Utilities;
using TaskBoard.Models;

namespace TaskBoard;

public class AccountTracker
{
    private ConcurrentHashset<long> _usedAccounts = new();

    public void Track(IEnumerable<SnapchatAccountModel> accounts)
    {
        _usedAccounts.AddRange(accounts.Select(a => a.Id));
    }

    public void Track(SnapchatAccountModel account)
    {
        _usedAccounts.Add(account.Id);
    }

    public bool IsUsed(SnapchatAccountModel account)
    {
        return _usedAccounts.Contains(account.Id);
    }

    public bool UnTrack(SnapchatAccountModel account)
    {
        return _usedAccounts.Remove(account.Id);
    }

    public void UnTrack(IEnumerable<SnapchatAccountModel> accounts)
    {
        foreach (var account in accounts)
        {
            _usedAccounts.Remove(account.Id);
        }
    }

    public IEnumerable<SnapchatAccountModel> FilterTracked(IEnumerable<SnapchatAccountModel> accounts)
    {
        return accounts.Where(a => !IsUsed(a));
    }
}