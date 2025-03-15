using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class QuickAddTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly WorkLogger _logger;
    private readonly WorkScheduler _scheduler;
    private readonly SnapchatAccountManager _accountManager;
    private readonly SnapchatClientFactory _factory;
    
    public QuickAddTask(SnapchatClientFactory factory, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _accountManager = accountManager;
        _runner = runner;
        _logger = logger;
        _scheduler = scheduler;
        _factory = factory;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, QuickAddArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting QuickAdd task", account);

        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

        try
        {
            int added = 0;
            var info = await _runner.GetSuggestions(account, proxyGroup, work.CancellationTokenSource.Token);

            if (info == null) {
                await _scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok; // RETURN OK ?? THERES NO ERROR THUNDA
            }
            
            foreach (var entry in info.suggested_friend_results_v2.Distinct().ToList())
            {
                
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                var amount_of_friends = await CacheFriends(account, proxyGroup, work.CancellationTokenSource.Token);

                if(amount_of_friends != null)
                {
                    if (amount_of_friends.Count == 838)
                        return WorkStatus.Ok;
                }

                // A try inside of a try LOL
                await _runner.AddByQuickAdd(account, entry.userId, proxyGroup, work.CancellationTokenSource.Token);
                added++;
                await _logger.LogInformation(work,
                        $"{account.Username} adding friend {entry.mutable_username}. Added: {added}");
                await _accountManager.UpdateAccount(account);

                await TargetManager.Add(new TargetUser() { Username = entry.mutable_username, CountryCode = "Unknown", Race = "Unknown", Gender = "Unkown", Added = true, Used = false, Searched = false});

               await Task.Delay(TimeSpan.FromSeconds(arguments.AddDelay));
            }
            
            account.hasAdded = true;
            await _accountManager.UpdateAccount(account);

            await _logger.LogInformation(work,
                $"{account.Username} finished adding {added} friend(s).");
            
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
            account.SetStatus(AccountManager, AccountStatus.RATE_LIMITED);
            await Logger.LogInformation(work, "Max Accounts Added On This Account", account);
            await Scheduler.UpdateWorkAddPass(work);
            return WorkStatus.Ok;
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
            await Logger.LogError(work, "Has no suggested friends failing.", account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found"))
            {
                await Logger.LogError(work, "Has no suggested friends failing.", account);
                await Scheduler.FailWorkAccount(work, account);
                return WorkStatus.Ok;
            }
            
            await Logger.LogError(work, ex, account);
            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        finally
        {
            await AssignAccount(work, account);
        }
    }

    public async Task Start(WorkRequest work, QuickAddArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}