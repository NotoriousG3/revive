using Microsoft.IdentityModel.Tokens;
using SnapchatLib.Exceptions;
using SnapchatLib.REST.Models;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class AcceptFriendTask: ActionWorkTask
{
    private readonly SnapchatActionRunner _runner;
    private readonly WorkLogger _logger;
    private readonly WorkScheduler _scheduler;
    private readonly SnapchatAccountManager _accountManager;

    public AcceptFriendTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _accountManager = accountManager;
        _runner = runner;
        _logger = logger;
        _scheduler = scheduler;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, AcceptFriendArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting AcceptFriend task", account);
        
        var count = 0;
        var friendCount = 0;
        List<string> messagedFriends = new List<string>();

        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
        
        try
        {
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

            SyncResponse.ami_friends info2 = null;

            try
            {
                info2 = await _runner.SyncFriends(account, proxyGroup, work.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await _logger.LogError(work,
                    $"{account.Username} could not sync friends list failing.");
                await Scheduler.FailWorkAccount(work, account);
                return WorkStatus.Error;
            }

            if (info2 != null)
                foreach (var entry2 in info2.added_friends)
                {
                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account))
                    {
                        return WorkStatus.Error;
                    }

                    if (entry2.type == 2 || entry2.type == 3 || entry2.mutable_username == "teamsnapchat" ||
                        entry2.mutable_username == account.Username || count >= arguments.MaxAdds)
                    {
                        continue;
                    }

                    if (entry2.type == 6 || entry2.type == 1)
                    {
                        await _logger.LogInformation(work,
                            $"{account.Username} accepting friend request from {entry2.mutable_username}. Accepted: {count}");

                        if (await TryAcceptFriend(work, account, proxyGroup, entry2.user_id))
                        {
                            try
                            {
                                if (!messagedFriends.Contains(entry2.mutable_username))
                                {
                                    if (work.CancellationTokenSource.IsCancellationRequested)
                                        return WorkStatus.Cancelled;

                                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account))
                                    {
                                        return WorkStatus.Error;
                                    }

                                    if (arguments.AcceptMessage.Length > 0)
                                    {
                                        await _runner.SendMessage(account, arguments.AcceptMessage,
                                            new HashSet<string>() { entry2.mutable_username }, proxyGroup,
                                            work.CancellationTokenSource.Token);
                                        await Logger.LogInformation(work,
                                            $"Sent message {arguments.AcceptMessage} to: {string.Join(", ", new List<string> { entry2.mutable_username })}",
                                            account);
                                        messagedFriends.Add(entry2.mutable_username);
                                    }

                                    foreach (var snap in arguments.Snaps)
                                    {
                                        if (work.CancellationTokenSource.IsCancellationRequested)
                                            return WorkStatus.Cancelled;

                                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account))
                                        {
                                            return WorkStatus.Error;
                                        }

                                        var media = await GetMediaFileOrCancelJob(work, snap);

                                        // Only return since the method above handles messaging
                                        if (media == null)
                                        {
                                            return WorkStatus.Error;
                                        }

                                        await Task.Delay(TimeSpan.FromSeconds(snap.SecondsBeforeStart));

                                        await Runner.PostDirect(account, media.ServerPath,
                                            snap.GetRandomUrl(3),
                                            new HashSet<string> { entry2.mutable_username }, proxyGroup,
                                            work.CancellationTokenSource.Token);

                                        await Logger.LogInformation(work,
                                            $"Post sent to user list: {string.Join(", ", new List<string> { entry2.mutable_username })}",
                                            account);
                                    }
                                }

                                count++;
                            }
                            catch
                            {
                                await _logger.LogInformation(work,
                                    $"{account.Username} failed accepting friend request from {entry2.mutable_username}. Accepted: {count}");
                            }

                            await Task.Delay(TimeSpan.FromSeconds(arguments.AddDelay));
                        }
                    }
                }

            await _accountManager.UpdateAccount(account);
            
            await _logger.LogInformation(work,
                $"{account.Username} finished adding {count} friend(s).");
            
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
            await Logger.LogInformation(work, "User No Found Skipping", account);
            await Scheduler.UpdateWorkAddPass(work);
            return WorkStatus.Ok;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found"))
            {
                await Logger.LogInformation(work, "User No Found Skipping", account);
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

    public async Task Start(WorkRequest work, AcceptFriendArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}