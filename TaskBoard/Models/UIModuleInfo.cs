using SnapWebModels;

namespace TaskBoard.Models;

public enum UIModuleGroup
{
    Functionality,
    Actions,
    Access
}

public struct UIModuleInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconClass { get; set; }
    public SnapWebModule Module { get; set; }

    public UIModuleInfo(string name, string description, string icon, SnapWebModule module)
    {
        Name = name;
        IconClass = icon;
        Module = module;
        Description = description;
    }
}
