using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.ViewComponents;

public class FriendsGraphViewModel
{
    public int pendingFriends;
    public int mutualFriends;
}

public class FriendsGraph: ViewComponent
{
    private readonly ApplicationDbContext _context;

    public FriendsGraph(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var incFriends = 0;
        var outFriends = 0;
        var mutFriends = 0;

        var friends = _context.Accounts.FromSqlRaw("SELECT * FROM `Accounts`;");

        foreach (var Account in friends)
        {
            incFriends += Account.IncomingFriendCount;
            outFriends += Account.OutgoingFriendCount;
            mutFriends += Account.FriendCount;
        }
        
        return View(new FriendsGraphViewModel() { pendingFriends = (incFriends + outFriends), mutualFriends = mutFriends });
    }
}