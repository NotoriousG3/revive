using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.ViewComponents;

public class BitmojiSelectViewModel
{
    public string ControlId;
    public bool ShowLabel;
    public IEnumerable<BitmojiModel> Bitmojis;
}

public class BitmojiSelectViewArguments
{
    public string ControlId;
    public bool ShowLabel;
}

public class BitmojiSelect : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public BitmojiSelect(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(BitmojiSelectViewArguments args)
    {
        var items = await _context.Bitmojis.ToListAsync();
        return View(new BitmojiSelectViewModel() { ControlId = args.ControlId, Bitmojis = items, ShowLabel = args.ShowLabel});
    }
}