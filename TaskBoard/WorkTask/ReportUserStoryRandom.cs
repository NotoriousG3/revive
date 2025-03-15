using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class ReportUserStoryRandomTask: ActionWorkTask
{
    public ReportUserStoryRandomTask(WorkScheduler scheduler, WorkLogger logger, SnapchatActionRunner runner, SnapchatAccountManager accountManager, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, ReportUserStoryRandomArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting Report User Story Random task", account);

        var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();

        try
        {
            if (work.CancellationTokenSource.IsCancellationRequested)
                return WorkStatus.Cancelled;
            
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
            
            await Runner.ReportUserStoryRandom(account, arguments.Username, proxyGroup, work.CancellationTokenSource.Token);
            await scheduler.UpdateWorkAddPass(work);
            await Logger.LogInformation(work, $"Reported {arguments.Username} story.", account);
            return WorkStatus.Ok;
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                if (e is not NoAvailableProxyException)
                {
                    // If we don't handle this, then the exception is not caught :(
                    Logger.LogError(work, e, account).Wait(work.CancellationTokenSource.Token);
                    scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
                    return true;
                }
                
                Logger.LogError(work, "No proxies found").Wait(work.CancellationTokenSource.Token);
                scheduler.EndWork(work, WorkStatus.Error, "No proxies.").Wait();
                work.CancellationTokenSource.Cancel();
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
    
    public async Task Start(WorkRequest work, ReportUserStoryRandomArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}