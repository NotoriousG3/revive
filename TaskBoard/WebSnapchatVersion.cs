using SnapchatLib;

namespace TaskBoard;

public struct WebSnapchatVersion
{
    public SnapchatVersion SnapchatVersion;
    public bool CanRegister;

    public WebSnapchatVersion(SnapchatVersion version, bool canRegister)
    {
        SnapchatVersion = version;
        CanRegister = canRegister;
    }
    public static WebSnapchatVersion V12_26_0_20 = new(SnapchatVersion.V12_26_0_20, true);

    public static readonly Dictionary<OS, WebSnapchatVersion[]> VersionMap = new()
    {
        {
            OS.android, new[]
            {
                V12_26_0_20,
            }
        }
    };
}