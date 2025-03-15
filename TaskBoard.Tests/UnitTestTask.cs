using TaskBoard.Models;
using TaskBoard.WorkTask;

namespace TaskBoard.Tests;

public class UnitTestTask: ActionWorkTask
{
    public UnitTestTask(WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider) : base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
    }

    internal async Task<SnapchatAccountModel[]?> PickAccounts(int accountsToUse, WorkRequest work)
    {
        return await base.PickAccounts(accountsToUse, work);
    }
}