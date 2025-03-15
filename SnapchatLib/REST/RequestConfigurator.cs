using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST;

internal struct RequestConfiguration
{
    public string Endpoint;
    public HttpMethod HttpMethod;
    public string TempRequestToken;
    public string RequestToken;
    public string Timestamp;
    public bool IsMulti;
    public string Username;
    public string DToken1V;
    public OS OS;
    public string XSnapAccessToken;
    public string XSnapchatUserId;
}

[Flags]
internal enum EndpointRequirements
{
    Username = 1 << 0,
    SignAndroid = 1 << 1,
    DSIG = 1 << 2,
    XSnapAccessToken = 1 << 3,
    RequestToken = 1 << 4,
    UseTempRequestToken = 1 << 5,
    XSnapchatUserId = 1 << 6,
    IPV6UseAlternateAuthEndpoint = 1 << 7,
    XSnapchatUUID = 1 << 8,
    OSUserAgent = 1 << 9,
    AcceptEncoding = 1 << 10,
    AcceptProtoBuf = 1 << 11,
    ParamsAsHeaders = 1 << 12,
    UseOldUsername = 1 << 13,
    UseBusinessAccessToken = 1 << 14,
    ArgosHeader = 1 << 15
}

internal struct EndpointInfo
{
    public string BaseEndpoint;
    public string Url;
    public EndpointRequirements Requirements;
    public string SignUrlOverride = null;

    public EndpointInfo()
    {
        BaseEndpoint = RequestConfigurator.ApiBaseEndpoint;
        Url = "";
        Requirements = 0;
    }
}

internal interface IRequestConfigurator
{
    Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, HttpContent content, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false);
    Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, Dictionary<string, string> parameters, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false);
}

internal class RequestConfigurator : IRequestConfigurator
{
    internal static string ApiBaseEndpoint => "https://app.snapchat.com";
    internal static string MediaApiBaseEndpoint => "https://mvm.snapchat.com";
    internal static string ApiGCPEast4Endpoint => "https://us-east4-gcp.api.snapchat.com";
    internal static string ApiAWSEast1Endpoint => "https://us-east1-aws.api.snapchat.com";
    internal static string ProAccountsEndpoint => "https://pro-accounts.snapchat.com";
    internal static string ProStoriesEndpoint => "https://pro-stories.snapchat.com";
    internal static string ApiGCPEndpoint => "https://gcp.api.snapchat.com";
    internal static string IPV6AuthEndpoint => "https://auth.snapchat.com";
    internal static string SearchBaseEndpoint => "https://aws.api.snapchat.com";

    internal static string XSnapAccessTokenHeaderName => "X-Snap-Access-Token";
    internal static string XSnapchatUserIdHeaderName => "x-snapchat-user-id";
    internal static string XSnapchatUUIDHeaderName => "x-snapchat-uuid";
    internal static string AcceptEncodingHeaderName => "Accept-Encoding";
    internal static string AcceptHeaderName => "Accept";
    internal static string AcceptLanguageHeaderName => "Accept-Language";
    internal static string AcceptLocaleHeaderName => "Accept-Locale";
    internal static string TimestampHeaderName => "timestamp";
    internal static string UsernameHeaderName => "username";
    internal static string DsigHeaderName => "dsig";
    internal static string ReqTokenHeaderName => "req_token";

    internal static string AcceptLanguageValue => "en";
    internal static string AcceptLocaleValue => "en_US";
    internal static string AcceptEncodingValue => "gzip, deflate, br";
    internal static string ApplicationProtobufValue => "application/x-protobuf";
    internal static string ApplicationJsonValue => "application/json";


    private readonly IUtilities m_Utilities;
    private readonly IClientLogger m_Logger;

    public RequestConfigurator(IClientLogger logger, IUtilities utilities)
    {
        m_Logger = logger;
        m_Utilities = utilities;
    }

    public async Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, HttpContent content, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false)
    {
        var config = CreateConfig(endpointInfo, httpMethod, client, isMulti);
        return await GenerateRequest(httpClient, config, endpointInfo, content);
    }

    public async Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, Dictionary<string, string> parameters, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false)
    {
        var config = CreateConfig(endpointInfo, httpMethod, client, isMulti);
        return await GenerateRequest(httpClient, config, endpointInfo, parameters, isMulti);
    }

    private RequestConfiguration CreateConfig(EndpointInfo endpointInfo, HttpMethod httpMethod, SnapchatClient client, bool isMulti = false)
    {
        var timestamp = m_Utilities.UtcTimestamp().ToString();
        var request_token = m_Utilities.GenerateRequestToken(client.SnapchatConfig.AuthToken, timestamp);
        var static_req_token = m_Utilities.GenerateTemporaryRequestToken(timestamp);
        var accessTokenToUse = endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapAccessToken) ? endpointInfo.Requirements.HasFlag(EndpointRequirements.UseBusinessAccessToken) ? client.SnapchatConfig.BusinessAccessToken : client.SnapchatConfig.Access_Token : null;
        // Create the configuration object that will be used to setup the request parameters
        var config = new RequestConfiguration
        {
            Endpoint = endpointInfo.Url,
            HttpMethod = httpMethod,
            DToken1V = client.SnapchatConfig.dtoken1v,
            OS = client.SnapchatConfig.OS,
            IsMulti = isMulti,
            RequestToken = request_token,
            TempRequestToken = static_req_token,
            Timestamp = timestamp,
            Username = endpointInfo.Requirements.HasFlag(EndpointRequirements.UseOldUsername) && client.SnapchatConfig.OldUsername != null ? client.SnapchatConfig.OldUsername : client.SnapchatConfig.Username,
            XSnapAccessToken = accessTokenToUse,
            XSnapchatUserId = client.SnapchatConfig.user_id,
        };

        return config;
    }

    private async Task<HttpRequestMessage> CreateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo)
    {
        var baseEndpoint = endpointInfo.BaseEndpoint;
        var url = baseEndpoint + configuration.Endpoint;
        var request = new HttpRequestMessage(configuration.HttpMethod, url);
        request.Version = configuration.HttpMethod == HttpMethod.Put ? HttpVersion.Version11 : HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        var req_id = m_Utilities.NewGuid();
        // Default for every request
        request.Headers.TryAddWithoutValidation(AcceptLanguageHeaderName, AcceptLanguageValue);
        request.Headers.TryAddWithoutValidation(AcceptLocaleHeaderName, AcceptLocaleValue);
        request.Headers.TryAddWithoutValidation("x-request-id", req_id);
        if (configuration.Endpoint == "/bq/post_story")
        {
            request.Headers.TryAddWithoutValidation(UsernameHeaderName, configuration.Username);
            request.Headers.TryAddWithoutValidation(ReqTokenHeaderName, configuration.RequestToken);
            request.Headers.TryAddWithoutValidation(TimestampHeaderName, configuration.Timestamp);
        }
        // We only sign on POST
        if (configuration.HttpMethod == HttpMethod.Post && configuration.Endpoint != "" && configuration.Endpoint != "/bitmoji-api/avatar-service/create-avatar-data" && configuration.Endpoint != "/snap_token/pb/snap_session" && configuration.Endpoint != "/search/search" && configuration.Endpoint != "/search/pretype" && configuration.Endpoint != "/readreceipt-indexer/batchuploadreadreceipts" && configuration.Endpoint != "/suggest_friend_high_availability" && configuration.Endpoint != "/ami/friends" && configuration.Endpoint != "/story-management-service/get_active_story_status" && configuration.Endpoint != "/df-mixer-prod/soma/batch_stories" && configuration.Endpoint != "/scauth/validate")
        {
            if (configuration.OS == OS.android && configuration.Endpoint != "/loq/device_id")
            {

                // Sign
                var signToken = configuration.Endpoint is "/scauth/login" or "/loq/register_v2" or "/loq/device_id" or "/account/odlv/request_otp" or "/loq/warm_user" ? configuration.TempRequestToken : configuration.RequestToken;

                var signResult = m_Utilities.JsonDeserializeObject<SignJson>(await client.Sign.SignRequest(signToken, configuration.Endpoint));

                if (signResult == null)
                    throw new SignerException("Could not deserialize SignRequest response");

                m_Logger.Debug("Trying to add sign headers to request");
                request.Headers.UserAgent.Clear();
                foreach (var (key, value) in signResult.Headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapAccessToken)) request.Headers.TryAddWithoutValidation(XSnapAccessTokenHeaderName, configuration.XSnapAccessToken);

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.ArgosHeader))
        {
            request.Headers.TryAddWithoutValidation("x-snapchat-att-token", await client.Get_Tokens.GetArgosTokenCached());
            request.Headers.TryAddWithoutValidation("x-snapchat-argos-strict-enforcement", "true");
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapchatUserId)) request.Headers.TryAddWithoutValidation(XSnapchatUserIdHeaderName, configuration.XSnapchatUserId);

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapchatUUID)) request.Headers.TryAddWithoutValidation(XSnapchatUUIDHeaderName, req_id);

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.AcceptEncoding)) request.Headers.TryAddWithoutValidation(AcceptEncodingHeaderName, AcceptEncodingValue);

        // By default, always add application/json as a header
        request.Headers.TryAddWithoutValidation(AcceptHeaderName, endpointInfo.Requirements.HasFlag(EndpointRequirements.AcceptProtoBuf) ? ApplicationProtobufValue : ApplicationJsonValue);

        return request;
    }

    private void AddExtraData(HttpRequestMessage request, EndpointInfo endpointInfo, RequestConfiguration configuration, IDictionary<string, string> parameters)
    {

        var paramsAsHeaders = endpointInfo.Requirements.HasFlag(EndpointRequirements.ParamsAsHeaders);
        if (paramsAsHeaders)
            request.Headers.TryAddWithoutValidation(TimestampHeaderName, configuration.Timestamp);
        else
            parameters?.TryAdd(TimestampHeaderName, configuration.Timestamp);


        if (endpointInfo.BaseEndpoint != "/snap_token/pb/snap_session")
        {
            // Append the username
            if (endpointInfo.Requirements.HasFlag(EndpointRequirements.Username))
            {
                if (paramsAsHeaders)
                    request.Headers.TryAddWithoutValidation(UsernameHeaderName, configuration.Username);
                else
                    parameters?.TryAdd(UsernameHeaderName, configuration.Username);
            }
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.SignAndroid) && configuration.OS == OS.android || endpointInfo.Requirements.HasFlag(EndpointRequirements.DSIG) && configuration.OS != OS.android)
        {

            if (configuration.OS == OS.android)
            {
                var deviceSignature = new DeviceSignature();
                var token = endpointInfo.Requirements.HasFlag(EndpointRequirements.UseTempRequestToken) ? configuration.TempRequestToken : configuration.RequestToken;
                var sig = deviceSignature.Sign(configuration.DToken1V, configuration.Username, null, null);
                if (paramsAsHeaders)
                    request.Headers.TryAddWithoutValidation(DsigHeaderName, sig);
                else
                    parameters?.TryAdd(DsigHeaderName, sig);
            }
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.RequestToken))
        {
            if (paramsAsHeaders)
                request.Headers.TryAddWithoutValidation(ReqTokenHeaderName, endpointInfo.Requirements.HasFlag(EndpointRequirements.UseTempRequestToken) ? configuration.TempRequestToken : configuration.RequestToken);
            else
                parameters?.TryAdd(ReqTokenHeaderName, endpointInfo.Requirements.HasFlag(EndpointRequirements.UseTempRequestToken) ? configuration.TempRequestToken : configuration.RequestToken);
        }
    }

    private async Task<HttpRequestMessage> GenerateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo, HttpContent content)
    {
        var request = await CreateRequest(client, configuration, endpointInfo);

        // This is required to be able to reuse the same code
        Dictionary<string, string> d = null;
        AddExtraData(request, endpointInfo, configuration, d);

        request.Content = content;
        return request;
    }

    private async Task<HttpRequestMessage> GenerateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo, Dictionary<string, string> parameters, bool ismulti)
    {
        var request = await CreateRequest(client, configuration, endpointInfo);

        AddExtraData(request, endpointInfo, configuration, parameters);
        if (ismulti)
        {
            var content = new MultipartFormDataContent();
            foreach (var parameter in parameters)
            {
                Console.WriteLine(parameter.Key);
                content.Add(new StringContent(parameter.Value), parameter.Key);
            }
            request.Content = content;
        }
        else
        {
            var content = new FormUrlEncodedContent(parameters);
            content.Headers.ContentType.CharSet = "utf-8";
            request.Content = content;
        }

        return request;
    }
}