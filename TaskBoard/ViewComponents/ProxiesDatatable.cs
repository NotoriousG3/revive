using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.ViewComponents;

public struct ProxiesDatatableArgs
{
    public bool ShowDeleteButton;
    public bool ShowAddToGroupButton;
    public bool ShowEditGroupButton;

    public ProxiesDatatableArgs()
    {
        ShowDeleteButton = true;
        ShowAddToGroupButton = false;
        ShowEditGroupButton = false;
    }
}

public class ProxiesDatatable: ViewComponent
{
    public IViewComponentResult Invoke(ProxiesDatatableArgs args)
    {
        return View(args);
    }
}