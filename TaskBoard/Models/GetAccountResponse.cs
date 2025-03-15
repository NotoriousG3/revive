namespace TaskBoard.Models;

public class GetAccountResponse : PaginationResponse<UIAccountModel>
{
    public GetAccountResponse(IEnumerable<UIAccountModel> accounts, int pageNumber, int resultsPerPage) : base(accounts, pageNumber, resultsPerPage)
    {
    }
}