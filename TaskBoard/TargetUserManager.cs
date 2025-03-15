using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class TargetManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Utilities _utilities;
    
    public TargetManager() {}

    public TargetManager(IServiceScopeFactory scopeFactory, Utilities utilities)
    {
        _scopeFactory = scopeFactory;
        _utilities = utilities;
    }

    public async Task Delete(TargetUser target)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Remove(target);
        await context.SaveChangesAsync();
    }

    public async Task SaveTargetUpdates(TargetUser target)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Update(target);
        await context.SaveChangesAsync();
    }
    
    public async Task FlagTargetAsAdded(IEnumerable<TargetUser> targets)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var target in targets)
        {
            target.Added = true;
            context.Update(target);    
        }
        
        await context.SaveChangesAsync();
    }

    public async Task Add(TargetUser target)
    {
        if (await CheckIfExists(target)) throw new Exception($"Target user \"{target.Username}\" already exists.");
     
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Add(target);
        await context.SaveChangesAsync();
    }

    private async Task<bool> CheckIfExists(TargetUser user)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.TargetUsers.FirstOrDefault(t => t.Username.Equals(user.Username)) != null;
    }
    
    public TargetUser? FindTargetUserObject(string name)
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.TargetUsers.FirstOrDefault(t => t.Username.Equals(name));
    }

    public virtual async Task<List<TargetUser>> GetWorkTargetUsers(WorkRequest work)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.ChosenTargets.Where(r => r.WorkId == work.Id).Include(e => e.TargetUser).Select(t => t.TargetUser).ToList();
    }

    public async Task<List<TargetUser>> ProcessTargetList(WorkRequest work, SnapchatAccountModel account, IEnumerable<string> users)
    {
        var tempUsers = new List<TargetUser>();

        foreach (var target in users)
        {
            var targetObject = FindTargetUserObject(target);

            if (targetObject != null)
            {
                tempUsers.Add(targetObject);
            }
            else
            {
                tempUsers.Add(new TargetUser(){Username = target});
            }
        }

        return tempUsers;
    }

    /*public virtual string CreateSqlQuery(int amount, bool usedOnly, bool addedOnly, string? country = "ANY", string? race = "ANY", string? gender = "ANY")
    {
        List<string> ArabicCountrys = new List<string>() { "MY","TR","BH","IQ","JO","KW","LB","OM","QA","SA","SY","AE","YE","AF","EG","MA","SD" };
        var _addedOnly = addedOnly ? "`Added` = 1" : "`Added`='0'";
        var _usedOnly = usedOnly ? "`Used` = 1" : "`Used`='0'";
        var _countryCode = string.IsNullOrWhiteSpace(country) || country == "ANY" ? "" : $"`CountryCode` LIKE '{country}'";
        var _genderEntry = string.IsNullOrWhiteSpace(gender) || gender == "ANY" ? "" : $"`Gender` LIKE '{gender}'";
        var _raceEntry = string.IsNullOrWhiteSpace(race) || race == "ANY" ? "" : $"`Race` LIKE '{race}'";

        if (country.Equals("ARABIC COUNTRIES"))
        {
            for (int i = 0; i < ArabicCountrys.Count; i++)
            {
                _countryCode += $" OR `CountryCode`='{ArabicCountrys[i]}'";
            }
        }
        
        var filters = new[] { _usedOnly, _addedOnly, _countryCode, _genderEntry, _raceEntry }.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
        
        var whereString = "";
        if (filters.Any())
        {
            var filterString = string.Join(" AND ", filters);
            whereString = $" WHERE {filterString}";
        }
        
        //Console.WriteLine(whereString);

        var query = $"SELECT * FROM `TargetUsers`{whereString} ORDER BY RAND() LIMIT {amount};";

        return query;
    }
    
    public virtual async Task<IEnumerable<TargetUser>> GetRandomTargetNames(int amount, bool usedOnly, bool addedOnly = false, string? country = "ANY", string? race = "ANY", string? gender = "ANY")
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var query = CreateSqlQuery(amount, usedOnly, addedOnly, country, race, gender);
        return context.TargetUsers.FromSqlRaw(query).ToList();
    }*/
    
    public virtual async Task<IEnumerable<TargetUser>> GetRandomTargetNames(int amount, bool usedOnly, bool addedOnly = false, string? country = "ANY", string? race = "ANY", string? gender = "ANY")
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var filteredResult = context.TargetUsers.Where(e =>
            (!addedOnly || e.Added) &&
            (!usedOnly || e.Used) &&
            (string.IsNullOrWhiteSpace(country) || country == "ANY" || (country == "ARABIC COUNTRIES" ? Utilities.ArabicCountries.Contains(e.CountryCode) : e.CountryCode == country)) &&
            (string.IsNullOrWhiteSpace(gender) || gender == "ANY" || e.Gender == gender) &&
            (string.IsNullOrWhiteSpace(race) || race == "ANY" || e.Race == race)
        );

        var result = await filteredResult.Take(amount).ToListAsync();

        return result.OrderBy(x => Guid.NewGuid());
    }
    
    public virtual async Task<IEnumerable<TargetUser>> GetRandomTargetNames(bool usedOnly, bool addedOnly = false, string? country = "ANY", string? race = "ANY", string? gender = "ANY")
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var filteredResult = context.TargetUsers.Where(e =>
            (!addedOnly || e.Added) &&
            (!usedOnly || e.Used) &&
            (string.IsNullOrWhiteSpace(country) || country == "ANY" || (country == "ARABIC COUNTRIES" ? Utilities.ArabicCountries.Contains(e.CountryCode) : e.CountryCode == country)) &&
            (string.IsNullOrWhiteSpace(gender) || gender == "ANY" || e.Gender == gender) &&
            (string.IsNullOrWhiteSpace(race) || race == "ANY" || e.Race == race)
        );

        var result = await filteredResult.ToListAsync();

        return result.OrderBy(x => Guid.NewGuid());
    }
    
    public virtual Task<IEnumerable<TargetUser>> FromStrings(IEnumerable<string> users)
    {
        // Potentially expensive... multiple query to fetch all targets, then multiple queries to update them as they are added
        var targetUsers = users.Select(FindTargetUserObject).Where(targetObject => targetObject != null).ToList()!;
        return Task.FromResult(!targetUsers.Any() ? Enumerable.Empty<TargetUser>() : targetUsers!);
    }
}