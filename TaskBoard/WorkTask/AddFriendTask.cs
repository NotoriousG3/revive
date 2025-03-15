using Microsoft.IdentityModel.Tokens;
using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class AddFriendTask: ActionWorkTask
{
    private readonly Utilities _utilities;
    private readonly SnapchatAccountManager _accountManager;
    
    public AddFriendTask(Utilities utilities, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _utilities = utilities;
        _accountManager = accountManager;
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, AddFriendArguments arguments, SnapchatAccountModel account)
    {
        if (work.FailedAccounts != null && work.FailedAccounts.Contains(account.Username))
        {
            return WorkStatus.Error;
        }
        
        await Logger.LogDebug(work, "Starting AddFriend task", account);

        // Store how many friends we have added so that we can stop at the requested limit
        var target = "";
        var targetCount = 0;
        var failedAttempts = 0;
        int added = 0;

        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
        while (true)
        {
            try
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                var amountOfFriends = await CacheFriends(account, proxyGroup, work.CancellationTokenSource.Token);

                if (arguments.Users.Count > 0)
                {
                    if (targetCount < arguments.Users.Count)
                    {
                        target = arguments.Users[targetCount++];
                    }
                    else
                    {
                        account.hasAdded = true;
                        await _accountManager.UpdateAccount(account);
                        await Scheduler.UpdateWorkAddPass(work);
                        return WorkStatus.Ok;
                    }
                }
                else
                {
                    target = (TargetManager.GetRandomTargetNames(false, false, arguments.CountryFilter,
                        arguments.RaceFilter, arguments.GenderFilter)).Result.FirstOrDefault()?.Username.ToLower();
                }

                if (account.EmailValidated != ValidationStatus.Validated ||
                    account.PhoneValidated != ValidationStatus.Validated)
                {
                    if (amountOfFriends.Count >= 100)
                    {
                        account.hasAdded = true;
                        await _accountManager.UpdateAccount(account);
                        await Scheduler.UpdateWorkAddPass(work);
                        return WorkStatus.Ok;
                    }
                }
                else if (account.EmailValidated == ValidationStatus.Validated ||
                         account.PhoneValidated == ValidationStatus.Validated)
                {
                    if (amountOfFriends.Count >= 838)
                    {
                        account.hasAdded = true;
                        await _accountManager.UpdateAccount(account);
                        await Scheduler.UpdateWorkAddPass(work);
                        return WorkStatus.Ok;
                    }
                }

                if (!target.IsNullOrEmpty())
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        Proxy = account.SnapClient.SnapchatClient.SnapchatConfig.Proxy,
                    };
                    
                    using (var _c = new HttpClient(httpClientHandler))
                    {
                        
                        _c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                        var resp = await _c.GetAsync($"https://www.snapchat.com/add/{target}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var content = await resp.Content.ReadAsStringAsync();
                            if (content.Contains("https://images.bitmoji.com/3d/avatar/") ||
                                content.Contains("https://cf-st.sc-cdn.net/aps/bolt/"))
                            {
                                if (await TryAddFriend(work, account, proxyGroup, target))
                                {
                                    added++;
                                    await Logger.LogInformation(work, $"Added {target} as friend", account);

                                    var targetObject = TargetManager.FindTargetUserObject(target);

                                    if (targetObject != null)
                                    {
                                        targetObject.Added = true;
                                        await TargetManager.SaveTargetUpdates(targetObject);
                                    }
                                }
                                else
                                {
                                    await Logger.LogInformation(work, $"Unable to add {target} as friend (account is most likely very inactive)", account);
                                    failedAttempts++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    await Logger.LogInformation(work, $"Unable to add {target} as friend", account);
                    failedAttempts++;
                }
                
                await Logger.LogInformation(work, $"Total added friends so far: {added}", account);

                if (added >= arguments.FriendsPerAccount)
                {
                    account.hasAdded = true;
                    await _accountManager.UpdateAccount(account);
                    await Scheduler.UpdateWorkAddPass(work);
                    await Logger.LogInformation(work, $"{account.Username} has finished adding.", account);
                    return WorkStatus.Ok;
                }
                
                if (failedAttempts >= 15)
                {
                    await Logger.LogError(work, $"Account {account.Username} has failed to add too many users and is now failed.");
                    await Scheduler.FailWorkAccount(work, account);
                    return WorkStatus.Error;
                }

                await Task.Delay(TimeSpan.FromSeconds(arguments.AddDelay), work.CancellationTokenSource.Token);
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
                await Logger.LogInformation(work,
                    "Proxy is banned from uploading change proxies we are stopping the job for this account", account);
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
    }

    public async Task Start(WorkRequest work, AddFriendArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}