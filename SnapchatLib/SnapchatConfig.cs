using System;
using System.Net;
using System.Runtime.InteropServices;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Search;

namespace SnapchatLib;
public class SnapchatConfig
{
    private string _ApiKey;
    private string _User_ID;
    private string _Install_ID;
    private string _ClientID;
    private string _Device_ID;
    private string _DeviceProfile;
    private string _OldUsername;
    private string _Username;
    private string _AuthToken;
    private string _dtoken1i;
    private string _BusinessAccessToken;
    private string _refreshToken;
    private string _Access_Token;
    private string _AccountCountryCode;
    public WebProxy Proxy { get; set; }
    public static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
    }
    public string ApiKey
    {
        get => _ApiKey;
        set
        {
            _ApiKey = value;
            if (string.IsNullOrEmpty(_ApiKey)) throw new ArgumentNullException("ApiKey Cannot be empty");
        }
    }
    public string Install
    {
        get => _Install_ID;
        set
        {
            _Install_ID = value;
            if (!string.IsNullOrEmpty(_Install_ID))
            {
                if (_Install_ID.Length != 36) throw new Exception("Invalid Install");
            }
        }
    }
    public string Device
    {
        get => _Device_ID;
        set
        {
            _Device_ID = value;
            if (!string.IsNullOrEmpty(_Device_ID))
            {
                if (_Device_ID.Length != 36) throw new Exception("Invalid Device");
            }
        }
    }
    public string DeviceProfile
    {
        get => _DeviceProfile;
        set
        {
            _DeviceProfile = value;
            if (!string.IsNullOrEmpty(_DeviceProfile))
            {
                if (!IsBase64String(_DeviceProfile)) throw new Exception("Invalid DeviceProfile");
            }
        }
    }
    public string OldUsername
    {
        get => _OldUsername;
        set
        {
            _OldUsername = value;
            if (!string.IsNullOrEmpty(_OldUsername))
            {
                if (_OldUsername == Username) throw new Exception("Invalid OldUsername");
            }
        }
    }
    public string Username
    {
        get => _Username;
        set
        {
            _Username = value;
            if (!string.IsNullOrEmpty(_Username))
            {
                if (_Username == OldUsername) throw new Exception("Invalid Username");
            }
        }
    }
    public string AuthToken
    {
        get => _AuthToken;
        set
        {
            _AuthToken = value;
            if (!string.IsNullOrEmpty(_AuthToken))
            {
                if (_AuthToken.Length != 32) throw new Exception("Invalid AuthToken");
            }
        }
    }
    public string dtoken1i
    {
        get => _dtoken1i;
        set
        {
            _dtoken1i = value;
            if (!string.IsNullOrEmpty(_dtoken1i))
            {
                if (!_dtoken1i.Contains("00001:")) throw new Exception("Invalid dtoken1i");
            }
        }
    }

    public string ClientID
    {
        get => _ClientID;
        set
        {
            _ClientID = value;
            if (!string.IsNullOrEmpty(_ClientID))
            {
                if (_ClientID.Length != 36) throw new Exception("Invalid ClientID");
            }
        }
    }

    public string dtoken1v { get; set; }
    public string user_id
    {
        get => _User_ID;
        set
        {
            _User_ID = value;
            if (!string.IsNullOrEmpty(_User_ID))
            {
                if (!Guid.TryParse(_User_ID, out _)) throw new Exception("Invalid user_id");
            }
        }
    }

    public string BusinessAccessToken
    {
        get => _BusinessAccessToken;
        set
        {
            _BusinessAccessToken = value;
            if (!string.IsNullOrEmpty(_BusinessAccessToken))
            {
                if(!_BusinessAccessToken.StartsWith("ey")) throw new Exception("Invalid BusinessAccessToken");
            }
        }
    }

    public string refreshToken
    {
        get => _refreshToken;
        set
        {
            _refreshToken = value;
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                if (!_refreshToken.StartsWith("ey")) throw new Exception("Invalid refreshToken");
            }
        }
    }

    public string Access_Token
    {
        get => _Access_Token;
        set
        {
            _Access_Token = value;
            if (!string.IsNullOrEmpty(_Access_Token))
            {
                if (!_Access_Token.StartsWith("gE")) throw new Exception("Invalid Access_Token");
            }
        }
    }

    public string AccountCountryCode
    {
        get => _AccountCountryCode;
        set
        {
            _AccountCountryCode = value;
            if (!string.IsNullOrEmpty(_AccountCountryCode))
            {
                if (_AccountCountryCode.Length != 2) throw new Exception("Invalid AccountCountryCode");
            }
        }
    }

    public long install_time { get; set; }
    public SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign Horoscope { get; set; }
    public bool Debug { get; set; } = false;
    public string TimeZone { get; set; }
    public bool BandwithSaver { get; set; } = true;
    public bool StealthMode { get; set; } = true;
    public int Age { get; set; }
    public int Timeout { get; set; } = 5;
    public SnapchatVersion SnapchatVersion = EnumerableExtension.RandomEnumValue<SnapchatVersion>();
    public OS OS = OS.android;
    internal readonly IUtilities Utilities;

    internal SnapchatConfig(IUtilities utilities)
    {
        Utilities = utilities;
    }

    public SnapchatConfig()
    {
        Utilities = new Utilities();
    }
}

public class SnapchatLockedConfig
{
    public SnapchatLockedConfig(SnapchatConfig config)
    {
        install_time = config.install_time;
        Device = config.Device;
        Install = config.Install;
        Username = config.Username;
        ApiKey = config.ApiKey;
        Proxy = config.Proxy;
        Debug = config.Debug;
        OldUsername = config.OldUsername;
        OS = config.OS;
        BandwithSaver = config.BandwithSaver;
        Timeout = config.Timeout;
        AuthToken = config.AuthToken;
        SnapchatVersion = config.SnapchatVersion;
        dtoken1i = config.dtoken1i;
        dtoken1v = config.dtoken1v;
        user_id = config.user_id;
        DeviceProfile = config.DeviceProfile;
        Access_Token = config.Access_Token;
        BusinessAccessToken = config.BusinessAccessToken;
        TimeZone = config.TimeZone;
        Horoscope = config.Horoscope;
        AccountCountryCode = config.AccountCountryCode;
        StealthMode = config.StealthMode;
        refreshToken = config.refreshToken;
        ClientID = config.ClientID;
        Age = config.Age;
    }
    public SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign Horoscope { get; set; }
    public bool StealthMode { get; set; }
    internal bool IsWindowsOS { get; set; }
    public string AccountCountryCode { get; set; }
    public string refreshToken { get; set; }
    public string TimeZone { get; set; }
    public string DeviceProfile { get; set; }
    public string dtoken1i { get; set; }
    public string dtoken1v { get; set; }
    public int Age { get; set; }
    public WebProxy Proxy { get; set; }
    public string ApiKey { get; set; }
    public string Username { get; set; }
    public string OldUsername { get; set; }
    public string AuthToken { get; set; }
    public string Access_Token { get; set; }
    public string BusinessAccessToken { get; set; }
    public string Install { get; set; }
    public string Device { get; set; }
    public long install_time { get; set; }
    public bool Debug { get; set; }
    public bool BandwithSaver { get; set; }
    public string user_id { get; set; }
    public string ClientID { get; set; }
    public int Timeout { get; set; }
    public SnapchatVersion SnapchatVersion;
    public OS OS;
}