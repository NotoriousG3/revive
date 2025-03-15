using Microsoft.AspNetCore.Mvc;
using SnapchatLib;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

[Route("[controller]")]
public class GetMediaSelectController : Controller
{
    // GET
    public IActionResult Index(string controlId, bool showNoMediaLink = true, bool showSwipeUpUrl = false, int iteration = 0, bool showDelayField = false)
    {
        return ViewComponent(nameof(MediaSelect), new MediaSelectInvokeArguments() { ControlIdPrefix = controlId, ShowNoMediaLink = showNoMediaLink, ShowSwipeUpUrlField = showSwipeUpUrl, Iteration = iteration, ShowDelayField = showDelayField });
    }
}