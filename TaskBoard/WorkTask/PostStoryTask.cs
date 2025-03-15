using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class PostStoryTask: ActionWorkTask
{
    public PostStoryTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, PostStoryArguments arguments, SnapchatAccountModel account)
    {
        using var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        
        await Logger.LogDebug(work, "Starting PostStory task", account);

        try
        {
            if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
            
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
            
            await Runner.PostStoryLegacy(account, work.MediaFile.ServerPath, arguments.SwipeUpUrl, arguments.Mentioned,
                proxyGroup, work.CancellationTokenSource.Token);
            await scheduler.UpdateWorkAddPass(work);
            await Logger.LogInformation(work, $"Posted Story", account);
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
            await Scheduler.EndWork(work, WorkStatus.Error, "ProxyAuthRequiredException");
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

    public async Task Start(WorkRequest work, PostStoryArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}