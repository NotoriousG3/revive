using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.ViewComponents;

public struct SnapchatAccountDatatableArgs
{
    public bool ShowRelogButton;
    public bool ShowDeleteButton;
    public bool ShowAddToGroupButton;
    public bool ShowEditGroupButton;

    public SnapchatAccountDatatableArgs()
    {
        ShowRelogButton = true;
        ShowDeleteButton = true;
        ShowAddToGroupButton = false;
        ShowEditGroupButton = false;
    }
}

public class SnapchatAccountsDatatable: ViewComponent
{
    public IViewComponentResult Invoke(SnapchatAccountDatatableArgs args)
    {
        return View(args);
    }
}