using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class KeywordManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Utilities _utilities;

    public KeywordManager(IServiceScopeFactory scopeFactory, Utilities utilities)
    {
        _scopeFactory = scopeFactory;
        _utilities = utilities;
    }

    public async Task Delete(Keyword keyword)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Remove(keyword);
        await context.SaveChangesAsync();
    }

    public async Task<int> Count()
    {
        return (await GetKeywords()).Count;
    }

    public Keyword PickRandom()
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Keywords == null)
            throw new Exception("KeywordManager.PickRandom is null parse it properly");

        return context.Keywords.AsParallel().PickRandom();
    }

    public async Task<List<Keyword>> GetKeywords()
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Keywords == null)
            throw new Exception("KeywordManager.GetKeywords is null parse it properly");

        return await context.Keywords.ToListAsync();
    }
}