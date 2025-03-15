using System.Collections.Concurrent;
using TaskBoard.Models;

namespace TaskBoard;

public class SnapchatTask: IDisposable
{
    // Task used to run InnerTask and wait for it to finish
    public Task? ExecutionTask;

    // Actual task performing some work
    public Task? InnerTask;
    public bool SkipQueue;
    public WorkRequest WorkRequest;

    public void Dispose()
    {
        ExecutionTask?.Dispose();
        InnerTask?.Dispose();
    }
}

public abstract class Worker : IHostedService, IDisposable
{
    private readonly ILogger<Worker> _logger;
    protected readonly IServiceProvider _serviceProvider;

    // Queues storing tasks that need to be performed
    private readonly ConcurrentDictionary<WorkRequest, ConcurrentQueue<SnapchatTask>> _jobsQueue = new();
    private readonly ConcurrentDictionary<WorkRequest, ConcurrentQueue<SnapchatTask>> _priorityJobQueue = new();

    // Currently running tasks, this will use the settings limits to have max number of tasks running per workrequest
    private readonly ConcurrentDictionary<long, List<SnapchatTask>> _runningTasks = new();
    protected int MaxThreads = 1;
    protected int MaxTasks = 1;

    protected Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public abstract void Dispose();

    public abstract Task StartAsync(CancellationToken cancellationToken);

    public abstract Task StopAsync(CancellationToken cancellationToken);

    public bool IsWorkRequestAtMaxTasks(WorkRequest workRequest)
    {
        // False if work request doesn't exist
        if (!_runningTasks.TryGetValue(workRequest.Id, out var tasks)) return false;

        if (tasks == null) return false;

        return tasks.Count > MaxThreads;
    }

    public void CleanWork(WorkRequest work)
    {
        _runningTasks.TryRemove(work.Id, out _);
        _jobsQueue.TryRemove(work, out _);
        _priorityJobQueue.TryRemove(work, out _);
    }

    private void ProcessQueue(ConcurrentDictionary<WorkRequest, ConcurrentQueue<SnapchatTask>> workQueues)
    {
        foreach (var (work, queue) in workQueues)
        {
            // See if we are at our limit of tasks per work request
            if (IsWorkRequestAtMaxTasks(work)) continue;

            while (!queue.IsEmpty)
            {
                // See if we are at our limit of tasks per work request
                if (IsWorkRequestAtMaxTasks(work)) continue;

                if (!queue.TryDequeue(out var task)) break;

                if (task.WorkRequest.CancellationTokenSource != null && (task.WorkRequest.Status == WorkStatus.Cancelled || task.WorkRequest.CancellationTokenSource.IsCancellationRequested)) { continue; }

                task.InnerTask?.Start();

                if (!_runningTasks.TryGetValue(task.WorkRequest.Id, out var tasks))
                {
                    tasks = new List<SnapchatTask>();
                }

                tasks.Add(task);

                // We need to check this here because otherwise we end up having a "running job" with no tasks
                if (task.WorkRequest.Status == WorkStatus.Cancelled || task.WorkRequest.CancellationTokenSource.IsCancellationRequested) break;
                
                _runningTasks[task.WorkRequest.Id] = tasks;
            }
        }
    }

    protected void ProcessJobQueues()
    {
        // Remove those tasks that are already complete
        foreach (var runningTasksValue in _runningTasks.Values)
        {
            runningTasksValue.RemoveAll(t => t.InnerTask == null || t.InnerTask.IsCompleted);
        }

        var totalTasks = _runningTasks.Values.Sum(t => t.Count);
        
        _logger.LogDebug($"Currently running {_runningTasks.Count} jobs. Total tasks: {totalTasks}");
        
        if (_jobsQueue.IsEmpty && _priorityJobQueue.IsEmpty) return;

        _logger.LogDebug($"Priority Job Queue Length: {_priorityJobQueue.Count}");
        ProcessQueue(_priorityJobQueue);
        _logger.LogDebug($"Job Queue Length: {_jobsQueue.Count}");
        ProcessQueue(_jobsQueue);
    }

    public async Task WaitTasksCompletion<T>(WorkRequest work, List<Task<T>> tasks, CancellationToken token)
    {
        try
        {
            while (!tasks.All(t => t.IsCompleted))
            {
                if (token.IsCancellationRequested)
                    return;

                await Task.Delay(1000, token);
            }
        }
        finally
        {
            _runningTasks.TryRemove(work.Id, out _);
        }
    }
    
    public async Task WaitTasksCompletion(List<Task> tasks, CancellationToken token)
    {
        while (!tasks.All(t => t.IsCompleted))
        {
            if (token.IsCancellationRequested)
                return;

            await Task.Delay(1000, token);
        }
    }

    private void AddToQueue(SnapchatTask task, ConcurrentDictionary<WorkRequest, ConcurrentQueue<SnapchatTask>> targetQueue)
    {
        if (!targetQueue.TryGetValue(task.WorkRequest, out var taskQueue))
        {
            taskQueue = new ConcurrentQueue<SnapchatTask>();
        }
        
        taskQueue.Enqueue(task);

        targetQueue[task.WorkRequest] = taskQueue;
    }

    public void QueueTask(SnapchatTask task)
    {
        while (_jobsQueue.Count >= MaxThreads)
        {
            Task.Delay(2000).Wait(task.WorkRequest.CancellationTokenSource.Token);
        }

        if (task.SkipQueue) AddToQueue(task, _priorityJobQueue);
        else AddToQueue(task, _jobsQueue);
    }

    protected void CancelTasks(WorkRequest work)
    {
        if (!_runningTasks.TryGetValue(work.Id, out var tasks)) return;

        foreach (var task in tasks)
        {
            task.Dispose();
        }
    }
}