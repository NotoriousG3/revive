namespace TaskBoard.Models;

public abstract class PaginationResponse<T>
{
    public PaginationResponse(IEnumerable<T> entries, int pageNumber, int resultsPerPage)
    {
        // Force some defaults for bad values
        if (pageNumber < 0) pageNumber = 0;
        if (resultsPerPage == 0) resultsPerPage = 100;
        var position = pageNumber * resultsPerPage;

        var entriesList = entries.ToList();
        Total = entriesList.Count;

        // if we are requesting more pages than we have, just return the final page
        if (position > Total)
        {
            position = Total - resultsPerPage;

            // correct the page number
            pageNumber = position / resultsPerPage;
        }

        PageNumber = pageNumber;
        Entries = entriesList.Skip(position).Take(resultsPerPage);
        PreviousPageNumber = pageNumber <= 0 ? null : pageNumber - 1;
        NextPageNumber = position + resultsPerPage > Total ? null : pageNumber + 1;
    }

    public int? NextPageNumber { get; set; }
    public int? PreviousPageNumber { get; set; }
    public int PageNumber { get; set; }
    public int Total { get; set; }
    public IEnumerable<T> Entries { get; set; }
}