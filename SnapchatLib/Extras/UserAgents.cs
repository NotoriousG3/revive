using SnapchatLib;
using SnapchatLib.Encryption;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

internal class CustomGrpcUserAgentHandler : DelegatingHandler
{
    private readonly string m_UserAgent;

    public CustomGrpcUserAgentHandler(SnapchatLockedConfig config)
    {
        m_UserAgent = UserAgentCreator.CreateUserAgent(config, true);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("User-Agent");
        request.Headers.TryAddWithoutValidation("User-Agent", m_UserAgent);
        return base.SendAsync(request, cancellationToken);
    }
}

internal static class UserAgentCreator
{
    internal static string CreateUserAgent(SnapchatLockedConfig config, bool isGrpc)
    {
        var deviceInfo = Base64.Base64Decode(config.DeviceProfile).Split(':');
        var snapchatInfo = SnapchatInfo.GetInfo(config.SnapchatVersion);

        if (config.OS == OS.android)
        {
            return CreateUserAgentAndroid(snapchatInfo.Version, deviceInfo[1], deviceInfo[2], deviceInfo[3], int.Parse(deviceInfo[4]), isGrpc);
        }
        else
        {
            return CreateUserAgentIos(snapchatInfo.Version, deviceInfo[1], deviceInfo[2], isGrpc);
        }
    }

    private static string CreateUserAgentIos(string appVersion, string deviceModel, string deviceVersion, bool isGrpc)
    {
        if (string.IsNullOrEmpty(appVersion) ||
            string.IsNullOrEmpty(deviceModel) ||
            string.IsNullOrEmpty(deviceVersion))
        {
            throw new ArgumentNullException("Invalid ios user-agent arguments");
        }

        var userAgent = $"Snapchat/{appVersion} ({deviceModel}; iOS {deviceVersion}; gzip)";

        if (isGrpc)
        {
            userAgent = $"{userAgent} grpc-c++/1.33.2 grpc-c/13.0.0 (ios; cronet_http)";
        }

        return userAgent;
    }

    private static string CreateUserAgentAndroid(string appVersion, string deviceModel, string deviceVersionRelease, string deviceVersionIncremental, int deviceVersionSdk, bool isGrpc)
    {
        if (string.IsNullOrEmpty(appVersion) ||
            string.IsNullOrEmpty(deviceModel) ||
            string.IsNullOrEmpty(deviceVersionRelease) ||
            string.IsNullOrEmpty(deviceVersionIncremental))
        {
            throw new ArgumentNullException("Invalid android user-agent arguments");
        }

        var userAgent = $"Snapchat/{appVersion} ({deviceModel}; Android {deviceVersionRelease}#{deviceVersionIncremental}#{deviceVersionSdk}; gzip) V/MUSHROOM";

        if (isGrpc)
        {
            userAgent = $"{userAgent} grpc-c++/1.33.2 grpc-c/13.0.0 (android; cronet_http)";
        }

        return userAgent;
    }
}

public class DisableActivityHandler : DelegatingHandler
{
    public DisableActivityHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Activity.Current = null;

        return base.SendAsync(request, cancellationToken);
    }
}