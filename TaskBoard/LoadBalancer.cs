using System.Collections.Concurrent;

namespace TaskBoard;

class LoadBalancer
{
    private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly int _maxThreads;
    private int _activeThreads = 0;
    public LoadBalancer(int maxThreads)
    {
        _maxThreads = maxThreads;
    }

    public void AddTask(Task task)
    {
        _tasks.Enqueue(task);
        
        if (_activeThreads < _maxThreads)
        {
            _activeThreads++;
            ThreadPool.QueueUserWorkItem(Worker, _cancellationTokenSource.Token);
        }
    }

    private void Worker(object state)
    {
        try
        {
            var cancellationToken = (CancellationToken)state;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_tasks.TryDequeue(out var task))
                {
                    task.Wait();
                }

                Thread.Sleep(1000);
            }

            _activeThreads--;
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                return true;
            });
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
}