using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;
using TaskBoard.WorkTask;
using WorkRequest = TaskBoard.Models.WorkRequest;

namespace TaskBoard;

public class SnapchatActionsWorker : Worker
{
    private readonly WorkRequestTracker _workRequestTracker;
    
    private Timer? _settingsUpdateTimer;

    private Timer? _timer;
    private bool enabled;
    private LoadBalancer loadBalancer;
    private readonly Dictionary<WorkAction, Func<WorkRequest, string, Task>> _workMethods;
    private readonly ILogger<Worker> _logger;

    public SnapchatActionsWorker(IServiceProvider serviceProvider, ILogger<SnapchatActionsWorker> logger, WorkRequestTracker workRequestTracker) : base(serviceProvider, logger)
    {
        _logger = logger;
        _workRequestTracker = workRequestTracker;
        _workRequestTracker.OnJobFinish += CleanWork;
        _workMethods = new Dictionary<WorkAction, Func<WorkRequest, string, Task>>
        {
            { WorkAction.Subscribe, (work, args) => SubscribeWork(work, args) },
            { WorkAction.ReportUserPublicProfileRandom, (work, args) => ReportUserPublicProfileRandomWork(work, args) },
            { WorkAction.ReportUserRandom, (work, args) => ReportUserRandomWork(work, args) },
            { WorkAction.PostDirect, (work, args) => PostDirectWork(work, args) },
            { WorkAction.SendMention, (work, args) => SendMentionWork(work, args) },
            { WorkAction.SendMessage, (work, args) => SendMessageWork(work, args) },
            { WorkAction.CreateAccounts, (work, args) => CreateAccountsWork(work, args) },
            { WorkAction.ViewBusinessPublicStory, (work, args) => ViewBusinessPublicStoryWork(work, args) },
            { WorkAction.AddFriend, (work, args) => AddFriendWork(work, args) },
            { WorkAction.TestWork, (work, args) => TestWork(work, args) },
            { WorkAction.PostStory, (work, args) => PostStoryWork(work, args) },
            { WorkAction.FindUsersViaSearch, (work, args) => FindUsersViaSearchWork(work, args) },
            { WorkAction.PhoneToUsername, (work, args) => PhoneScraperWork(work, args) },
            { WorkAction.EmailToUsername, (work, args) => EmailScraperWork(work, args) },
            { WorkAction.AcceptFriend, (work, args) => AcceptFriendWork(work, args) },
            { WorkAction.QuickAdd, (work, args) => QuickAddWork(work, args) },
            { WorkAction.RefreshFriends, (work, args) => RefreshFriendWork(work, args) },
            { WorkAction.FriendCleaner, (work, args) => FriendCleanerWork(work, args) },
            { WorkAction.ViewPublicStory, (work, args) => ViewPublicStoryWork(work, args) },
            { WorkAction.ChangeUsername, (work, args) => ChangeUsernameWork(work, args) },
            { WorkAction.RelogAccounts, (work, args) => RelogAccountsWork(work, args) },
            { WorkAction.ExportFriends, (work, args) => ExportFriendWork(work, args) },
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var logger = WorkLogger.GetFromServiceProvider(_serviceProvider);
        try
        {
            var settings = await AppSettings.GetSettingsFromProvider(_serviceProvider);
            MaxTasks = settings.MaxTasks;
            MaxThreads = settings.Threads;
            enabled = true;
            loadBalancer = new(Environment.ProcessorCount);
            
            await logger.LogInformation(null, "Starting Snapchat Actions Worker Service");
            _settingsUpdateTimer = new Timer(CheckSettingsWork, cancellationToken, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            _timer = new Timer(DoWork, cancellationToken, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(3));
        }
        catch (ApiKeyNotSetException)
        {
            // Mark the flag as false so that we don't try processing jobs without an api key
            enabled = false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var logger = WorkLogger.GetFromServiceProvider(_serviceProvider);
        await logger.LogInformation(null, "Snapchat Actions Worker Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        _settingsUpdateTimer?.Change(Timeout.Infinite, 0);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        _settingsUpdateTimer?.Dispose();
    }

    private async void CheckSettingsWork(object? state)
    {
        try
        {
            var settings = await AppSettings.GetSettingsFromProvider(_serviceProvider);
            MaxTasks = settings.MaxTasks;
            MaxThreads = settings.Threads;
            enabled = true;
        }
        catch (ApiKeyNotSetException)
        {
            enabled = false;
        }
    }

    private async Task CheckCancelledJobs(WorkScheduler scheduler, CancellationToken cancellationToken)
    {
        // Loop over works in the DB that are marked for cancellation
        var jobsToCancel = scheduler.GetPendingCancellationJobs(cancellationToken);

        foreach (var work in jobsToCancel)
        {
            // First we retrieve the work from the current ones that are running. Otherwise we'll lose it when EndWork runs
            if (!_workRequestTracker.GetTrackedWork(work, out var runningJob))
            {
                // Mark the reference in the DB as cancelled only
                await scheduler.EndWork(work, WorkStatus.Cancelled);
                continue;
            }
            
            await SaveAndRemoveWork(work, WorkStatus.Cancelled);
            CleanWork(runningJob);
            runningJob.CancellationTokenSource.Cancel();
            runningJob.Status = WorkStatus.Cancelled;
        }
    }

    private bool HasAvailableWorkSlot(AppSettings settings)
    {
        return _workRequestTracker.RunningWorks() < settings.MaxTasks;
    }

    private async void DoWork(object? state)
    {
        // Exit early if the service is not enabled
        if (!enabled) return;

        using var scope = _serviceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        var cancellationToken = state != null ? (CancellationToken) state : CancellationToken.None;

        await CheckCancelledJobs(scheduler, cancellationToken);
        
        ProcessJobQueues();

        var pendingJobs = (await scheduler.GetPendingJobs(cancellationToken)).Where(r => !_workRequestTracker.GetTrackedWork(r, out _));

        var settingsLoader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
        var settings = await settingsLoader.Load();
        var expireDate = settings.AccessDeadline;
        
        if (DateTime.UtcNow >= expireDate)
        {
            await scheduler.CancelAllJobs();

            return;
        }
        
        foreach (var work in pendingJobs)
        {
            if (!HasAvailableWorkSlot(settings)) return;

            // For each starting work, we assign a new token for cancellation
            var tokenSource = new CancellationTokenSource();
            work.CancellationTokenSource = tokenSource;

            _workRequestTracker.Track(work);

            if (work.StartTime == null) await scheduler.UpdateWorkStartData(work);

// We do not want to await this since we don't care about the completion of its execution            
#pragma warning disable 4014
            KickoffWork(work);
#pragma warning restore 4014
        }
    }

    private void KickoffWork(WorkRequest work)
    {
        if (!_workMethods.TryGetValue(work.Action, out var method))
        {
            _logger.LogDebug($"Unsupported work action: {work.Action}");
            return;
        }

        //method(work, work.Arguments);
            
        loadBalancer.AddTask(method(work, work.Arguments));
    }
    
    private async Task SaveAndRemoveWork(WorkRequest work, WorkStatus status)
    {
        using var scope = _serviceProvider.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<WorkScheduler>();
        await scheduler.EndWork(work, status);
    }

    private async Task SubscribeWork(WorkRequest work, SubscribeArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<SubscribeTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task ReportUserRandomWork(WorkRequest work, ReportUserRandomArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ReportUserRandomTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task ReportUserPublicProfileRandomWork(WorkRequest work, ReportUserPublicProfileRandomArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ReportPublicProfileTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task FindUsersViaSearchWork(WorkRequest work, FindUsersViaSearchArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<FindUsersViaSearchTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }

    private async Task RefreshFriendWork(WorkRequest work, RefreshFriendArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<RefreshFriendTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task PhoneScraperWork(WorkRequest work, PhoneSearchArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<PhoneScrapeTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }
        
        
    private async Task EmailScraperWork(WorkRequest work, EmailSearchArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<EmailScraperTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);       
    }

    private async Task PostDirectWork(WorkRequest work, PostDirectArguments arguments, bool useArgumentsAccountToUse = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<PostDirectTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task SendMentionWork(WorkRequest work, SendMentionArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<SendMentionTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse);
    }

    private async Task SendMessageWork(WorkRequest work, SendMessageArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<SendMessageTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }

    private async Task ViewBusinessPublicStoryWork(WorkRequest work, ViewBusinessPublicStoryArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ViewBusinessPublicStoryTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task ViewPublicStoryWork(WorkRequest work, ViewPublicStoryArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ViewPublicStoryTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task ReportUserStoryRandomWork(WorkRequest work, ReportUserStoryRandomArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ReportUserStoryRandomTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }

    private async Task AddFriendWork(WorkRequest work, AddFriendArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<AddFriendTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task AcceptFriendWork(WorkRequest work, AcceptFriendArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<AcceptFriendTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task ExportFriendWork(WorkRequest work, ExportFriendsArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ExportFriendTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task RelogAccountsWork(WorkRequest work, RelogAccountArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<RelogAccountTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task QuickAddWork(WorkRequest work, QuickAddArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<QuickAddTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task FriendCleanerWork(WorkRequest work, FriendCleanerArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<FriendCleanerTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task PostStoryWork(WorkRequest work, PostStoryArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<PostStoryTask>();
        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }
    
    private async Task ChangeUsernameWork(WorkRequest work, ChangeUsernameArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<ChangeUsernameTask>();

        work.AccountsById = true;
        if (arguments.AccID != null) work.AssignedAccounts = new List<string>(){arguments.AccID}.ToArray();

        await task.Start(work, arguments, this, useArgumentsAccountToUse); 
    }

    private async Task TestWork(WorkRequest work, TestArguments arguments, bool useArgumentsAccountToUse = false, bool isRerun = false)
    {
        using var scope = _serviceProvider.CreateScope();
        var task = scope.ServiceProvider.GetService<TestTask>();
        await task.Start(work, arguments, this); 
    }

    private async Task CreateAccountsWork(WorkRequest work, CreateAccountArguments arguments)
    {
        using var scope = _serviceProvider.CreateScope();
        var createAccountTask = scope.ServiceProvider.GetService<CreateAccountTask>();
        await createAccountTask.Start(work, arguments, this);
    }
}