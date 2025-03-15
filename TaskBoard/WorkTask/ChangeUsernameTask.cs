using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class ChangeUsernameTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly SnapchatAccountManager _manager;
    public ChangeUsernameTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _runner = runner;
        _manager = accountManager;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, ChangeUsernameArguments arguments, SnapchatAccountModel account)
    {
        using var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        
        await Logger.LogDebug(work, "Starting ChangeUserName task", account);
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

        try
        {
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            if (await _runner.ChangeUsername(account, arguments.NewName, proxyGroup, work.CancellationTokenSource.Token))
            {
                await scheduler.UpdateWorkAddPass(work);

                account.Username = arguments.NewName;
                
                await _manager.UpdateAccount(account);
            }
            else
            {
                await scheduler.FailWorkAccount(work, account);
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
        catch (UnauthorizedAuthTokenException)
        {
            account.SetStatus(AccountManager, AccountStatus.BANNED);
            await Logger.LogInformation(work, "Selected account is banned.", account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        catch (RateLimitedException)
        {
            await Logger.LogInformation(work, "Check your proxies theres to few to actually post its causing SC to ratelimit your connection", account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        catch (ProxyTimeoutException)
        {
            await Logger.LogInformation(work, "Proxy Timed Out Retrying", account);
            return WorkStatus.Retry;
        }
        catch (BannedProxyForUploadException)
        {
            await Logger.LogInformation(work, "Proxy is banned from uploading change proxies we are stopping the job for this account", account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        catch (ProxyAuthRequiredException)
        {
            await Logger.LogInformation(work, "Proxy Auth Exception Cancelling job", account);
            await Scheduler.EndWork(work, WorkStatus.Error);
            work.CancellationTokenSource.Cancel();
            return WorkStatus.Error;
        }
        catch (UsernameNotFoundException)
        {
            await Logger.LogInformation(work, "User No Found Skipping", account);
            await Scheduler.UpdateWorkAddPass(work);
            return WorkStatus.Ok;
        }
        catch (Exception ex)
        {
            await Logger.LogError(work, ex, account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        finally
        {
            await AssignAccount(work, account);
        }
    }

    public async Task Start(WorkRequest work, ChangeUsernameArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}