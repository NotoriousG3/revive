using Microsoft.AspNetCore.Mvc;

namespace SnapWebManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SendGridInfoController : Controller
{
    // GET
    public IActionResult Index()
    {
        return Json(new SendGridInfo()
        {
            ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY"),
            Email = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL"),
            Name = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME"),
        });
    }
}