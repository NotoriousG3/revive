using System.Net;
using Newtonsoft.Json;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using SnapchatLib.Exceptions;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class FindUsersViaSearchTask: ActionWorkTask
{
    public FindUsersViaSearchTask(TargetManager targetManager, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        CheckForMedia = false;
    }

    private async Task<WorkStatus> DoWork(WorkRequest work, FindUsersViaSearchArguments arguments, SnapchatAccountModel account)
    {
        var keyword = string.Empty;
        var nCountry = "UNKNOWN";
        var nGender = "UNKNOWN";
        var nRace = "UNKNOWN";
        
        await Logger.LogDebug(work, "Starting FindUsersViaSearch task", account);

        using var scope = ServiceProvider.CreateScope();
        var keywordManager = scope.ServiceProvider.GetRequiredService<KeywordManager>();

        try
        {
            if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
            
            string firstName = "";
            string lastName = "";
            int actions = 0;
            
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

            while (actions++ < arguments.ActionsPerAccount)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                
                if (work.AccountsLeft <= 0)
                {
                    return WorkStatus.Ok;
                }
                
                if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                
                if (arguments.Keyword.Equals("keyword"))
                {
                    var keywordCount = await keywordManager.Count();

                    if (keywordCount == 0)
                    {
                        await Logger.LogInformation(work, $"No more keywords to process.", account);
                        Scheduler.EndWork(work, WorkStatus.Error).Wait();
                        return WorkStatus.Error;
                    }

                    await Logger.LogInformation(work, $"Keyword Count: {keywordCount}", account);
                    keyword = (keywordManager.PickRandom()).Name;
                    
                    if (keyword.Split(' ').Length > 1)
                    {
                        firstName = keyword.Split(' ')[0];
                        lastName = keyword.Split(' ')[1];
                    }
                    else
                    {
                        firstName = keyword;
                        lastName = " ";
                    }
                }
                else if(arguments.Keyword.Equals("random"))
                {
                    keyword = Faker.Lorem.Words(1).First();
                    if (keyword.Split(" ").Length > 1)
                    {
                        firstName = keyword.Split(" ")[0];
                        lastName = keyword.Split(" ")[1];
                    }
                }
                else
                {
                    firstName = Faker.Name.First();
                    lastName = Faker.Name.Last();
                    keyword = $"{firstName} {lastName}";
                }

                await Logger.LogInformation(work, $"Picked keyword {keyword}", account);

                List<string> foundNames = new();

                var obj = await Runner.FindUsersViaSearch(account, keyword, proxyGroup, work.CancellationTokenSource.Token);

                if (obj.SectionsArray != null)
                {
                    foreach (var sectionsArray in obj.SectionsArray)
                    {
                        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
                        
                        if (!await Runner.CheckAccountStatus(work, Logger, Scheduler, account)) { return WorkStatus.Error; }
                        
                        if (sectionsArray.ResultsArray == null) continue;

                        foreach (var resultsArray in sectionsArray.ResultsArray)
                        {
                            if (resultsArray == null || resultsArray.User == null) continue;

                            if (arguments.OnlyActive)
                            {
                                if (!await Runner.IsUserActive(account, resultsArray.User.MutableUsername, proxyGroup, work.CancellationTokenSource.Token))
                                {
                                    continue;
                                }
                                
                                await Task.Delay(TimeSpan.FromSeconds(arguments.SearchDelay));
                            }
                            
                            foundNames.Add($"{resultsArray.User.MutableUsername}:{resultsArray.User.IdP}");

                            await Logger.LogInformation(work,
                                $"{account.Username} found {resultsArray.User.MutableUsername} searching with keyword {keyword}",
                                account);
                        }
                    }
                }
                else
                {
                    await Logger.LogInformation(work,
                        $"{account.Username} found no accounts using keyword {keyword}",
                        account);
                }

                //TODO: Move this to its own service?
                var credentials = new NetworkCredential(account.Proxy?.User, account.Proxy?.Password);
                using var client = new HttpClient(new HttpClientHandler()
                {
                    Proxy = new WebProxy(account.Proxy?.Address, true, null, credentials),
                    AllowAutoRedirect = false
                });

                if (firstName?.Length > 0 && lastName?.Length > 0)
                {
                    var settingsLoader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
                    var settings = await settingsLoader.Load();
                    if (settings.NamsorApiKey != null && settings.NamsorApiKey.Length > 0)
                    {
                        try
                        {
                            Configuration config = new();
                            config.ApiKey.Add("X-API-KEY", settings.NamsorApiKey);
                            var apiInstance = new PersonalApi(config);
                            var response = await client.GetAsync($"https://api.diversitydata.io/?fullname={firstName}%20{lastName}");
                            var json = await response.Content.ReadAsStringAsync();
                            DiversityData? diversityData = JsonConvert.DeserializeObject<DiversityData?>(json);
                            nRace = diversityData?.ethnicity;
                            nCountry = (await apiInstance.CountryAsync($"{firstName} {lastName}")).Country;
                            nGender = diversityData?.gender;
                        }
                        catch (Exception)
                        {
                            nRace = "UNKNOWN";
                            nCountry = "UNKNOWN";
                            nGender = "UNKNOWN";
                        }
                    }
                    else
                    {
                        try
                        {
                            var genderApiResponse = await client.GetAsync(
                                $"http://genderapi.io/api/?name={firstName}%20{lastName}");
                            var genderApiJson = await genderApiResponse.Content.ReadAsStringAsync();
                            
                            PredictKlass? predictKlass = JsonConvert.DeserializeObject<PredictKlass?>(genderApiJson);

                            nGender = predictKlass?.gender;
                            nCountry = predictKlass?.country;

                            var diversityResponse = await client.GetAsync(
                                $"https://api.diversitydata.io/?fullname={firstName}%20{lastName}");
                            var json = await diversityResponse.Content.ReadAsStringAsync();
                            
                            DiversityData? diversityData = JsonConvert.DeserializeObject<DiversityData?>(json);

                            nRace = diversityData?.ethnicity;
                        }
                        catch (Exception)
                        {
                            nRace = "UNKNOWN";
                            nCountry = "UNKNOWN";
                            nGender = "UNKNOWN";
                        }
                    }
                }
                
                if (foundNames.Any())
                {
                    foreach (var name in foundNames)
                    {
                        var user = new TargetUser()
                        {
                            Username = name.Split(":")[0], UserID = name.Split(":")[1], CountryCode = nCountry, Race = nRace?.ToUpper(),
                            Gender = nGender?.ToUpper()
                        };
                        try
                        {
                            await TargetManager.Add(user);
                        }
                        catch (Exception ex)
                        {
                            await Logger.LogError(work, ex, account);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(arguments.SearchDelay));
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

            if (ex.Message.Contains("API Limit Reached or API Key Disabled"))
            {
                await Scheduler.EndWork(work, WorkStatus.Error);
                work.CancellationTokenSource.Cancel();
            }

            await Scheduler.FailWorkAccount(work, account);
            return WorkStatus.Error;
        }
        finally
        {
            await AssignAccount(work, account);
        }
    }

    public async Task Start(WorkRequest work, FindUsersViaSearchArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}