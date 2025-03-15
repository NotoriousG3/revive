using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.Controllers;

public class NoAccessController: Controller
{
    public IActionResult Index()
    {
        return View();
    }
}