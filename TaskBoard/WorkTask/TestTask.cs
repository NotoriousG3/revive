using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class TestTask: ActionWorkTask
{
    private readonly FakePersonGenerator _fakePersonGenerator;

    public TestTask(FakePersonGenerator fakePersonGenerator, WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider): base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
        _fakePersonGenerator = fakePersonGenerator;
        CheckForMedia = false;
    }
    
    private async Task<WorkStatus> DoWork(WorkRequest work, TestArguments arguments, SnapchatAccountModel account)
    {
        using var scope = ServiceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proxyManager = scope.ServiceProvider.GetRequiredService<IProxyManager>();
        var logger = scope.ServiceProvider.GetRequiredService<WorkLogger>();

        try
        {
            if (arguments.FriendsOnly)
            {
                await logger.LogInformation(work, "Using 1000 imaginary targets", account);
                var fakes = new List<string>();
                for (var i = 0; i < 1000; i++)
                {
                    fakes.Add((_fakePersonGenerator.Generate(null, Gender.Random)).Name);
                }
                
                var friends = fakes.Chunk(200).ToList();

                if (!friends.Any() || !arguments.Pass)
                {
                    await Scheduler.FailWorkAccount(work, account);
                    return WorkStatus.Error;
                }

                // If this is sending repeated messages, we need to change arguments.User for new List<string>() { user }
                foreach (var chunk in friends)
                {
                    await Task.Delay(arguments.DelayMs, work.CancellationTokenSource.Token);
                    await logger.LogInformation(work, $"Finished waiting for chunk of targets: {string.Join(", ", chunk)}", account);
                }
                
                account.SetStatus(AccountManager, AccountStatus.BAD_PROXY);
                account.Proxy = await proxyManager.Take();
                context.Update(account);
                await context.SaveChangesAsync();

                await AssignAccount(work, account);
                await Scheduler.UpdateWorkAddPass(work);
                return WorkStatus.Ok;
            }

            // For testing target assignment and chaining
            await TargetManager.FlagTargetAsAdded(TargetUsers);

            await Task.Delay(arguments.DelayMs, work.CancellationTokenSource.Token);

            if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

            account.SetStatus(AccountManager, AccountStatus.NEEDS_RELOG);
            var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);
            var proxy = await proxyManager.Take(proxyGroup);
            account.ProxyId = proxy.Id;
            context.Update(account);
            
            await logger.LogInformation(work, $"Finished waiting for chunk of targets: {string.Join(", ", TargetUsers.Select(s => s.Username))}", account);
            await Scheduler.UpdateWorkAddPass(work);
            await context.SaveChangesAsync();
                
            return WorkStatus.Ok;
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                if (e is not NoAvailableProxyException)
                {
                    // If we don't handle this, then the exception is not caught :(
                    logger.LogError(work, e, account)
                        .Wait(work.CancellationTokenSource.Token);
                    Scheduler.FailWorkAccount(work, account).Wait(work.CancellationTokenSource.Token);
                    return true;
                }

                logger.LogError(work, "No proxies found").Wait(work.CancellationTokenSource.Token);
                Scheduler.EndWork(work, WorkStatus.Error).Wait();
                work.CancellationTokenSource.Cancel();
                return true;
            });

            return WorkStatus.Error;
        }
        catch (AccountBannedException)
        {
            // When an account is banned, we want to try again with a different account
            await logger.LogInformation(work, "Selected account is banned. Restarting process for 1 account", account);
            return WorkStatus.Retry;
        }
        catch (Exception ex)
        {
            await logger.LogError(work, ex, account);
            return WorkStatus.Error;
        }
        finally
        {
            await AssignAccount(work, account);
        }
    }
    
    public async Task Start(WorkRequest work, TestArguments arguments, SnapchatActionsWorker worker, bool useArgumentsAccountToUse = false)
    {
        await SetTaskTargets(work, arguments);
        await base.Start(work, arguments, worker, DoWork, TaskCleanup, useArgumentsAccountToUse);
    }
}