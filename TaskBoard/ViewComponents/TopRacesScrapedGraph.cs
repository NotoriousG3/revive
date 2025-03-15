using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.ViewComponents;

public class TopRacesScrapedGraphViewModel
{
    public Dictionary<string, int> Races;
}

public class TopRacesScrapedGraph: ViewComponent
{
    private readonly ApplicationDbContext _context;

    public TopRacesScrapedGraph(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var races = await _context.TargetUsers.Where(u => u.Race != null).GroupBy(u => u.Race)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count())).ToDictionaryAsync(g => g.Key);

        var result = new Dictionary<string, int>();
        foreach (var kvp in races.Values)
        {
            result.Add(kvp.Key, kvp.Value);
        }
        
        return View(new TopRacesScrapedGraphViewModel() { Races = result });
    }
}