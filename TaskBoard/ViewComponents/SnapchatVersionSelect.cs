using Microsoft.AspNetCore.Mvc;
using SnapchatLib;

namespace TaskBoard.ViewComponents;

public enum SnapchatVersionSelectLayout
{
    Default,
    Horizontal
}

public class SnapchatVersionSelectArgs
{
    public OS OS { get; set; }
    public SnapchatVersionSelectLayout Layout { get; set; } = SnapchatVersionSelectLayout.Default;

    public SnapchatVersionSelectArgs(OS os, SnapchatVersionSelectLayout layout = SnapchatVersionSelectLayout.Default)
    {
        OS = os;
        Layout = layout;
    }
}

public class SnapchatVersionSelect : ViewComponent
{
    public IViewComponentResult Invoke(SnapchatVersionSelectArgs args)
    {
        var items = WebSnapchatVersion.VersionMap[args.OS];
        return args.Layout switch
        {
            SnapchatVersionSelectLayout.Horizontal => View("Horizontal", items),
            _ => View(items)
        };
    }
}