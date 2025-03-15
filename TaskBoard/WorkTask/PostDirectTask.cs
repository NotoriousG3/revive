using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class PostDirectTask: ActionWorkTask
{
    private readonly AppSettingsLoader _settingsLoader;
    public PostDirectTask(AppSettingsLoader settingsLoader, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
        _settingsLoader = settingsLoader;
    }
    
    private async Task<WorkStatus> DoWork(WorkRequest work, PostDirectArguments arguments, SnapchatAccountModel account)
    {
        try
        {
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

            await Logger.LogInformation(work, "Starting PostDirect work");

            if (arguments.FriendsOnly)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                
                var friends = await CacheAndGetTargetsSplit(work, account, proxyGroup);
                
                if (friends == null || !friends.Any())
                {
                    await Logger.LogError(work, $"No Friends Skipping Account", account);
                    await Scheduler.UpdateWorkAddPass(work);
                    return WorkStatus.Ok; // Don't fail are clients are paseants and dumb and will think error
                }
                
                foreach (var snap in arguments.Snaps)
                {
                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                    
                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                    var media = await GetMediaFileOrCancelJob(work, snap);

                    // Only return since the method above handles messaging
                    if (media == null) { return WorkStatus.Error; }; // We already cancel with GetMediaFileOrCancelJob()

                    await Task.Delay(TimeSpan.FromSeconds(snap.SecondsBeforeStart));
                    
                    foreach (HashSet<string> chunk in friends)
                    {
                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                        
                        // nice dump code but ok i solved https://cdn.discordapp.com/attachments/1063171030224474142/1063171051602858114/chrome_wAKVVn9grB.png

                        await Runner.PostDirect(account, media.ServerPath,
                                snap.GetRandomUrl(arguments.RotateLinkEvery), chunk, proxyGroup,
                                work.CancellationTokenSource.Token);


                        await Logger.LogInformation(work, $"Post sent to user list: {string.Join(", ", chunk)}", account);
                    }
                }

                await Scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok;
            }
            else // Thunda forgets code still excutes after if retard l0l
            {
                var targets = arguments.RandomUsers ? await TargetManager.GetRandomTargetNames(arguments.RandomTargetAmount, false, false, arguments.CountryFilter, arguments.RaceFilter, arguments.GenderFilter) : await TargetManager.ProcessTargetList(work, account, arguments.Users);

                // We need our account to add the targets

                foreach (var T in targets)
                {
                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                    
                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                    if (!await TryAddFriend(work, account, proxyGroup, T.Username)) continue;

                    // Now flag it as added
                    T.Added = true;

                    foreach (var snap in arguments.Snaps)
                    {
                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                        
                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                        var media = await GetMediaFileOrCancelJob(work, snap);

                        // Only return since the method above handles messaging
                        if (media == null) { return WorkStatus.Error; }; // We already cancel job in GetMediaFileOrCancelJob();

                        await Task.Delay(TimeSpan.FromSeconds(snap.SecondsBeforeStart));

                        await Runner.PostDirect(account, media.ServerPath, snap.GetRandomUrl(arguments.RotateLinkEvery),
                            new HashSet<string> { T.Username }, proxyGroup,
                            work.CancellationTokenSource.Token);

                        // Since our action finished, now we mark it as used
                        T.Used = true;

                        await Logger.LogInformation(work,
                            $"Post sent to user list: {string.Join(", ", new List<string> { T.Username })}", account);
                    }
                    
                    await TargetManager.SaveTargetUpdates(T); // Call this once after we're done with a target.
                }

                await Scheduler.UpdateWorkAddPass(work);

                return WorkStatus.Ok;
            }

        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                if (e is not NoAvailableProxyException)
                {
                    // If we don't handle this, then the exception is not caught :(
                    Logger.LogError(work, e, account)
                        .Wait(work.CancellationTokenSource.Token);
                    Scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
                    return true;
                }

                Logger.LogError(work, "No proxies found").Wait(work.CancellationTokenSource.Token);
                Scheduler.EndWork(work, WorkStatus.Error).Wait();
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
        /*catch (UsernameNotFoundException)
        {
            await Logger.LogInformation(work, "Username not found when trying to post skipping.", account);
            await Scheduler.UpdateWorkAddPass(work);
            return WorkStatus.Ok;
        }*/
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

    public async Task Start(WorkRequest work, PostDirectArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await SetTaskTargets(work, arguments);
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}