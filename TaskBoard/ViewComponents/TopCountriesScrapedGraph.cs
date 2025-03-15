using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.ViewComponents;

public class TopCountriesScrapedGraphViewModel
{
    public Dictionary<string, int> Countries;
}

public class TopCountriesScrapedGraph: ViewComponent
{
    private readonly ApplicationDbContext _context;

    public TopCountriesScrapedGraph(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var countries = await _context.TargetUsers.Where(u => u.CountryCode != null).GroupBy(u => u.CountryCode)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count())).ToDictionaryAsync(g => g.Key);

        var result = new Dictionary<string, int>();
        foreach (var kvp in countries.Values)
        {
            result.Add(kvp.Key, kvp.Value);
        }
        
        return View(new TopCountriesScrapedGraphViewModel() { Countries = result });
    }
}