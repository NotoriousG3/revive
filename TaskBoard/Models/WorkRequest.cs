using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;

public enum WorkAction
{
    PostDirect,
    ReportUserPublicProfileRandom,
    ReportUserRandom,
    SendMessage,
    Subscribe,
    CreateAccounts,
    ViewBusinessPublicStory,
    AddFriend,
    TestWork,
    PostStory,
    FindUsersViaSearch,
    PhoneToUsername,
    EmailToUsername,
    SendMention,
    AcceptFriend,
    RefreshFriends,
    QuickAdd,
    FriendCleaner,
    ViewPublicStory,
    ReportUserStoryRandom,
    ChangeUsername,
    RelogAccounts,
    ExportFriends
}

/// <summary>
/// A work flow should be: NotRun (inserted in DB) -> Waiting (taken by worker) -> Error/Ok/Cancelled/Incomplete
/// PendingCancellation is a transitory state to Cancelled
/// Cancelled: Work has been cancelled
/// Waiting: Work has iterations pending. This is a transitory state to one the following:
/// Retry is an INTERNAL task status, it should be moved in the future IMO
/// Error: Work finished with error
/// Ok: Work finished without errors and ALL iterations PASSED
/// Incomplete: Work finished without errors, but some iterations FAILED
/// </summary>
public enum WorkStatus
{
    NotRun = 0,
    Error = 1,
    Incomplete = 2,
    Ok = 3,
    Cancelled = 4,
    PendingCancellation = 5,
    Retry = 6,
    Waiting = 7
}

public class WorkRequest
{
    [NotMapped]
    public CancellationTokenSource? CancellationTokenSource;

    [Key] public long Id { get; set; }

    public WorkAction Action { get; set; }
    public int AccountsToUse { get; set; }
    public string Arguments { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public int AccountsFail { get; set; }
    public int AccountsPass { get; set; }
    public int ActionsPerAccount { get; set; }
    public string? FailedAccounts { get; set; }
    public string? ExportedFriends { get; set; }
    public WorkStatus Status { get; set; }
    public List<LogEntry> Logs { get; set; }

    [ForeignKey("MediaFile")]
    public long? MediaFileId { get; set; }
    public MediaFile? MediaFile { get; set; }
    public long? PreviousWorkRequestId { get; set; } = null;
    public WorkRequest? PreviousWorkRequest { get; set; }
    public long? ChainDelayMs { get; set; } = null;
    public bool IsFinished => (AccountsLeft <= 0);
    public bool IsRunning(WorkRequestTracker tracker) => (!IsFinished && tracker.GetTrackedWork(this, out _));
    public bool IsScheduled => Status == WorkStatus.NotRun && ScheduledTime != null && ScheduledTime > DateTime.UtcNow;
    public int AccountsLeft => (AccountsToUse - AccountsFail - AccountsPass);
    [NotMapped]
    public bool AccountsById { get; set; } = false;
    public int MinFriends { get; set; } = 0;
    public int MaxFriends { get; set; } = 50000;
    [NotMapped]
    public IEnumerable<string>? AssignedAccounts { get; set; } = null;
}

public class UIWorkRequest
{
    [Key] public long Id { get; set; }
    public WorkAction Action { get; set; }
    public int AccountsToUse { get; set; }
    public string Arguments { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public int AccountsFail { get; set; }
    public int AccountsPass { get; set; }
    public int ActionsPerAccount { get; set; }
    public WorkStatus Status { get; set; }
    public bool IsFinished => AccountsLeft <= 0;
    public bool IsRunning { get; set; }
    public bool IsScheduled => Status == WorkStatus.NotRun && ScheduledTime != null && ScheduledTime > DateTime.UtcNow;
    public int AccountsLeft => AccountsToUse - AccountsFail - AccountsPass;
    
    public static IEnumerable<UIWorkRequest> ToEnumerable(List<WorkRequest> workRequests, WorkRequestTracker tracker)
    {
        return workRequests.Select(work => new UIWorkRequest
        {
            Id = work.Id,
            Action = work.Action,
            Arguments = work.Arguments,
            AccountsToUse = work.AccountsToUse,
            RequestTime = work.RequestTime,
            ScheduledTime = work.ScheduledTime,
            StartTime = work.StartTime,
            FinishTime = work.FinishTime,
            AccountsFail = work.AccountsFail,
            AccountsPass = work.AccountsPass,
            ActionsPerAccount = work.ActionsPerAccount,
            Status = work.Status,
            IsRunning = work.IsRunning(tracker)
        });
    }
}