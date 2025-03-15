using Newtonsoft.Json;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;
using TaskBoard.WorkTask;

namespace TaskBoard.Tests;

public class UnitTestMessagingTaskArguments: MessagingArguments {

}

public class UnitTestMessagingTask: ActionWorkTask
{
    public UnitTestMessagingTask(WorkScheduler scheduler, WorkLogger logger, SnapchatAccountManager accountManager, SnapchatActionRunner runner, AccountTracker accountTracker, TargetManager targetManager, IServiceProvider serviceProvider) : base(scheduler, logger, accountManager, runner, accountTracker, targetManager, serviceProvider)
    {
    }

    public async Task SetTaskTargets(WorkRequest work, UnitTestMessagingTaskArguments arguments)
    {
        await base.SetTaskTargets(work, arguments);
    }

    public static string ToString(UnitTestMessagingTaskArguments arguments)
    {
        return arguments.ToString();
    }

    public static UnitTestMessagingTaskArguments ToUnitTestMessagingTaskArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<UnitTestMessagingTaskArguments>(arguments)!;
    }
}