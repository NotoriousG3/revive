using TaskBoard.Models;

namespace TaskBoard;

public class WorkRequestTracker
{
    // Handle list of currently running Jobs (WorkRequest). This is to avoid starting the same job twice if there's some delay with the DB.
    // WHY keep a ref to WorkRequest when we already have it from outside...
    private readonly Dictionary<long, WorkRequest> _runningWorkRequests = new();

    public delegate void FinishJobEvent(WorkRequest work);

    public FinishJobEvent OnJobFinish;
    
    public WorkRequestTracker() {}

    public bool GetTrackedWork(WorkRequest work, out WorkRequest? trackedWork)
    {
        return _runningWorkRequests.TryGetValue(work.Id, out trackedWork);
    }

    public void Track(WorkRequest work)
    {
        _runningWorkRequests.Add(work.Id, work);
    }

    public void Untrack(WorkRequest work)
    {
        _runningWorkRequests.Remove(work.Id);
        OnJobFinish?.Invoke(work);
    }

    public bool HasRunningWork()
    {
        return _runningWorkRequests.Count > 0;
    }

    public int RunningWorks()
    {
        return _runningWorkRequests.Count;
    }
}