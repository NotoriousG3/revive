namespace TaskBoard.Models;

public class GetWorkLogsResponse : PaginationResponse<LogEntry>
{
    public GetWorkLogsResponse(List<LogEntry> accounts, int pageNumber, int resultsPerPage) : base(accounts, pageNumber, resultsPerPage)
    {
    }
}