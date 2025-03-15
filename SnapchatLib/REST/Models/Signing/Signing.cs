namespace SnapchatLib.REST.Models
{
    public class SignConfig
    {
        public string platform { get; set; }
        public string version { get; set; }
    }

    public class Metrics
    {
        public SignConfig config { get; set; }
        public string origin { get; set; }
        public string key { get; set; }
        public string value { get; set; }
    }


    public class Filter
    {
        public string brand { get; set; }
        public string model { get; set; }
        public string versionRelease { get; set; }
        public string versionIncremental { get; set; }
        public int versionSdk { get; set; }
    }

    public class SignDevice
    {
        public Filter filter { get; set; }
        public SignConfig config { get; set; }
    }
    public class DeviceAndroid
    {
        public string model { get; set; }
        public string versionRelease { get; set; }
        public string versionIncremental { get; set; }
        public int versionSdk { get; set; }
    }
    public class DeviceIos
    {
        public string blob { get; set; }
        public string model { get; set; }
        public string version { get; set; }
        public string build { get; set; }
    }
    public class Persist
    {
        public long installTimestamp { get; set; }
        public string installId { get; set; }
        public string deviceTokenIdentifier { get; set; }
        public DeviceAndroid deviceAndroid { get; set; }
        public DeviceIos deviceIos { get; set; }
    }

    public class RequestSign
    {
        public string path { get; set; }
        public string token { get; set; }
    }

    public class Sign
    {
        public SignConfig config { get; set; }
        public Session session { get; set; }
        public Persist persist { get; set; }
        public RequestSign request { get; set; }
    }

    public class Session
    {
        public string sessionId { get; set; }
        public int sequenceRequest { get; set; }
        public int sequenceAuth { get; set; }
        public long launchTimestamp { get; set; }
    }
}
