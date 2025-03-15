using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

internal class SignJson
{
    public Dictionary<string, string> Headers { get; set; }
}

internal class DeviceJson
{
    public string brand { get; set; }
    public string model { get; set; }
    public string versionRelease { get; set; }
    public string versionIncremental { get; set; }
    public int versionSdk { get; set; }
    public string version { get; set; }
    public string build { get; set; }
    public string blob { get; set; }
}
