using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class ExportFriendTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly WorkLogger _logger;
    private readonly WorkScheduler _scheduler;
    private readonly SnapchatAccountManager _accountManager;
    private readonly SnapchatClientFactory _factory;

    public ExportFriendTask(SnapchatClientFactory factory, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _accountManager = accountManager;
        _runner = runner;
        _logger = logger;
        _scheduler = scheduler;
        _factory = factory;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, ExportFriendsArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting ExportFriend task", account);
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
        
        try
        {
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            var info2 = await _runner.SyncFriends(account, proxyGroup, work.CancellationTokenSource.Token);

            if (info2 == null) return 0;
            
            foreach (var entry2 in info2.friends)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                
                if (entry2.type == 2 || entry2.type == 3 || entry2.mutable_username == "teamsnapchat" ||
                    entry2.mutable_username == account.Username)
                {
                    continue;
                }
                
                if (entry2.type == 0)
                {
                    if (account != null)
                    {
                        if (work.ExportedFriends is { Length: > 0 })
                        {
                            work.ExportedFriends += $"<br />{entry2.mutable_username}";
                        }
                        else
                        {
                            work.ExportedFriends = $"{entry2.mutable_username}";
                        }
                    }
                    await Logger.LogInformation(work, $"Exported {entry2.mutable_username}", account);
                }
            }
            
            foreach (var entry2 in info2.added_friends)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (entry2.type == 2 || entry2.type == 3 || entry2.mutable_username == "teamsnapchat" ||
                    entry2.mutable_username == account.Username)
                {
                    continue;
                }
                
                if (entry2.type == 0)
                {
                    if (account != null)
                    {
                        if (work.ExportedFriends is { Length: > 0 })
                        {
                            work.ExportedFriends += $"<br />{entry2.mutable_username}";
                        }
                        else
                        {
                            work.ExportedFriends = $"{entry2.mutable_username}";
                        }
                    }
                    await Logger.LogInformation(work, $"Exported {entry2.mutable_username}", account);
                }
            }
            
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

    public async Task Start(WorkRequest work, ExportFriendsArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}