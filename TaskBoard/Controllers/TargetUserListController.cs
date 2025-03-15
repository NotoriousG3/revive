using Microsoft.AspNetCore.Mvc;
using TaskBoard.ViewComponents;

namespace TaskBoard.Controllers;

public class TargetUserListController: Controller
{
    // GET
    public IActionResult Index(string[] args)
    {
        return ViewComponent(nameof(TargetUserList), args);
    }
}