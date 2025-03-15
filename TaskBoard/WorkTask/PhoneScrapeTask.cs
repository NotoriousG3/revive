using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class PhoneScrapeTask: ActionWorkTask
{
    public PhoneScrapeTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, PhoneSearchArguments arguments, SnapchatAccountModel account)
    {
        await Logger.LogInformation(work, "Starting PhoneScrape work");

        using var scope = ServiceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        
        try
        {
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
            
            string nNumber = string.Empty;
            string nCountry = string.Empty;
            var actionsPerformed = 0;

            while (actionsPerformed++ < arguments.ActionsPerAccount)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (work.AccountsLeft <= 0)
                {
                    return WorkStatus.Ok;
                }
                
                if (!arguments.Randomizer.Equals("randomize"))
                {
                    var phoneManager = scope.ServiceProvider.GetRequiredService<PhoneNumberManager>();
                    int phoneCount = await phoneManager.Count();

                    if (phoneCount > 0)
                    {
                        await Logger.LogInformation(work, $"Phone Count: {phoneCount}");
                        PhoneListModel phone = phoneManager.PickRandom();
                        nNumber = phone.Number;
                        nCountry = phone.CountryCode;
                        await phoneManager.Delete(phone);
                    }
                    else
                    {
                        await Logger.LogInformation(work, $"No more phone numbers to process.");
                        scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
                        return WorkStatus.Error;
                    }
                }
                else
                {
                    var translatedCountry = "";

                    switch (arguments.CountryCode)
                    {
                        case "US":
                            translatedCountry = "en_US";
                            break;
                        case "CA":
                            translatedCountry = "en_CA";
                            break;
                        case "UK":
                            translatedCountry = "en_GB";
                            break;
                        case "NL":
                            translatedCountry = "nl";
                            break;
                        case "PL":
                            translatedCountry = "pl";
                            break;
                        case "AE":
                            translatedCountry = "ar";
                            break;
                        case "SE":
                            translatedCountry = "sv";
                            break;
                        case "AU":
                            translatedCountry = "en_AU";
                            break;
                        case "FI":
                            translatedCountry = "fi";
                            break;
                        case "DE":
                            translatedCountry = "de";
                            break;
                    }

                    Utilities _utilities = new();
                    nNumber = _utilities.GenerateRandomPhoneNumber(translatedCountry);

                    switch (arguments.CountryCode)
                    {
                        case "US":
                            nNumber = nNumber.Substring(nNumber.IndexOf('-') + 1, (nNumber.Length - 2))
                                .Replace("-", "")
                                .Replace("(", "")
                                .Replace(")", "");
                            break;
                        case "CA":
                            nNumber = nNumber.Replace(".", "");
                            break;
                        case "UK":
                            nNumber = nNumber.Substring(1, (nNumber.Length - 1)).Replace(" ", "");
                            break;
                        case "NL":
                            break;
                        case "DE":
                            nNumber = nNumber.Substring(nNumber.IndexOf('-') + 1, (nNumber.Length - 4))
                                .Replace("-", "");
                            break;
                        case "AE":
                            nNumber = nNumber.Replace("-", "");
                            break;
                        case "FI":
                            nNumber = nNumber.Replace("-", "");
                            break;
                        case "AU":
                            nNumber = nNumber.Replace(" ", "");
                            break;
                        case "PL":
                            nNumber = nNumber.Replace("-", "");
                            break;
                        default:
                            break;
                    }

                    nCountry = arguments.CountryCode;
                }

                await Logger.LogInformation(work, $"Picked phone ({nCountry}){nNumber}");
                await Logger.LogInformation(work, $"Picked account {account.Username}");

                List<string> foundNames = new();

                using (var obj = Runner.PhoneToUsername(account, nNumber, nCountry, proxyGroup, work.CancellationTokenSource.Token))
                {
                    foreach (var resultsArray in obj.Result.SnapchattersArray)
                    {
                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                        
                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                        
                        if (resultsArray is not null)
                        {
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
                                $"{account.Username} found {resultsArray.Username} searching with phone ({nCountry}){nNumber}");
                        }
                    }
                }

                if (foundNames.Any())
                {
                    var targetManager = scope.ServiceProvider.GetRequiredService<TargetManager>();

                    foreach (var name in foundNames)
                    {
                        var user = new TargetUser()
                            { Username = name, CountryCode = nCountry, Race = "UNKNOWN", Gender = "UNKNOWN" };

                        await targetManager.Add(user);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(20));
            }

            await scheduler.UpdateWorkAddPass(work);
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

    public async Task Start(WorkRequest work, PhoneSearchArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}