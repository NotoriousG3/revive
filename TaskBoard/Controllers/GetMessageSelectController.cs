using Microsoft.AspNetCore.Mvc;
using SnapchatLib;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

[Route("[controller]")]
public class GetMessageSelectController : Controller
{
    // GET
    public IActionResult Index(string controlId, bool showNoMediaLink = true, bool showSwipeUpUrl = false, int iteration = 0, bool showDelayField = false)
    {
        return ViewComponent(nameof(MessageSelect), new MessageSelectInvokeArguments() { ControlIdPrefix = controlId, Iteration = iteration, ShowDelayField = showDelayField });
    }
}