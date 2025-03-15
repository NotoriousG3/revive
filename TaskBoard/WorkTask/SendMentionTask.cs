using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class SendMentionTask: ActionWorkTask
{
    public SendMentionTask(WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
    }
    
    private async Task<WorkStatus> DoWork(WorkRequest work, SendMentionArguments arguments, SnapchatAccountModel account)
    {
        using var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

        await Logger.LogDebug(work, "Starting SendMention task", account);
        
        try
        {
            if (arguments.FriendsOnly)
            {
                var friends = (await CacheAndGetTargetsSplit(work, account, proxyGroup));

                if (friends == null || !friends.Any())
                {
                    await Logger.LogError(work, $"No Friends Skipping Account", account);
                    await Scheduler.UpdateWorkAddPass(work);
                    return WorkStatus.Ok; // Don't fail are clients are paseants and dumb and will think error
                }

                // If this is sending repeated messages, we need to change arguments.User for new List<string>() { user }
                foreach (var chunk in friends)
                {
                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                    
                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                    await Runner.SendMention(account, arguments.User, chunk, proxyGroup, work.CancellationTokenSource.Token);
                    await Logger.LogInformation(work, $"Mentioned {arguments.User} to: {string.Join(", ", chunk)}", account);
                }
                
                await scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok;
            }
            else // Thunda forgets code still excutes after if retard l0l // Justxn, this shouldn't execute because of the return in the previous block
            {
                var targets = arguments.RandomUsers ? await TargetManager.GetRandomTargetNames(arguments.RandomTargetAmount, false, false, arguments.CountryFilter, arguments.RaceFilter, arguments.GenderFilter) : await TargetManager.ProcessTargetList(work, account, arguments.Users);

                // We need our account to add the targets
                foreach (var T in targets)
                {
                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                    if (!await TryAddFriend(work, account, proxyGroup, T.Username)) continue;

                    HashSet<string> target = new HashSet<string>{ T.Username };

                    await Runner.SendMention(account, arguments.User, target,
                        proxyGroup, work.CancellationTokenSource.Token);

                    T.Added = true;
                    T.Used = true;
                    await TargetManager.SaveTargetUpdates(T);

                    await Logger.LogInformation(work,
                        $"Mentioned {arguments.User} to: {string.Join(", ", target)}",
                        account);
                }
                await scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok;
            }
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
    
    public async Task Start(WorkRequest work, SendMentionArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await SetTaskTargets(work, arguments);
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}