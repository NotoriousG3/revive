using System.Collections.Generic;

namespace SnapchatLib;

public enum SnapchatVersion
{
    V12_28_0_22,
    V12_27_0_8,
    V12_26_0_20,
}

public enum OS
{
    android
}
internal class SnapchatInfo
{
    public string Version;
    internal OS OS;
    private static readonly Dictionary<SnapchatVersion, SnapchatInfo> VersionKeyMap = new()
    {
        //Android
        { SnapchatVersion.V12_28_0_22, new SnapchatInfo { Version = "12.28.0.22", OS = OS.android }},
        { SnapchatVersion.V12_27_0_8, new SnapchatInfo { Version = "12.27.0.8", OS = OS.android }},
        { SnapchatVersion.V12_26_0_20, new SnapchatInfo { Version = "12.26.0.20", OS = OS.android }},
    };

    public static SnapchatInfo GetInfo(SnapchatVersion version)
    {
        return VersionKeyMap[version];
    }
}