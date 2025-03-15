using Microsoft.AspNetCore.Mvc;
using SnapchatLib;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

[Route("[controller]")]
public class GetSnapchatVersionSelectController : Controller
{
    // GET
    public IActionResult Index(OS os)
    {
        return ViewComponent(nameof(SnapchatVersionSelect), new SnapchatVersionSelectArgs(os));
    }

    [HttpGet("horizontal")]
    public IActionResult Horizontal(OS os)
    {
        return ViewComponent(nameof(SnapchatVersionSelect), new SnapchatVersionSelectArgs(os, SnapchatVersionSelectLayout.Horizontal));
    }
}