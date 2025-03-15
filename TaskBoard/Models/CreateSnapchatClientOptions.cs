using SnapchatLib;

namespace TaskBoard.Models;

public class CreateSnapchatClientOptions
{
    public OS OS { get; set; }
    public SnapchatVersion SnapchatVersion { get; set; }
    public SnapchatAccountModel? Account { get; set; }
    public ProxyGroup? ProxyGroup { get; set; }
}