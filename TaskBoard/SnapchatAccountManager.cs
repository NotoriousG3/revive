using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard;

public enum RelogResult
{
    AccountNotFound,
    IncorrectAccountStatus,
    Ok
}

public class SnapchatAccountManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AccountTracker _accountTracker;
    public SnapchatAccountManager() {}
    
    public SnapchatAccountManager(IServiceScopeFactory scopeFactory, AccountTracker accountTracker)
    {
        _scopeFactory = scopeFactory;
        _accountTracker = accountTracker;
    }

    public async Task<bool> UpdateAccount(SnapchatAccountModel entity)
    {
        try
        {   
            await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entry = context.Accounts.First(e=>e.Id == entity.Id);
            context.Entry(entry).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            // handle correct exception
            // log error
            return false;
        }
    }

    public async Task<bool> Exists(string userId)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.Accounts.Where(u => u.UserId == userId).Any();
    }

    public async Task<bool> Delete(long id, bool saveBannedLog = false)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        if (account == null) return false;

        var email = context.Emails.Where(e => e.AccountId == account.Id);
        
        if (await email.AnyAsync())
            context.RemoveRange(email);
        context.Remove(account);

        if (saveBannedLog)
        {
            var log = new BannedAccountDeletionLog { Username = account.Username, DeletionTime = DateTime.UtcNow };
            context.BannedAccountDeletionLog!.Add(log);
        }

        await context.SaveChangesAsync();
        
        _accountTracker.UnTrack(account);
        
        return true;
    }
    
    public async Task<bool> RelogAll() // todo: remake into a task
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var account in context.Accounts)
        {
            if (account.AccountStatus != AccountStatus.BANNED && account.AccountStatus != AccountStatus.LOCKED &&
                account.AccountStatus != AccountStatus.RATE_LIMITED)
            {
                account.SetStatus(this, AccountStatus.NEEDS_RELOG);
            }
        }
        
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<RelogResult> ChangeUsername(ChangeUsernameArguments args)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(args.AccID);

        if (account == null) return RelogResult.AccountNotFound;

        await context.SaveChangesAsync();
        
        return RelogResult.Ok;
    }
    
    public async Task<RelogResult> Relog(long id)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        if (account == null) return RelogResult.AccountNotFound;
        
        if (account.AccountStatus != AccountStatus.OKAY && account.AccountStatus != AccountStatus.NEEDS_CHECKED)
        {
            return RelogResult.IncorrectAccountStatus;
        }
        
        account.SetStatus(this, AccountStatus.NEEDS_RELOG);
        
        await context.SaveChangesAsync();
        
        return RelogResult.Ok;
    }
    
    public async Task<bool> ReloadFriends(long id)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        if (account == null) return false;

        if (account.AccountStatus != AccountStatus.OKAY)
        {
            throw new Exception("Account must be status `Okay` to proceed."); 
        }

        account.SetStatus(this, AccountStatus.NEEDS_FRIEND_REFRESH);
        
        await context.SaveChangesAsync();
        
        return true;
    }

    public int Count()
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.Accounts?.Count() ?? 0;
    }

    private IEnumerable<SnapchatAccountModel> FilterAccounts(List<SnapchatAccountModel> accounts, int number, IEnumerable<string>? excludeAccounts)
    {
        var filtered = excludeAccounts != null ? accounts.Where(a => !excludeAccounts.Contains(a.Username)).ToList() : accounts;
        var result = filtered.Count <= number ? filtered : filtered.OrderBy(x => Guid.NewGuid()).Take(number);
        return result;
    }

    public virtual async Task<IEnumerable<SnapchatAccountModel>> PickMultipleRandom(int number, IEnumerable<string>? excludeAccounts, long accountGroupId = 0)
    {
        using var scope = _scopeFactory.CreateScope();
        var settingsLoader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
        var settings = await settingsLoader.Load();
        var list = await GetAllowedAccounts(settings, accountGroupId);
        // If our group id is being given, disregard number so that we pass all the accounts in the group
        return FilterAccounts(list, accountGroupId == 0 ? number : list.Count, excludeAccounts);
    }

    public virtual async Task<IEnumerable<SnapchatAccountModel>> PickWorkAccounts(WorkRequest work, int number, IEnumerable<string>? excludeAccounts)
    {
        using var scope = _scopeFactory.CreateScope();
        var settingsLoader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
        var settings = await settingsLoader.Load();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var list = await context.ChosenAccounts
            .Where(r => r.WorkId == work.Id)
            .Include(r => r.Account)
            .Select(r => r.Account).Take(settings.MaxManagedAccounts)
            .ToListAsync();
        return FilterAccounts(list, number, excludeAccounts);
    }

    public async Task<IEnumerable<SnapchatAccountModel>> PickWithIds(int number, IEnumerable<string> ids, IEnumerable<string>? excludeAccounts)
    {
        using var scope = _scopeFactory.CreateScope();
        var settingsLoader = scope.ServiceProvider.GetRequiredService<AppSettingsLoader>();
        var settings = await settingsLoader.Load();
        var list = (await GetAllowedAccounts(settings)).Where(a => ids.Any(i => i == a.Id.ToString())).ToList();
        return FilterAccounts(list, number, excludeAccounts);
    }

    public async Task<List<SnapchatAccountModel>> GetAllowedAccounts(AppSettings settings, long accountGroupId = 0)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Filter by group if required
        if (accountGroupId > 0)
        {
            var group = await context.AccountGroups.FindAsync(accountGroupId);

            if (group == null) return new List<SnapchatAccountModel>();
            await context.Entry(group).Collection(g => g.Accounts).LoadAsync();
            return group.Accounts.Take(settings.MaxManagedAccounts).ToList();
        }
        
        // No group filtering
        return await context.Accounts.Take(settings.MaxManagedAccounts).Include(e => e.Proxy).Include(e => e.Groups).ToListAsync();
    }
}