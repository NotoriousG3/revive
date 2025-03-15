using Microsoft.AspNetCore.Mvc;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

[Route("[controller]")]
public class WorkSelectController: Controller
{
    // GET
    public IActionResult Index(string[] args)
    {
        return ViewComponent(nameof(WorkSelect), args);
    }
}