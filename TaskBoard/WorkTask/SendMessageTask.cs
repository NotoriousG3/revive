using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;
using System.Text;
using SnapProto.Snapchat.Search;
using SnapchatLib.Exceptions;

namespace TaskBoard.WorkTask;

public class SendMessageTask: ActionWorkTask
{
    private readonly AppSettingsLoader _settingsLoader;
    public SendMessageTask(AppSettingsLoader settingsLoader, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
        _settingsLoader = settingsLoader;
    }

    private string ProcessMacros(string message, SCS2SearchResponse response)
    {
        using var scope = ServiceProvider.CreateScope();
        var macroManager = scope.ServiceProvider.GetRequiredService<MacroManager>();
        SCS2User result = null;
        
        if (response.SectionsArray != null)
        {
            var section = response.SectionsArray.FirstOrDefault();

            if (section != null)
            {
                var user = section.ResultsArray.FirstOrDefault();

                if (user != null)
                {
                    result = user.User;
                }
            }
        }

        StringBuilder resultString = new StringBuilder(message);
        
        // Name
        resultString.Replace("#name", result?.MutableUsername);
        
        // Display Name
        resultString.Replace("#display_name", result?.DisplayName);

        foreach (var macro in macroManager._macros)
        {
            resultString.Replace(macro.Text, macro.Replacement);
        }

        return resultString.ToString();
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, SendMessageArguments arguments, SnapchatAccountModel account)
    {
        using var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
        
        await Logger.LogDebug(work, "Starting SendMessage task", account);
        
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
                foreach (HashSet<string> chunk in friends)
                {
                    if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                    
                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                    
                    foreach(var message in arguments.Messages){
                        
                        await Task.Delay(TimeSpan.FromSeconds(message.SecondsBeforeStart));
                        
                        if (arguments.EnableMacros)
                        {
                            foreach (var user in chunk)
                            {
                                var userObj = await Runner.FindUsersViaSearch(account, user, proxyGroup,
                                    work.CancellationTokenSource.Token);

                                if (userObj == null)
                                {
                                    continue;
                                }
                                
                                if (message.IsLink)
                                {
                                    await Runner.SendLink(account, ProcessMacros(message.Message, userObj),
                                        new HashSet<string> { user },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }
                                else
                                {
                                    await Runner.SendMessage(account, ProcessMacros(message.Message, userObj),
                                        new HashSet<string> { user },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }
                            }
                        }
                        else
                        {
                            if (message.IsLink)
                            {
                                await Runner.SendLink(account, message.Message, chunk,
                                    proxyGroup, work.CancellationTokenSource.Token);
                            }
                            else
                            {
                                await Runner.SendMessage(account, message.Message, chunk,
                                    proxyGroup, work.CancellationTokenSource.Token);
                            }
                        }
                        
                        await Logger.LogInformation(work, $"Sent {message.Message} to: {string.Join(", ", chunk)}", account);
                    }
                }
                
                await scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok;
            }
            else
            {
                var targets = arguments.RandomUsers ? await TargetManager.GetRandomTargetNames(arguments.RandomTargetAmount, false, false, arguments.CountryFilter, arguments.RaceFilter, arguments.GenderFilter) : await TargetManager.ProcessTargetList(work, account, arguments.Users);
                
                UnicodeEncoding unicode = new UnicodeEncoding();

                foreach (var T in targets)
                {
                    
                    if (!await TryAddFriend(work, account, proxyGroup, T.Username)) continue;

                    T.Added = true;
                    await TargetManager.SaveTargetUpdates(T);
                    
                    foreach (var msg in arguments.Messages)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(msg.SecondsBeforeStart));
                        
                        var message = unicode.GetString(unicode.GetBytes(msg.Message));
                        
                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account))
                        {
                            return WorkStatus.Error;
                        }

                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                        if (arguments.EnableMacros)
                        {
                            var userObj = await Runner.FindUsersViaSearch(account, T.Username, proxyGroup,
                                work.CancellationTokenSource.Token);

                            if (userObj == null)
                            {
                                continue;
                            }

                            if (msg.IsLink)
                            {
                                await Runner.SendLink(account, message,
                                    new HashSet<string> { T.Username },
                                    proxyGroup, work.CancellationTokenSource.Token);
                            }
                            else
                            {
                                await Runner.SendMessage(account, ProcessMacros(message, userObj),
                                    new HashSet<string> { T.Username },
                                    proxyGroup, work.CancellationTokenSource.Token);
                            }

                            T.Used = true;
                            await TargetManager.SaveTargetUpdates(T);

                            await Logger.LogInformation(work,
                                $"Sent message {message} to: {string.Join(", ", new List<string> { T.Username })}",
                                account);
                        }
                        else
                        {
                            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account))
                            {
                                return WorkStatus.Error;
                            }

                            if (arguments.EnableMacros)
                            {
                                var userObj = await Runner.FindUsersViaSearch(account, T.Username, proxyGroup,
                                    work.CancellationTokenSource.Token);

                                if (userObj == null)
                                {
                                    continue;
                                }

                                if (msg.IsLink)
                                {
                                    await Runner.SendLink(account, message,
                                        new HashSet<string> { T.Username },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }
                                else
                                {
                                    await Runner.SendMessage(account, ProcessMacros(message, userObj),
                                        new HashSet<string> { T.Username },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }

                                T.Used = true;
                                await TargetManager.SaveTargetUpdates(T);

                                await Logger.LogInformation(work,
                                    $"Sent message {message} to: {string.Join(", ", new List<string> { T.Username })}",
                                    account);
                            }
                            else
                            {
                                if (msg.IsLink)
                                {
                                    await Runner.SendLink(account, message,
                                        new HashSet<string> { T.Username },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }
                                else
                                {
                                    await Runner.SendMessage(account, message, new HashSet<string> { T.Username },
                                        proxyGroup, work.CancellationTokenSource.Token);
                                }

                                T.Used = true;
                                await TargetManager.SaveTargetUpdates(T);

                                await Logger.LogInformation(work,
                                    $"Sent message {message} to: {string.Join(", ", new List<string> { T.Username })}",
                                    account);
                            }
                        }
                    }
                }

                await scheduler.UpdateWorkAddPass(work);
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
                    scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
                    return true;
                }

                Logger.LogError(work, "No proxies found").Wait(work.CancellationTokenSource.Token);
                scheduler.EndWork(work, WorkStatus.Error).Wait();
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
            await Logger.LogInformation(work, "User No Found Skipping", account);
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
    
    public async Task Start(WorkRequest work, SendMessageArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await SetTaskTargets(work, arguments);
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}