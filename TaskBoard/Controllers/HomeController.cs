using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult NameManager()
    {
        return View();
    }
    
    public IActionResult BitmojiCreator()
    {
        return View();
    }
    
    public IActionResult UserNameManager()
    {
        return View();
    }
    
    public IActionResult AccountTools()
    {
        return View();
    }

    public IActionResult AccountImport()
    {
        return View();
    }

    public IActionResult EmailManager()
    {
        return View();
    }
    
    public IActionResult ProxyManager()
    {
        return View();
    }

    public IActionResult MessagePoster()
    {
        return View();
    }

    public IActionResult Relationships()
    {
        return View();
    }

    public IActionResult OtherActions()
    {
        return View();
    }

    public IActionResult Settings()
    {
        return View();
    }

    public IActionResult Purchase()
    {
        return View();
    }

    public IActionResult TargetManager()
    {
        return View();
    }
    
    public IActionResult KeywordScraper()
    {
        return View();
    }
    
    public IActionResult EmailScraper()
    {
        return View();
    }
    
    public IActionResult PhoneScraper()
    {
        return View();
    }
    
    public IActionResult MacroManager()
    {
        return View();
    }
    
    public IActionResult Analytics()
    {
        return View();
    }

    public IActionResult Overview()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}