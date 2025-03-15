using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public abstract class BaseWorkTask
{
    protected readonly WorkLogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseWorkTask(WorkLogger logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public async Task SetBitmoji(SnapchatActionRunner _runner, SnapchatAccountModel account, WorkRequest work, CreateAccountArguments arguments, ProxyGroup? proxyGroup)
    {
        try
        {
            // Call bitmoji function if selected
            if (arguments.BitmojiSelection == BitmojiSelection.Random)
            {
                Random r = new();

                if (arguments.Gender == Gender.Female)
                {
                    arguments.BitmojiSelection = (BitmojiSelection)r.Next(3, 14);
                }
                else
                {
                    //arguments.BitmojiSelection = (BitmojiSelection.Black_BlackHaired_MaleMoji); throws a null reference object when we try to use this bitmoji.
                }
            }

            await _runner.CallBitmoji(account, arguments.BitmojiSelection, proxyGroup, work.CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            await Logger.LogError(work, $"SetBitmoji: {ex.Message}{ex.StackTrace}");
        }
    }
    
    //TODO: context might become an issue here. It could be better to create a new one on each call of this method
    public async Task BoostScoreAction(AppSettingsLoader settingsLoader, SnapchatAccountManager _accountManager, WorkScheduler _scheduler, SnapchatActionRunner _runner,
        CreateAccountArguments arguments, SnapchatAccountModel account, ApplicationDbContext context, WorkRequest work, ProxyGroup proxyGroup)
    {
        try
        {
            account.SetStatus(_accountManager, AccountStatus.BUSY);

            var currentScore = 0;
            List<string> Messagers = new List<string>() { "m3ntion" };
            HashSet<string> MSG = new HashSet<string>();
            var mediaFile = context.MediaFiles.FirstOrDefault();

            foreach (var f in Messagers)
            {
                try
                {
                    await _runner.AddFriend(account, f, proxyGroup, work.CancellationTokenSource.Token);
                    MSG.Add(f);
                }
                catch (Exception)
                {
                    await Logger.LogError(work, $"{account.Username} failed to add friend {f}.");
                }
            }

            if (mediaFile != null && mediaFile.ServerPath != null)
            {
                while (currentScore < arguments.BoostScore)
                {
                    await _runner.PostDirect(account, mediaFile.ServerPath, "",
                        MSG, proxyGroup,
                        work.CancellationTokenSource.Token);

                    currentScore += MSG.Count;
                }

                await _scheduler.UpdateWorkAddPass(work);
            }
            else
            {
                await Logger.LogError(work, $"No media file found.");
            }
        }
        catch (Exception ex)
        {
            await Logger.LogError(work, $"BoostScore: {ex.Message}{ex.StackTrace}");
        }
        finally
        {
            account.SetStatus(_accountManager, AccountStatus.OKAY);
        }
    }
}