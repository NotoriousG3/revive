using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class FriendCleanerTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly WorkLogger _logger;
    private readonly WorkScheduler _scheduler;
    private readonly SnapchatAccountManager _accountManager;
    private readonly SnapchatClientFactory _factory;
    
    public FriendCleanerTask(SnapchatClientFactory factory, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _accountManager = accountManager;
        _runner = runner;
        _logger = logger;
        _scheduler = scheduler;
        _factory = factory;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, FriendCleanerArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting DeleteFriend task", account);
        
        var count = 0;
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
        
        try
        {
            var info = await _runner.SyncFriends(account, proxyGroup, work.CancellationTokenSource.Token);

            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            if (info == null) return 0;


            foreach (var entry in info.friends)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                if (entry.mutable_username != "teamsnapchat" || entry.mutable_username != account.Username)
                {
                    if (entry.type == (int)FriendsEnums.Mutual)
                    {
                        try
                        {
                            await _runner.RemoveFriend(account, entry.user_id, proxyGroup, work.CancellationTokenSource.Token);
                        }
                        finally
                        {
                            count++;
                            await _logger.LogInformation(work,
                                $"{account.Username} deleting friend {entry.mutable_username}. Deleted: {count}");
                            await _accountManager.UpdateAccount(account);

                            await Task.Delay(TimeSpan.FromSeconds(arguments.AddDelay));
                        }
                    }
                }
            }

            foreach (var entry in info.added_friends)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                if (entry.mutable_username != "teamsnapchat" || entry.mutable_username != account.Username)
                {

                    if (entry.type == (int)AddedFriendsEnums.Mutual)
                    {
                        try
                        {
                            await _runner.RemoveFriend(account, entry.user_id, proxyGroup, work.CancellationTokenSource.Token);
                        }
                        finally
                        {
                            count++;
                            await _logger.LogInformation(work,
                                $"{account.Username} deleting friend {entry.mutable_username}. Deleted: {count}");
                            await _accountManager.UpdateAccount(account);

                            await Task.Delay(TimeSpan.FromSeconds(arguments.AddDelay));
                        }
                    }
                }
            }
            
            await _accountManager.UpdateAccount(account);
            
            await _logger.LogInformation(work,
                $"{account.Username} finished deleting {count} friend(s).");
            
            await _scheduler.UpdateWorkAddPass(work);

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

    public async Task Start(WorkRequest work, FriendCleanerArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}