using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class RelogAccountTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly WorkLogger _logger;
    private readonly WorkScheduler _scheduler;
    private readonly SnapchatAccountManager _accountManager;
    
    public RelogAccountTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _accountManager = accountManager;
        _runner = runner;
        _logger = logger;
        _scheduler = scheduler;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, RelogAccountArguments arguments, SnapchatAccountModel account)
    {
        try
        {
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
            
            await Logger.LogInformation(work, "Starting RelogAccount work");
            
            if (work.CancellationTokenSource.IsCancellationRequested)
                return WorkStatus.Cancelled;
            
            if (await _runner.RelogAccount(account, work, _accountManager, _runner, _logger, proxyGroup))
            {
                await _scheduler.UpdateWorkAddPass(work);
            }
            else
            {
                await Logger.LogError(work, $"Account failed to login. (Status: {account.GetAccountStatus})", account);
                await _scheduler.FailWorkAccount(work, account);
                return WorkStatus.Error;
            }

            return WorkStatus.Ok;
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                ProcessAggregateException(e, work, account);
                return true;
            });

            return WorkStatus.Error;
        }
        catch (AccountBannedException)
        {
            account.SetStatus(AccountManager, AccountStatus.BANNED);
            await Logger.LogInformation(work, "Selected account is banned.", account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        catch (Exception ex)
        {
            await Logger.LogError(work, ex);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        finally
        {
            await AssignAccount(work, account);
        }
    }

    public async Task Start(WorkRequest work, RelogAccountArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}