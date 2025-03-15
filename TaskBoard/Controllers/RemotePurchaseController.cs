using Microsoft.AspNetCore.Mvc;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckLateAccessDeadlineAttribute))]
[Route("api/purchase")]
public class RemotePurchaseController : ApiController
{
    private readonly SnapWebManagerClient _managerClient;

    public RemotePurchaseController(SnapWebManagerClient managerClient)
    {
        _managerClient = managerClient;
    }

    [HttpPost]
    public async Task<IActionResult> Index(AddonsCart cart)
    {
        var invoice = await _managerClient.Purchase(cart, Url.Action("Purchase", "Home"));
        return OkApi("", invoice);
    }
}