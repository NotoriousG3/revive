using TaskBoard.Models.Datatables;

namespace TaskBoard;

public class SearchUtilities
{
    public static IList<T> SearchDataTablesEntities<T>(DataTableAjaxModel model, IQueryable<T> set, Func<T, bool> wherePredicate, out int filteredResultsCount, out int totalResultsCount) where T : class
    {
        var searchBy = model.search.value;
        var take = model.length;
        var skip = model.start;

        totalResultsCount = set.Count();

        if (string.IsNullOrWhiteSpace(searchBy))
        {
            filteredResultsCount = totalResultsCount;
            return set.Skip(skip).Take(take).ToList();
        }
        
        var filtered = set.Where(wherePredicate).ToList();
        if (!filtered.Any())
        {
            filteredResultsCount = 0;
            // empty collection...
            return new List<T>();
        }
        
        var result = filtered.Skip(skip).Take(take).ToList();
        
        // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
        filteredResultsCount = filtered.Count;

        return result;
    }
}