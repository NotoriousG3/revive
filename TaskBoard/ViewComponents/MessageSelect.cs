using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapchatLib;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class MessageSelectViewModel
{
    public string ControlIdPrefix { get; set; } // The id of the media select control
    public int Iteration { get; set; } // Used for generating increasing ids in html elements
    public bool ShowDelayField { get; set; }
}

public class MessageSelectInvokeArguments
{
    public string ControlIdPrefix { get; set; }
    public int Iteration { get; set; }
    public bool ShowDelayField { get; set; }
}

public class MessageSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public MessageSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(MessageSelectInvokeArguments arguments)
    {
        return View(new MessageSelectViewModel() { ControlIdPrefix = arguments.ControlIdPrefix, Iteration = arguments.Iteration, ShowDelayField = arguments.ShowDelayField });
    }
}