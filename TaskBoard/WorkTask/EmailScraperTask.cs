using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class EmailScraperTask: ActionWorkTask
{
    private readonly EmailAddressManager _emailAddressManager;
    public EmailScraperTask(EmailAddressManager emailAddressManager, TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
        _emailAddressManager = emailAddressManager;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, EmailSearchArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogDebug(work, "Starting EmailScraper task", account);
        
        using var scope = ServiceProvider.CreateScope();
        var actionsPerformed = 0;
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

        try
        {
            while (actionsPerformed++ < arguments.ActionsPerAccount)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (work.AccountsLeft <= 0)
                {
                    return WorkStatus.Ok;
                }
                
                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }

                EmailListModel email = null;
                
                if (!arguments.Address.Equals("random"))
                {
                    if (await _emailAddressManager.Count() == 0)
                    {
                        await Logger.LogInformation(work, $"No e-mail addresses to process.");
                        await Scheduler.EndWork(work, WorkStatus.Error);
                        return WorkStatus.Error;
                    }
                    
                    await Logger.LogInformation(work, $"E-Mail Count: {await _emailAddressManager.Count()}");
                    email = await _emailAddressManager.PopRandom();
                }
                else
                {
                    Random r = new();

                    int emailDomain = r.Next(0, 4);
                    
                    email = new();
                    email.Address = Faker.Internet.Email();

                    switch (emailDomain)
                    {
                        case 0:
                            email.Address = email.Address.Split('@')[0] + "@"  + "gmail.com";
                            break;
                        case 1:
                            email.Address = email.Address.Split('@')[0] + "@"  + "yahoo.com";
                            break;
                        case 2:
                            email.Address = email.Address.Split('@')[0] + "@"  + "aol.com";
                            break;
                        case 3:
                            email.Address = email.Address.Split('@')[0] + "@"  + "outlook.com";
                            break;
                        case 4:
                            email.Address = email.Address.Split('@')[0] + "@" + "hotmail.com";
                            break;
                    }
                }

                await Logger.LogInformation(work, $"Picked e-mail {email.Address}");
                await Logger.LogInformation(work, $"Picked account {account.Username}");

                List<string> foundNames = new();

                using (var obj = Runner.EmailToUsername(account, email.Address, proxyGroup, work.CancellationTokenSource.Token))
                {
                    foreach (var resultsArray in obj.Result.SnapchattersArray)
                    {
                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                        
                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                        
                        if (resultsArray.Username == null)
                        {
                            continue;
                        }

                        if (arguments.OnlyActive)
                        {
                            if (!await Runner.IsUserActive(account, resultsArray.Username, proxyGroup, work.CancellationTokenSource.Token))
                            {
                                continue;
                            }
                        }
                        
                        foundNames.Add(resultsArray.Username);

                        await Logger.LogInformation(work,
                            $"{account.Username} found {resultsArray.Username} searching with e-mail {email.Address}");
                    }
                }

                if (foundNames.Any())
                {
                    foreach (var name in foundNames)
                    {
                        var user = new TargetUser()
                            { Username = name, CountryCode = "UNKNOWN", Race = "UNKNOWN", Gender = "UNKNOWN" };
                        await TargetManager.Add(user);
                    }
                }
                else
                {
                    await Logger.LogInformation(work,
                        $"{account.Username} found no usernames searching with e-mail {email.Address}");
                }

                await Task.Delay(TimeSpan.FromSeconds(20));
            }

            await Scheduler.UpdateWorkAddPass(work).WaitAsync(work.CancellationTokenSource.Token);
            
            return WorkStatus.Ok;
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                ProcessAggregateException(ae, work, account);
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

    public async Task Start(WorkRequest work, EmailSearchArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}