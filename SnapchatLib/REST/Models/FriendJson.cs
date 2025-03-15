using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class Device
{
    public string out_beta { get; set; }
    public int version { get; set; }
}

public class FideliusInfo
{
    public List<Device> devices { get; set; }
}
