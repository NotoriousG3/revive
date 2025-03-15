using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Perception.Content_understanding;

namespace SnapchatLib.REST.Endpoints;

internal struct JInfo
{
    public string Version;
    public string Key;
}

internal interface ISignEndpoint
{
    Task<string> SignRequest(string req_token, string path);
    Task GetDeviceInfo();
    Task SendMetrics(string key, string value);
}

internal class SignEndpoint : EndpointAccessor, ISignEndpoint
{
    internal const string DefaultSignUrl = "https://ghost.siggen.io";

    public SignEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    internal static HttpResponseMessage response { get; set; }
    internal static Dictionary<string, object> requestData { get; set; } = new Dictionary<string, object>();

    private void RaiseForInvalidValues(string path)
    {

        if (Config.Device == null || Config.install_time == 0 || Config.Install == null || Config.dtoken1i == null || Config.dtoken1v == null)
            throw new Exception("SnapchatConfig.Device SnapchatConfig.install_time SnapchatConfig.Install SnapchatConfig.dtoken1i SnapchatConfig.dtoken1v is required");

        if (string.IsNullOrEmpty(Config.DeviceProfile))
            throw new Exception("SnapchatConfig.DeviceProfile is required");

        if (path != "/loq/device_id")
        {
            if (string.IsNullOrEmpty(Config.dtoken1i)) throw new ArgumentNullException("dtoken1i can no longer be null");
            if (string.IsNullOrEmpty(Config.dtoken1v)) throw new ArgumentNullException("dtoken1v can no longer be null");
        }

        if (path != "/snapchat.janus.api.LoginService/LoginWithPassword" && path != "/snapchat.janus.api.RegistrationService/RegisterWithUsernamePassword" && path != "/snap.security.ArgosService/GetTokens" && path != "/loq/device_id" && path != "/loq/and/register_exp" && path != "/loq/register_v2" && path != "/snapchat.janus.api.LoginService/SendODLVCode" && path != "/snapchat.janus.api.LoginService/VerifyODLV")
        {
            if (string.IsNullOrEmpty(Config.Access_Token))
                throw new Exception("Access Token was null protecting from ban");
        }

        if (Config.Device == null)
        {
            throw new DeviceNotSetException();
        }
        if (Config.Install == null)
        {
            throw new InstallNotSetException();
        }
        if (Config.install_time == 0)
        {
            throw new SignerException("install_time is 0");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new SignerException("v cannot be null nor empty");
        }
    }
    private string GetUrlToSign(string endpointOverride)
    {
        return $"{DefaultSignUrl}/{endpointOverride}";
    }

    public async Task GetDeviceInfo()
    {
        if (string.IsNullOrEmpty(Config.DeviceProfile))
        {
            DateTime installTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    .AddMilliseconds(Config.install_time);
            DateTime launchTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(SnapchatClient.launchTimestamp);

            if (DateTime.Compare(installTime, launchTime) > 0)
            {
                throw new Exception("install_time incorrect");
            }

            var info = SnapchatInfo.GetInfo(Config.SnapchatVersion);

            SignDevice a = null;

            if (Config.OS == OS.android)
            {
                a = new SignDevice { config = new SignConfig { platform = "android", version = info.Version }, filter = new Filter { versionSdk = new Random().Next(24, 31) } };
            }
            var url = GetUrlToSign("get_device");
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Version = HttpVersion.Version20;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            request.Headers.TryAddWithoutValidation("JSnap-Key", Config.ApiKey);
            request.Content = new StringContent(m_Utilities.JsonSerializeObject(a), Encoding.UTF8, "application/json");
            // This cannot use EndpointAccesor.Send because THAT tries to use this method to sign a request. We also do not want to clean up here
            // because there's usually a second request at this point
            response = await SnapchatHttpClient.Send(url, request, false);

            var responseData = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new SignerException(responseData);

            var result = m_Utilities.JsonDeserializeObject<DeviceJson>(responseData);

            if (Config.OS == OS.android)
                Config.DeviceProfile = Base64.Base64Encode(result.brand + ":" + result.model + ":" + result.versionRelease + ":" + result.versionIncremental + ":" + result.versionSdk);
        }
    }
    public async Task<string> SignRequest(string t, string p)
    {
        if (t == null)
            t = "";

        DateTime installTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(Config.install_time);
        DateTime launchTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(SnapchatClient.launchTimestamp);

        if (DateTime.Compare(installTime, launchTime) > 0)
        {
            throw new Exception("install_time incorrect");
        }


        var info = SnapchatInfo.GetInfo(Config.SnapchatVersion);

        if (Config.OS != info.OS)
            throw new SignerException("Version Dosen't match OS");

        RaiseForInvalidValues(p);

        string[] split = Base64.Base64Decode(Config.DeviceProfile).Split(":");
        Sign sign = null;
        if (Config.OS == OS.android)
        {
            sign = new Sign { config = new SignConfig { platform = "android", version = info.Version }, persist = new Persist { deviceAndroid = new DeviceAndroid { model = split[1], versionIncremental = split[3], versionRelease = split[2], versionSdk = Convert.ToInt32(split[4]) }, deviceTokenIdentifier = Config.dtoken1i, installId = Config.Install, installTimestamp = Config.install_time }, request = new RequestSign { path = p, token = t }, session = new Session { launchTimestamp = SnapchatClient.launchTimestamp, sequenceAuth = SnapchatClient.s_a, sequenceRequest = SnapchatClient.s_r, sessionId = SnapchatClient.sessionId } };
        }
        var url = "";


        if (Config.OS == OS.android)
            url = GetUrlToSign("sign_android");

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Version = HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

        request.Headers.TryAddWithoutValidation("JSnap-Key", Config.ApiKey);
        request.Content = new StringContent(m_Utilities.JsonSerializeObject(sign), Encoding.UTF8, "application/json");

        // This cannot use EndpointAccesor.Send because THAT tries to use this method to sign a request. We also do not want to clean up here
        // because there's usually a second request at this point
        response = await SnapchatHttpClient.Send(url, request, false);

        var responseData = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
            throw new SignerException(responseData);

        SnapchatClient.s_r = SnapchatClient.s_r += 1;

        var result = m_Utilities.JsonDeserializeObject<SignJson>(responseData);
        if (result.Headers != null)
            return responseData;

        throw new SignerException(responseData);
    }
    public async Task SendMetrics(string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrEmpty(value))
        {
            using (var _c = new HttpClient())
            {
                _c.Timeout = TimeSpan.FromSeconds(SnapchatClient.SnapchatConfig.Timeout);
                _c.DefaultRequestHeaders.TryAddWithoutValidation("JSnap-Key", Config.ApiKey);
                var info = SnapchatInfo.GetInfo(Config.SnapchatVersion);
                var metrics = new Metrics
                {
                    config = new SignConfig { platform = "android", version = info.Version },
                    key = key,
                    value = value,
                    origin = "Middleware"
                };
                _c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Environment.UserName);
                var keys = await _c.PostAsync("https://lib.signer.sh/metrics", new StringContent(m_Utilities.JsonSerializeObject(metrics), Encoding.UTF8, "application/json"));
            }
        }
    }
}