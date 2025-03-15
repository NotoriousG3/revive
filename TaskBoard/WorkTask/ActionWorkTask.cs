using Microsoft.EntityFrameworkCore;
using SnapchatLib.Exceptions;
using SnapProto.Snapchat.Friending;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public abstract class ActionWorkTask : BaseWorkTask
{
    protected delegate Task<WorkStatus> WorkDelegate<T>(WorkRequest work, T arguments, SnapchatAccountModel account);

    protected delegate void TaskCleanupDelegate(SnapchatAccountModel account);

    protected readonly SnapchatAccountManager AccountManager;
    protected readonly SnapchatActionRunner Runner;
    protected readonly WorkScheduler Scheduler;
    protected readonly AccountTracker AccountTracker;
    protected readonly TargetManager TargetManager;

    protected bool CheckForMedia = true;

    protected List<TargetUser> TargetUsers;

    public ActionWorkTask(WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager,
        SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager,
        IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        AccountManager = accountManager;
        Runner = runner;
        Scheduler = scheduler;
        AccountTracker = accountTracker;
        TargetManager = targetManager;
    }

    protected async Task AssignAccount(WorkRequest work, SnapchatAccountModel account)
    {
        if (work.CancellationTokenSource != null && work.CancellationTokenSource.IsCancellationRequested) return;
        await using var context =
            ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
                     
        var chosenAccount = new ChosenAccount { WorkId = work.Id, AccountId = account.Id };
        if (context.ChosenAccounts != null) context.ChosenAccounts.Add(chosenAccount);

        await context.SaveChangesAsync();
    }
                                                                                                                                                                         
    protected async Task PersistAccountChanges(ApplicationDbContext context, SnapchatAccountModel account)
    {
        context.Update(account);
        await context.SaveChangesAsync();
    }

    private async Task<bool> ValidateAccounts(WorkRequest work, IEnumerable<SnapchatAccountModel> accounts)
    {
        if (accounts.Any()) return true;

        await Logger.LogInformation(work, "There are no accounts that can execute this work");

        await Scheduler.EndWork(work, WorkStatus.Error);
        return false;
    }

    public async Task<SnapchatAccountModel[]?> PickAccounts(int accountsToUse, WorkRequest work, long accountGroupId = 0)
    {
        // If we are requesting 0, then returning an empty ienumerable
        if (accountsToUse <= 0) return Array.Empty<SnapchatAccountModel>();
        
        await using var context = ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Exclude those accounts we already used for this work
        var excludeAccounts = await context.ChosenAccounts?
            .Where(a => a.Work.Id == work.Id &&
                        a.Account.FriendCount >= work.MinFriends &&
                        a.Account.FriendCount <= work.MaxFriends &&
                        a.Account.AccountStatus != AccountStatus.OKAY &&
                        a.Account.AccountStatus != AccountStatus.RATE_LIMITED)
            .Select(ca => ca.Account.Username)
            .ToListAsync()!;;

        SnapchatAccountModel[]? accounts = null;

        if (work.AccountsById)
        {
            await Logger.LogDebug(work,
                $"Picking accounts by ID - Excluding {excludeAccounts.Count} already used on this job");
            if (work.AssignedAccounts != null)
                accounts = (await AccountManager.PickWithIds(work.AssignedAccounts.Count(), work.AssignedAccounts,
                    excludeAccounts)).ToArray();
        }else if (work.PreviousWorkRequestId == null)
        {
            await Logger.LogDebug(work,
                $"Picking random accounts - Excluding {excludeAccounts.Count} already used on this job");
            accounts = (await AccountManager.PickMultipleRandom(accountsToUse, excludeAccounts, accountGroupId)).ToArray();
        }
        else
        {
            await Logger.LogDebug(work,
                $"Picking from previous work accounts - Excluding {excludeAccounts.Count} already used on this job");

            if (work.PreviousWorkRequest != null)
                accounts = (await AccountManager.PickWorkAccounts(work.PreviousWorkRequest, accountsToUse,
                    excludeAccounts)).ToArray();
        }

        work.AccountsToUse = accounts.Length;
        
        await context.SaveChangesAsync();

        if (!await ValidateAccounts(work, accounts))
            return null;

        return accounts;
    }

    protected async Task<HashSet<string>> CacheFriends(SnapchatAccountModel account, ProxyGroup proxyGroup, CancellationToken cancellationToken)
    {
        var sync = await Runner.SyncFriends(account, proxyGroup, cancellationToken);
        
        if(sync != null)
        {
            var friends = sync.friends.AsParallel().Where(x => x.type == 0 && x.mutable_username != "teamsnapchat").Select(x=> x.mutable_username).ToList();
            var addedFriends = sync.added_friends.AsParallel().Where(x => x.type is 1 or 0 or 6 && x.mutable_username != "teamsnapchat").Select(x => x.mutable_username).ToList();
            var combinedFriends = friends.Concat(addedFriends).Distinct().ToList();
            
            return combinedFriends.ToHashSet();
        }
        else
        {
            throw new Exception("SyncFriends Was Null");
        }
    }
    
    protected async Task<HashSet<string>> CacheTargets(SnapchatAccountModel account, ProxyGroup proxyGroup, CancellationToken cancellationToken)
    {
        var sync = await Runner.SyncFriends(account, proxyGroup, cancellationToken);
        
        if(sync != null)
        {
            var friends = sync.friends.AsParallel().Where(x => x.mutable_username != "teamsnapchat").Select(x=> x.mutable_username).ToList();
            var addedFriends = sync.added_friends.AsParallel().Where(x => x.mutable_username != "teamsnapchat").Select(x => x.mutable_username).ToList();
            var combinedFriends = friends.Concat(addedFriends).Distinct().ToList();
            
            return combinedFriends.ToHashSet();
        }
        else
        {
            throw new Exception("SyncFriends Was Null");
        }
    }
    
    protected async Task<bool> IsFriend(SnapchatAccountModel account, string username, ProxyGroup proxyGroup, CancellationToken cancellationToken)
    {
        var accFriends = await CacheFriends(account, proxyGroup, cancellationToken);

        return Enumerable.Contains(accFriends, username, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsUpdateAccountStatusException(WorkRequest work, Exception ex, SnapchatAccountModel account)
    {
        if (ex.Message.Contains("UnauthorizedAuthTokenException"))
        {
            Logger.LogError(work, $"{account.Username}: Unauthorized Auth Token (Possibly needs relogged)", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.NEEDS_RELOG);
            return true;
        }
        
        if (ex.Message.Contains("AccountBannedException"))
        {
            Logger.LogError(work, $"{account.Username}: Account BANNED", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.BANNED);
            return true;
        }
        
        if (ex.Message.Contains("ProxyTimeoutException"))
        {
            Logger.LogError(work, $"{account.Username}: Proxy Timeout", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.BAD_PROXY);
            return true;
        }
        
        if (ex.Message.Contains("RateLimitedException"))
        {
            Logger.LogError(work, $"{account.Username}: Rate Limited", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.RATE_LIMITED);
            return true;
        }
        
        if (ex.Message.Contains("DeadAccountException"))
        {
            Logger.LogError(work, $"{account.Username}: Account BANNED", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.BANNED);
            return true;
        }
        
        if (ex.Message.Contains("ProxyAuthRequiredException"))
        {
            Logger.LogError(work, $"{account.Username}: Invalid Proxy Credentials", account).Wait();
            account.SetStatus(AccountManager, AccountStatus.BAD_PROXY);
            return true;
        }

        switch (ex)
        {
            case AccountBannedException:
            {
                Logger.LogError(work, $"{account.Username}: Account BANNED", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.BANNED);
                return true;
            }
            case UnauthorizedAuthTokenException:
            {
                Logger.LogError(work, $"{account.Username}: Unauthorized Auth Token", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.NEEDS_RELOG);
                return true;
            }
            case ProxyTimeoutException:
            {
                Logger.LogError(work, $"{account.Username}: Proxy Timeout", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.BAD_PROXY);
                return true;
            }
            case RateLimitedException:
            {
                Logger.LogError(work, $"{account.Username}: Rate Limited", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.RATE_LIMITED);
                return true;
            }
            case UsernameNotFoundException:
            {
                return true;
            }
            case DeadAccountException:
            {
                Logger.LogError(work, $"{account.Username}: Account BANNED", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.BANNED);
                return true;
            }
            case ProxyAuthRequiredException:
            {
                Logger.LogError(work, $"{account.Username}: Invalid Proxy Credentials", account).Wait();
                account.SetStatus(AccountManager, AccountStatus.BAD_PROXY);
                return true;
            }
            default:
                return false;
        }
    }

    protected async Task<bool> TryAcceptFriend(WorkRequest work, SnapchatAccountModel account, ProxyGroup? proxyGroup, string targetUser)
    {
        /*try
        {
            if (await IsFriend(account, targetUser, proxyGroup, work.CancellationTokenSource.Token))
            {
                await Logger.LogDebug(work, $"User {targetUser} is already a friend", account);
                return true;
            }
        }
        catch (Exception e) when (IsUpdateAccountStatusException(work, e, account))
        {
            return false;
        }
        catch (SocketException se)
        {
            await Logger.LogError(work, $"Unexpected issue when validating if user {targetUser} is friend.", account);
            Scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
            return false;
        }
        catch (Exception ex) // account banned exceptions need to go higher 
        {
            await Logger.LogError(work, $"Unexpected issue when validating if user {targetUser} is friend.", account);
            return false;
        }*/ /// This shit causes too many failures at the moment.

        try
        {
            // Add friend
            await Logger.LogDebug(work, $"Accepting {targetUser} as friend", account);
            var response = await Runner.AcceptFriend(account, targetUser, proxyGroup, work.CancellationTokenSource.Token);

            if (response.FailuresArray.Any())
            {
                foreach (var failResponse in response.FailuresArray)
                {
                    if (failResponse.Reason == SCFriendingFriendActionFailure.Types.SCFriendingFriendActionFailure_FailureReason.ErrorNoPermission || failResponse.Reason == SCFriendingFriendActionFailure.Types.SCFriendingFriendActionFailure_FailureReason.AddReachLimit)
                    {
                        account.SetStatus(AccountManager, AccountStatus.RATE_LIMITED);
                    }
                }
                
                return false;
            }

            return true;
        }
        catch (Exception e) when (IsUpdateAccountStatusException(work, e, account))
        {
            return false;
        }
        catch (Exception ex) // account banned exceptions need to go higher
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            await Logger.LogError(work, $"Unexpected issue when trying to add friend {targetUser}.", account);
            return false;
        }
    }
    
    public async Task<MediaFile?> GetMediaFileOrCancelJob(WorkRequest work, PostDirectSingleSnap snap)
    {
        var media = await snap.GetMediaPath(ServiceProvider);
                    
        if (media == null)
        {
            CancelWork(work, $"Requested media with id {snap.MediaFileId} was not found in the database", WorkStatus.Error);
            return null;
        }

        if (!File.Exists(media.ServerPath))
        {
            CancelWork(work, "Requested media file was not found in the server", WorkStatus.Error);
            return null;
        }

        return media;
    }
    
    protected async Task<bool> TryAddFriend(WorkRequest work, SnapchatAccountModel account, ProxyGroup? proxyGroup, string targetUser)
    {
        /*try
        {
            if (await IsFriend(account, targetUser, proxyGroup, work.CancellationTokenSource.Token))
            {
                await Logger.LogDebug(work, $"User {targetUser} is already a friend", account);
                return true;
            }
        }
        catch (Exception e) when (IsUpdateAccountStatusException(work, e, account))
        {
            return false;
        }
        catch (SocketException se)
        {
            await Logger.LogError(work, $"Unexpected issue when validating if user {targetUser} is friend.", account);
            await Scheduler.FailWorkAccount(work, account).WaitAsync(work.CancellationTokenSource.Token);
            return false;
        }
        catch (Exception ex) // account banned exceptions need to go higher 
        {
            await Logger.LogError(work, $"Unexpected issue when validating if user {targetUser} is friend.", account);
            return false;
        }*/ /// This shit causes too many failures at the moment.

        try
        {
            // Add friend
            await Logger.LogDebug(work, $"Adding {targetUser} as friend", account);
            
            var response = await Runner.AddFriend(account, targetUser, proxyGroup, work.CancellationTokenSource.Token);

            if (response.FailuresArray.Any())
            {
                foreach (var failResponse in response.FailuresArray)
                {
                    if (failResponse.Reason == SCFriendingFriendActionFailure.Types.SCFriendingFriendActionFailure_FailureReason.ErrorNoPermission || failResponse.Reason == SCFriendingFriendActionFailure.Types.SCFriendingFriendActionFailure_FailureReason.AddReachLimit)
                    {
                        account.SetStatus(AccountManager, AccountStatus.RATE_LIMITED);
                        await Scheduler.FailWorkAccount(work, account).WaitAsync(work.CancellationTokenSource.Token);
                    }
                }
                
                return false;
            }
            
            await Logger.LogDebug(work, $"Added {targetUser} as friend", account);

            return true;
        }
        catch (Exception e) when (IsUpdateAccountStatusException(work, e, account))
        {
            return false;
        }
        catch (Exception ex) // account banned exceptions need to go higher
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            await Logger.LogError(work, $"Unexpected issue when trying to add friend {targetUser}.", account);
            return false;
        }
    }

    private async Task<bool> CheckStart<T>(WorkRequest work, T arguments) where T: ActionArguments
    {
        await Logger.LogInformation(work, $"Starting {work.Action} work with Id: {work.Id}");
        return await Scheduler.EndWorkForInvalidArguments(work, arguments);
    }

    private async Task<bool> CheckRequiredMedia(WorkRequest work)
    {
        if (work.MediaFile != null && File.Exists(work.MediaFile.ServerPath)) return true;
        
        await Logger.LogError(work, $"Input file was not found in the server. Validate that upload happened correctly");
        await Scheduler.EndWork(work, WorkStatus.Error);
        return false;
    }
    
    private async Task WaitForAccount(WorkRequest work, SnapchatAccountModel account)
    {
        var loggedOnce = false;
        while (AccountTracker.IsUsed(account))
        {
            if (!loggedOnce)
            {
                Logger.LogInformation(work, "Account is in use. Waiting until it becomes free for processing", account);
                loggedOnce = true;
            }

            await Task.Delay(2000, work.CancellationTokenSource.Token);
        }

        AccountTracker.Track(account);
    }

    protected virtual void TaskCleanup(SnapchatAccountModel account)
    {
        AccountTracker.UnTrack(account);
    }

    private async Task<IEnumerable<Task<WorkStatus>>> CreateAndRunTasks<T>(WorkRequest work, T arguments, SnapchatActionsWorker worker, WorkDelegate<T> workDelegate, TaskCleanupDelegate? cleanupDelegate, int accountCount) where T: ActionArguments
    {
        var accounts = await PickAccounts(accountCount, work, arguments.AccountGroupToUse);

        if (accounts.Length < accountCount)
            await Logger.LogInformation(work, $"There are no enough available accounts to execute all requested tasks. Only {accounts.Length} will be executed.");
        
        var tasks = new List<Task<WorkStatus>>();
        foreach (var account in accounts)
        {
            if (work.CancellationTokenSource.IsCancellationRequested) break;

            var task = new Task<WorkStatus>(() =>
            {
                try
                {
                    WaitForAccount(work, account).Wait(work.CancellationTokenSource.Token);
                    return work.CancellationTokenSource.IsCancellationRequested ? WorkStatus.Cancelled : workDelegate(work, arguments, account).Result;
                }
                finally
                {
                    cleanupDelegate?.Invoke(account);   
                }
            });
            
            worker.QueueTask(new SnapchatTask() { InnerTask = task, WorkRequest = work});
            tasks.Add(task);
        }
        
        await worker.WaitTasksCompletion(work, tasks, work.CancellationTokenSource.Token);

        return tasks;
    }
    
    protected async Task Start<T>(WorkRequest work, T arguments, SnapchatActionsWorker worker, WorkDelegate<T> workDelegate, TaskCleanupDelegate? cleanupDelegate, bool useArgumentsAccountToUse = false) where T: ActionArguments
    {
        try
        {
            if (!await CheckStart(work, arguments)) return;

            if (CheckForMedia && !await CheckRequiredMedia(work)) return;

            var accountCount = useArgumentsAccountToUse ? arguments.AccountsToUse : work.AccountsLeft;

            await Scheduler.UpdateWorkStartData(work);

            if (work.ChainDelayMs is > 0)
            {
                await Logger.LogInformation(work, $"Waiting for setup Chain Delay of {work.ChainDelayMs}ms");
                await Task.Delay((int)work.ChainDelayMs);
            }

            if (work.CancellationTokenSource is { IsCancellationRequested: true })
            {
                await Scheduler.EndWork(work);
                return;
            }

            var tasks = await CreateAndRunTasks(work, arguments, worker, workDelegate, cleanupDelegate, accountCount);

            var tasksToRetry = tasks.Count(t => t.Result == WorkStatus.Retry);

            if (tasksToRetry > 0)
                await CreateAndRunTasks(work, arguments, worker, workDelegate, cleanupDelegate, tasksToRetry);
        }
        finally
        {
            await Scheduler.EndWork(work);
        }
    }


    
    protected async Task<List<HashSet<string>>?> CacheAndGetFriendsSplit(WorkRequest work, SnapchatAccountModel account, ProxyGroup? proxyGroup, int binSize = 50)
    {
        var friends = await CacheFriends(account, proxyGroup, work.CancellationTokenSource.Token);
        var utilities = ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<Utilities>();
        
        return utilities.SplitHash(friends, binSize);
    }

    protected async Task<List<HashSet<string>>?> CacheAndGetTargetsSplit(WorkRequest work, SnapchatAccountModel account, ProxyGroup? proxyGroup, int binSize = 50)
    {
        var friends = await CacheTargets(account, proxyGroup, work.CancellationTokenSource.Token);
        var utilities = ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<Utilities>();
        
        return utilities.SplitHash(friends, binSize);
    }

    protected async Task<IEnumerable<string>> AddTargetsAsFriends(WorkRequest work, SnapchatAccountModel account, List<TargetUser> targets, ProxyGroup? proxyGroup)
    {
        var tempUsers = new List<TargetUser>();

        foreach (var target in targets)
        {
            // TryAddFriend will never throw an exception
            if (!await TryAddFriend(work, account, proxyGroup, target.Username)) continue;
                    
            tempUsers.Add(target);

            await TargetManager.SaveTargetUpdates(target);

            if (target == targets.Last()) break;
            await Task.Delay(15);
        }

        return tempUsers.Select(t => t.Username);
    }

    private async Task CreateChosenTargets(WorkRequest work, List<TargetUser> targets)
    {
        // Now we need to create the ChosenTarget records in the db
        await using var context =
            ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var target in targets)
        {
            var chosenRecord = new ChosenTarget()
            {
                TargetUserId = target.Id,
                WorkId = work.Id
            };
            context.Add(chosenRecord);
        }

        await context.SaveChangesAsync();
    }

    protected async Task SetTaskTargets<T>(WorkRequest work, T arguments) where T: MessagingArguments
    {
        // When chaining, we get the previous work results
        if (work.PreviousWorkRequestId != null)
        {
            TargetUsers = (await TargetManager.GetWorkTargetUsers(work.PreviousWorkRequest)).ToList();
        } else if (arguments.RandomUsers)
        {
            TargetUsers = (await TargetManager.GetRandomTargetNames(arguments.RandomTargetAmount, false, false, arguments.CountryFilter, arguments.RaceFilter, arguments.GenderFilter)).ToList();
        }
        else
        {
            TargetUsers = (await TargetManager.FromStrings(arguments.Users)).ToList();
        }
        
        await CreateChosenTargets(work, TargetUsers);
    }

    private bool IsValid(TargetUser compare, bool addedOnly, string country, string race, string gender)
    {
        if (compare != null && country != null && race != null && gender != null)
        {
            int score = 0;

            if (country.Equals("ARABIC COUNTRIES"))
            {
                if (Utilities.ArabicCountries.Contains(country))
                {
                    score++;
                }
            }

            if (compare.CountryCode.Equals(country) || country.Equals("ANY"))
            {
                score++;
            }
            
            if (compare.Race.Equals(race) || race.Equals("ANY"))
            {
                score++;
            }
            
            if (compare.Gender.Equals(gender) || gender.Equals("ANY"))
            {
                score++;
            }

            if (compare.Added && !addedOnly)
            {
                score = 0;
            }
            
            if (score >= 3)
            {
                return true;
            }
        }

        return false;
    }

    protected void ProcessAggregateException(Exception e, WorkRequest work, SnapchatAccountModel account)
    {
        if (e is not NoAvailableProxyException)
        {
            // If we don't handle this, then the exception is not caught :(
            Logger.LogError(work, e, account)
                .Wait(work.CancellationTokenSource.Token);
            Scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
            return;
        }

        Logger.LogError(work, "No proxies found").Wait(work.CancellationTokenSource.Token);
        Scheduler.EndWork(work, WorkStatus.Error).Wait();
        work.CancellationTokenSource.Cancel();
    }

    protected void CancelWork(WorkRequest work, string errorMsg, WorkStatus status)
    {
        Logger.LogError(work, errorMsg).Wait(work.CancellationTokenSource.Token);
        Scheduler.EndWork(work, status).Wait();
        work.CancellationTokenSource.Cancel();
    }
}