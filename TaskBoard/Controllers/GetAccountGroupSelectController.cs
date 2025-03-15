using Microsoft.AspNetCore.Mvc;
using SnapchatLib;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

[Route("[controller]")]
public class GetAccountGroupSelectController : Controller
{
    // GET
    public IActionResult Index(string controlId, bool showLabel)
    {
        return ViewComponent(nameof(AccountGroupSelect), new AccountGroupSelectViewArguments() { ControlId = controlId, ShowLabel = showLabel});
    }
}