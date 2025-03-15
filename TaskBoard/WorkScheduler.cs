using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard;

public class WorkScheduler
{
    private readonly ILogger<WorkScheduler> _logger;
    private readonly WorkLogger _workLogger;
    private readonly WorkRequestTracker _workRequestTracker;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SnapchatAccountManager _accountManager;
    private readonly IEmailSender _emailSender;
    private readonly SnapWebManagerClient _managerClient;
    
    public WorkScheduler() {}

    public WorkScheduler(SnapWebManagerClient managerClient, IEmailSender emailSender, ILogger<WorkScheduler> logger, WorkLogger workLogger, WorkRequestTracker workRequestTracker, IServiceScopeFactory scopeFactory, SnapchatAccountManager accountManager)
    {
        _logger = logger;
        _workLogger = workLogger;
        _workRequestTracker = workRequestTracker;
        _scopeFactory = scopeFactory;
        _accountManager = accountManager;
        _emailSender = emailSender;
        _managerClient = managerClient;
    }

    public async Task<IEnumerable<WorkRequest>> GetPendingJobs(CancellationToken cancellationToken)
    {
        await using var context  = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await context.WorkRequests!
            .Where(r =>
                r.Status == WorkStatus.NotRun || r.Status == WorkStatus.Waiting)
            .Include(e => e.MediaFile)
            .Include(e => e.PreviousWorkRequest)
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .Where(e => 
                e is { IsFinished: false, IsScheduled: false, PreviousWorkRequestId: null } ||
                (e.PreviousWorkRequestId != null && e.PreviousWorkRequest.IsFinished));
    }

    public IEnumerable<WorkRequest> GetPendingCancellationJobs(CancellationToken cancellationToken)
    {
        using var context  = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.WorkRequests!.AsNoTracking().ToListAsync(cancellationToken).Result.Where(r => r.Status == WorkStatus.PendingCancellation);
    }

    public async Task UpdateWork(WorkRequest work)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Update(work);

        await context.SaveChangesAsync();
    }

    private async Task CancelDependents(WorkRequest originatingWork)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var toCancel = await context.WorkRequests.Where(e => e.PreviousWorkRequestId == originatingWork.Id && (e.Status == WorkStatus.NotRun || e.Status == WorkStatus.Waiting)).ToListAsync();
        if (toCancel.Count > 0) foreach (var dependent in toCancel) await CancelWork(dependent);
    }

    public async Task CancelWork(WorkRequest work)
    {
        work.Status = WorkStatus.PendingCancellation;
        await UpdateWork(work);
        
        // We also need to cancel the complete chain of jobs
        await CancelDependents(work);
        
        if (!_workRequestTracker.GetTrackedWork(work, out var runningJob)) return;
        runningJob.Status = WorkStatus.PendingCancellation;
    }

    public async Task CancelAllJobs()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        foreach (var job in context.WorkRequests)
        {
            bool isRunning = _workRequestTracker.GetTrackedWork(job, out var runningJob);
                
            if (isRunning)
            {
                await CancelWork(job);
            }
        }
    }

    public async Task FailWorkAccount(WorkRequest work, SnapchatAccountModel? account = null)
    {
        if (account == null)
        {
            work.AccountsFail++;
            await UpdateWork(work);
            return;
        }

        if (work.FailedAccounts != null && work.FailedAccounts.Contains(account.Username))
        {
            // account.Username already exists in failedAccounts
            return;
        }
        
        if (work.FailedAccounts is { Length: > 0 })
        {
            work.FailedAccounts += $", {account.Username}";
        }
        else
        {
            work.FailedAccounts = $"{account.Username}";
        }
        
        work.AccountsFail++;
        await UpdateWork(work);
    }

    public async Task UpdateWorkStartData(WorkRequest work)
    {
        work.StartTime = DateTime.UtcNow;
        work.Status = WorkStatus.Waiting;
        await UpdateWork(work);
    }

    public async Task UpdateWorkAddPass(WorkRequest work)
    {
        work.AccountsPass++;
        await UpdateWork(work);
    }

    private async Task FinishWork(WorkRequest work, WorkStatus status)
    {
        work.FinishTime = DateTime.UtcNow;
        work.Status = status;
        _workRequestTracker.Untrack(work);
        await UpdateWork(work);
    }

    private async Task JobEnding(WorkRequest work)
    {
        switch (work.Action)
        {
            case WorkAction.ExportFriends:
                var arguments = JsonConvert.DeserializeObject<ExportFriendsArguments>(work.Arguments);

                if (work.ExportedFriends != null && work.ExportedFriends.Length > 0)
                {
                    await _emailSender.SendEmailAsync(arguments.ExportEmail, "Friends List",
                        work.ExportedFriends);
                }

                break;
            default:
                break;
        }
    }
    
    public async Task EndWork(WorkRequest work, WorkStatus status, string? reason = null)
    {
        await JobEnding(work);
        await _workLogger.LogDebug(work, $"Finishing job with status {Enum.GetName(typeof(WorkStatus), status)}");
        
        if (!reason.IsNullOrEmpty())
        {
            await _workLogger.LogDebug(work, $"Job Finish Reason: {reason}");
        }
        
        await FinishWork(work, status);
    }

    public async Task<WorkRequest> GetWorkFromDb(long workId)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.WorkRequests.FindAsync(workId);
    }
    
    public async Task EndWork(WorkRequest work)
    {
        // When we cancel, the status change is made in the db and it does not reflect always on the incoming object
        // so get the status from it and use that to avoid marking the job as "Incomplete" instead of "Cancelled"

        var currentData = await GetWorkFromDb(work.Id);
        work.Status = currentData.Status;
        
        switch (work.Status)
        {
            // Check if our work has been cancelled. And if so, do nothing
            case WorkStatus.Cancelled:
                await FinishWork(work, work.Status);
                return;
            // if error we do not calculation and just finish as is
            case WorkStatus.Error:
                await EndWork(work, WorkStatus.Error);
                return;
        }

        // Start from incomplete
        var workStatus = WorkStatus.Incomplete;

        // If no failures then it should be ok
        if (work.AccountsLeft <= 0 && work.AccountsFail == 0)
            workStatus = WorkStatus.Ok;

        await EndWork(work, workStatus);
    }
    
    public async Task<bool> EndWorkForInvalidArguments(WorkRequest work, ActionArguments arguments)
    {
        // First make sure the arguments are ok
        var validationResult = arguments.Validate();
        if (validationResult.Exception == null) return true;
        
        await _workLogger.LogError(work, "Finishing because of invalid arguments");
        await EndWork(work, WorkStatus.Error);
        return false;
    }

    public async Task<bool> IsCancelled(WorkRequest work)
    {
        var current = await GetWorkFromDb(work.Id);
        return current.Status == WorkStatus.Cancelled;
    } 

    private async Task SaveToDb(WorkRequest work)
    {
        await using var context  = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.WorkRequests.Add(work);
        await context.SaveChangesAsync();
    }

    private async Task SaveToDb(WorkRequest work, MediaFile? file)
    {
        await using var context  = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (file != null)
            context.Attach(file);
        
        context.WorkRequests.Add(work);
        await context.SaveChangesAsync();
    }

    // Create the work request associated with an action with the required default values
    private WorkRequest CreateWorkRequest<T>(WorkAction action, T arguments, int actionsPerAccount = 0) where T: ActionArguments
    {
        return new WorkRequest()
        {
            AccountsToUse = arguments.AccountsToUse,
            Action = action,
            RequestTime = DateTime.UtcNow,
            ScheduledTime = arguments.ScheduledTime,
            Arguments = arguments.ToString(),
            PreviousWorkRequestId = arguments.PreviousWorkRequestId,
            ChainDelayMs = arguments.ChainDelayMs,
            ActionsPerAccount = actionsPerAccount,
        };
    }

    public async Task<WorkRequest> Subscribe(SubscribeArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.Subscribe, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> ChangeUsername(ChangeUsernameArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ChangeUsername, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> FindUsersViaSearch(FindUsersViaSearchArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.FindUsersViaSearch, arguments, arguments.ActionsPerAccount);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> PhoneToUsername(PhoneSearchArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.PhoneToUsername, arguments, arguments.ActionsPerAccount);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> EmailToUsername(EmailSearchArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.EmailToUsername, arguments, arguments.ActionsPerAccount);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> ReportUserRandom(ReportUserRandomArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ReportUserRandom, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> ReportUserPublicProfileRandom(ReportUserPublicProfileRandomArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ReportUserPublicProfileRandom, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> PostDirect(PostDirectArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.PostDirect, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> SendMessage(SendMessageArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.SendMessage, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> SendMention(SendMentionArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.SendMention, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> CreateAccounts(CreateAccountArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.CreateAccounts, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> ViewBusinessPublicStory(ViewBusinessPublicStoryArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ViewBusinessPublicStory, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> ReportUserStoryRandom(ReportUserStoryRandomArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ReportUserRandom, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> ViewPublicStory(ViewPublicStoryArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ViewPublicStory, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> AddFriend(AddFriendArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.AddFriend, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> AcceptFriend(AcceptFriendArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.AcceptFriend, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> FriendCleaner(FriendCleanerArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.FriendCleaner, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> QuickAdd(QuickAddArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.QuickAdd, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> RefreshFriends(RefreshFriendArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.RefreshFriends, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> ExportFriends(ExportFriendsArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.ExportFriends, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> RelogAccounts(RelogAccountArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.RelogAccounts, arguments);
        await SaveToDb(work);
        return work;
    }

    public async Task<WorkRequest> Test(TestArguments arguments)
    {
        var work = CreateWorkRequest(WorkAction.TestWork, arguments);
        await SaveToDb(work);
        return work;
    }
    
    public async Task<WorkRequest> PostStory(PostStoryArguments arguments, MediaFile file)
    {
        var work = CreateWorkRequest(WorkAction.PostStory, arguments);
        var fileRefs = file.WorkRequests?.ToList() ?? new List<WorkRequest>();
        fileRefs.Add(work);
        file.WorkRequests = fileRefs;
        await SaveToDb(work, file);
        return work;
    }
}