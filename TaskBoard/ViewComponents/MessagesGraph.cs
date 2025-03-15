using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class MessagesGraphViewModel
{
    public int TotalFriendsAdded;
    public int MessagesSent;
    public int PostsSent;
}

public class MessagesGraph: ViewComponent
{
    private readonly ApplicationDbContext _context;

    public MessagesGraph(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var friendsAdded = await _context.WorkRequests.Where(w => w.Action == WorkAction.AddFriend || w.Action == WorkAction.Subscribe)
            .SumAsync(w => (w.AccountsPass * w.ActionsPerAccount));
        var messagesSent = await _context.WorkRequests.Where(w => w.Action == WorkAction.SendMessage)
            .SumAsync(w => w.AccountsPass);
        var postsSent = await _context.WorkRequests.Where(w => w.Action == WorkAction.PostDirect)
            .SumAsync(w => w.AccountsPass);
        return View(new MessagesGraphViewModel() { TotalFriendsAdded = friendsAdded, MessagesSent = messagesSent, PostsSent = postsSent });
    }
}