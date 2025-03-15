using System;
using System.Collections.Generic;
using System.Net.Http;
using Grpc.Core;
using System.Threading.Tasks;
using Grpc.Net.Client;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Activation.Api;
using SnapProto.Snapchat.Abuse.Support;
using SnapProto.Snapchat.Content.V2;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snap.Security;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Cdp.Cof;
using SnapProto.Services.Snapchat.Activation.Api;
using SnapProto.Services.Snapchat.Friending.Server;
using SnapProto.Services.Snapchat.Cdp.Cof;
using SnapProto.Services.Snapchat.Janus.Api;
using SnapProto.Services.Snap.Security;
using SnapProto.Services.Snapchat.Abuse.Support;
using SnapProto.Services.Snapchat.Content.V2;
using SnapProto.Services.Messagingcoreservice;
using SnapProto.Services.Com.Snapchat.Deltaforce.External;
using SnapProto.Com.Snapchat.Deltaforce;
using SnapProto.Services.Snapchat.Notification.Notificationdata;
using SnapProto.Snapchat.Notification.Notificationdata;
using SnapchatLib.Encryption;
using SnapProto.Com.Snapchat.Proto.Security;
using SnapProto.Services.Com.Snapchat.Proto.Security;
using DeltaSyncRequest = SnapProto.Snapchat.Messaging.DeltaSyncRequest;
using DeltaSyncResponse = SnapProto.Snapchat.Messaging.DeltaSyncResponse;
using System.Net;
using System.Threading;
using Google.Protobuf;

namespace SnapchatLib;

internal readonly struct GrpcSignResult
{
    public long Timestamp { get; }
    public string RequestToken { get; }
    public string Attestation { get; }

    public GrpcSignResult(long timestamp, string requestToken, string attestation)
    {
        Timestamp = timestamp;
        RequestToken = requestToken;
        Attestation = attestation;
    }
}

internal interface ISnapchatGrpcClient
{
    Task<List<DeliveryDestination>> CreateDestinations(HashSet<string> users);
    void SetupServiceClients();
    Task<GrpcSignResult> Sign(string url);
    Task<DeltaSyncResponse> DeltaSyncAsync(DeltaSyncRequest request);
    AsyncUnaryCall<SCSuggestUsernamePbSuggestUsernameResponse> SuggestUsernameAsync(SCSuggestUsernamePbSuggestUsernameRequest request);
    AsyncUnaryCall<SCCofConfigTargetingResponse> CofAsync(SCCofConfigTargetingRequest request);
    AsyncUnaryCall<BatchDeltaSyncResponse> BatchDeltaSyncAsync(BatchDeltaSyncRequest request);
    AsyncUnaryCall<ConditionalPutResponse> ConditionalPutAsync(ConditionalPutRequest request);
    AsyncUnaryCall<SCJanusRegisterWithUsernamePasswordResponse> RegisterAsync(SCJanusRegisterWithUsernamePasswordRequest request);
    AsyncUnaryCall<SCJanusLoginWithPasswordResponse> LoginAsync(SCJanusLoginWithPasswordRequest request);
    AsyncUnaryCall<SCJanusSendODLVCodeResponse> SendEmail2FA(SCJanusSendODLVCodeRequest request);
    AsyncUnaryCall<SCJanusVerifyODLVResponse> LoginEmail2FA(SCJanusVerifyODLVRequest request);
    Task<SCPBSecurityGetUrlReputationResponse> CheckUrl(SCPBSecurityGetUrlReputationRequest request);
    AsyncUnaryCall<ArgosGetTokensResponse> GetTokensAsync(ArgosGetTokensRequest request);
    AsyncUnaryCall<SyncConversationsResponse> SyncConversationsAsync(SyncConversationsRequest request);
    AsyncUnaryCall<CreateContentMessageResponse> CreateContentMessageAsync(CreateContentMessageRequest request);
    AsyncUnaryCall<SCBoltv2GetUploadLocationsResponse> GetUploadLocationsAsync(SCBoltv2GetUploadLocationsRequest request);
    AsyncUnaryCall<SCChangeUsernamePbChangeUsernameResponse> ChangeUsernameAsync(SCChangeUsernamePbChangeUsernameRequest request);
    Task<SCReportSendReportResponse> ReportUserAsync(SCReportSendReportRequest request);
    Task<SCFriendingFriendsActionResponse> AddFriendAsync(SCFriendingFriendsAddRequest request);
    Task<SCFriendingFriendsActionResponse> RemoveFriendAsync(SCFriendingFriendsRemoveRequest request);
    Task<SCFriendingFriendsActionResponse> BlockFriendsAsync(SCFriendingFriendsBlockRequest request);
    Task<SCFriendingFriendsActionResponse> UnblockFriendsAsync(SCFriendingFriendsUnblockRequest request);
    Task<SCFriendingContactBookUploadResponse> FullSyncContactBookUploadAsync(SCFriendingContactBookUploadRequest request);
    Task<SCFriendingFriendsActionResponse> ChangeDisplayNameForFriendAsync(SCFriendingFriendsDisplayNameChangeRequest request);
    AsyncUnaryCall<SCNotificationRegisterDeviceResponse> RegisterDeviceAsync(SCNotificationRegisterDeviceRequest request);
    AsyncUnaryCall<SCChangeUsernamePbGetLatestUsernameChangeDateResponse> GetLatestUsernameChangeDate(SCChangeUsernamePbGetLatestUsernameChangeDateRequest request);
}

internal class SnapchatGrpcClient : ISnapchatGrpcClient
{
    private HttpClient AndroidHttpClient
    {
        get
        {
            if (m_AndroidHttpClient != null) ConfigureClient(m_AndroidHttpClient);
            else m_AndroidHttpClient = CreateHttpClient();
            return m_AndroidHttpClient;
        }
    }

    private HttpClient m_AndroidHttpClient;

    private HttpClient IOSHttpClient
    {
        get
        {
            if (m_IOSHttpClient != null) ConfigureClient(m_IOSHttpClient);
            else m_IOSHttpClient = CreateHttpClient();
            return m_IOSHttpClient;
        }
    }

    private HttpClient m_IOSHttpClient;

    private HttpClient HttpClient => m_SnapchatClient.SnapchatConfig.OS == OS.android ? AndroidHttpClient : IOSHttpClient;

    private readonly SnapchatClient m_SnapchatClient;
    private readonly IUtilities m_Utilities;
    private int ConfigTimeout => m_SnapchatClient.SnapchatConfig.Timeout;
    private MediaDeliveryService.MediaDeliveryServiceClient MediaDeliveryServiceClient { get; set; }
    private MessagingCoreService.MessagingCoreServiceClient MessagingCoreServiceClient { get; set; }
    private ChangeUsernameService.ChangeUsernameServiceClient ChangeUsernameServiceClient;
    private ReportService.ReportServiceClient ReportServiceClient;
    private ArgosService.ArgosServiceClient ArgosServiceClient { get; set; }
    private FriendAction.FriendActionClient FriendActionClient { get; set; }
    private LoginService.LoginServiceClient LoginServiceClient { get; set; }
    private RegistrationService.RegistrationServiceClient RegistrationServiceClient { get; set; }
    private CircumstancesService.CircumstancesServiceClient CircumstancesClient { get; set; }
    private SuggestUsernameService.SuggestUsernameServiceClient SuggestUsernameServiceClient { get; set; }
    private ContactBook.ContactBookClient ContactBookClient { get; set; }
    private UrlReputationService.UrlReputationServiceClient UrlReputationServiceClient { get; set; }
    private DeltaForce.DeltaForceClient DeltaForceClient { get; set; }
    private PushNotificationDataRegistryService.PushNotificationDataRegistryServiceClient PushNotificationDataRegistryServiceClient { get; set; }
    public SnapchatGrpcClient(SnapchatClient client, IUtilities utilities)
    {
        m_SnapchatClient = client;
        m_Utilities = utilities;
    }

    public void SetupServiceClients()
    {
        var channel = CreateChannel();
        var channel2 = CreateChannel2();
        MediaDeliveryServiceClient ??= new MediaDeliveryService.MediaDeliveryServiceClient(channel);
        MessagingCoreServiceClient ??= new MessagingCoreService.MessagingCoreServiceClient(channel2);
        ChangeUsernameServiceClient ??= new ChangeUsernameService.ChangeUsernameServiceClient(channel);
        ReportServiceClient ??= new ReportService.ReportServiceClient(channel);
        ArgosServiceClient ??= new ArgosService.ArgosServiceClient(channel);
        FriendActionClient ??= new FriendAction.FriendActionClient(channel);
        LoginServiceClient ??= new LoginService.LoginServiceClient(channel);
        CircumstancesClient ??= new CircumstancesService.CircumstancesServiceClient(channel);
        SuggestUsernameServiceClient ??= new SuggestUsernameService.SuggestUsernameServiceClient(channel);
        ContactBookClient ??= new ContactBook.ContactBookClient(channel);
        RegistrationServiceClient ??= new RegistrationService.RegistrationServiceClient(channel);
        DeltaForceClient ??= new DeltaForce.DeltaForceClient(channel);
        PushNotificationDataRegistryServiceClient ??= new PushNotificationDataRegistryService.PushNotificationDataRegistryServiceClient(channel);
        UrlReputationServiceClient ??= new UrlReputationService.UrlReputationServiceClient(channel);
    }

    public async Task<GrpcSignResult> Sign(string url)
    {
        var timestamp = m_Utilities.UtcTimestamp();
        var req_token = m_Utilities.GenerateRequestToken(m_SnapchatClient.SnapchatConfig.AuthToken, timestamp.ToString());

        if (url == "/snapchat.janus.api.LoginService/LoginWithPassword" ||
            url == "/snapchat.janus.api.RegistrationService/RegisterWithUsernamePassword" ||
            url == "/snap.security.ArgosService/GetTokens" || url == "/snapchat.janus.api.LoginService/VerifyODLV" || url == "/snapchat.janus.api.LoginService/SendODLVCode")
        {
            var signResult = m_Utilities.JsonDeserializeObject<SignJson>(await m_SnapchatClient.SignRequest(null, url));
            return new GrpcSignResult(timestamp, req_token, signResult.Headers["x-snapchat-att"]);
        }
        else
        {
            var signResult = m_Utilities.JsonDeserializeObject<SignJson>(await m_SnapchatClient.SignRequest(req_token, url));
            return new GrpcSignResult(timestamp, req_token, signResult.Headers["x-snapchat-att"]);
        }
    }

    private async Task<Metadata> CreateMetadataAsync(string url)
    {
        var attestation = await Sign(url);

        if (url == "/snapchat.janus.api.LoginService/LoginWithPassword" ||
    url == "/snapchat.janus.api.RegistrationService/RegisterWithUsernamePassword" ||
    url == "/snap.security.ArgosService/GetTokens")
        {

            if (string.IsNullOrEmpty(attestation.Attestation))
                throw new Exception("Attestation was null protecting from ban");
        }
        else
        {
            if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
                throw new Exception("Access Token was null protecting from ban");
        }

        return new Metadata
        {
             { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
             { "x-snapchat-att", attestation.Attestation }
        };
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new DisableActivityHandler(new HttpClientHandler
        {
            Proxy = m_SnapchatClient.SnapchatConfig.Proxy,
            AutomaticDecompression = DecompressionMethods.All
        });
        var client = new HttpClient(new CustomGrpcUserAgentHandler(m_SnapchatClient.SnapchatConfig)
        {
            InnerHandler = handler,
        });

        client.Timeout = TimeSpan.FromSeconds(m_SnapchatClient.SnapchatConfig.Timeout);
        ConfigureClient(client);
        return client;
    }

    private void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
    }

    public GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress("https://aws.api.snapchat.com/", new GrpcChannelOptions
        {
            HttpClient = HttpClient
        });
    }
    public GrpcChannel CreateChannel2()
    {
        return GrpcChannel.ForAddress("https://aws-proxy-gcp.api.snapchat.com/", new GrpcChannelOptions
        {
            HttpClient = HttpClient
        });
    }
    public async Task<List<DeliveryDestination>> CreateDestinations(HashSet<string> users)
    {

        var info = await m_SnapchatClient.GetConversationID(users);

        if (info == null)
            throw new Exception($"Conversation ID not found for users: {string.Join(", ", users)}");

        var destinations = new List<DeliveryDestination>();

        // - Create DeliveryDestination
        foreach (var c in info)
        {
            destinations.Add(new DeliveryDestination
            {
                ConversationDestination = new ConversationDestination
                {
                    Id = c.ConversationId,
                    CurrentVersion = c.ConversationVersion
                }
            });
        }

        return destinations;
    }

    private DateTime DeadlineFromTimeout()
    {
        return DateTime.UtcNow.AddSeconds(ConfigTimeout);
    }

    /**
     * 
     * If any GRPC call requires x-snapchat-att, add this:
     * 
     * <code>, metadata: await CreateMetadataAsync(url)</code>
     * 
     */
    public async Task<DeltaSyncResponse> DeltaSyncAsync(DeltaSyncRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.mcs_cof_ids_bin } }.ToByteArray() }
        };

        return await MessagingCoreServiceClient.DeltaSyncAsync(request, metadata, DeadlineFromTimeout(), CancellationToken.None);
    }
    public AsyncUnaryCall<BatchDeltaSyncResponse> BatchDeltaSyncAsync(BatchDeltaSyncRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MessagingCoreServiceClient.BatchDeltaSyncAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }


    public AsyncUnaryCall<SCNotificationRegisterDeviceResponse> RegisterDeviceAsync(SCNotificationRegisterDeviceRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return PushNotificationDataRegistryServiceClient.RegisterDeviceAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snap-route-tag", "" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<ConditionalPutResponse> ConditionalPutAsync(ConditionalPutRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return DeltaForceClient.ConditionalPutAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-device-id", "-" + new Random().Next(000000, 999999) } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingFriendsActionResponse> AddFriendAsync(SCFriendingFriendsAddRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.AddFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingFriendsActionResponse> BlockFriendsAsync(SCFriendingFriendsBlockRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.BlockFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCFriendingFriendsActionResponse> UnblockFriendsAsync(SCFriendingFriendsUnblockRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.UnblockFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCFriendingFriendsActionResponse> RemoveFriendAsync(SCFriendingFriendsRemoveRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.RemoveFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingFriendsActionResponse> ChangeDisplayNameForFriendAsync(SCFriendingFriendsDisplayNameChangeRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.ChangeDisplayNameForFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingContactBookUploadResponse> FullSyncContactBookUploadAsync(SCFriendingContactBookUploadRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await ContactBookClient.FullSyncContactBookUploadAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<ArgosGetTokensResponse> GetTokensAsync(ArgosGetTokensRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ArgosServiceClient.GetTokensAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snap-route-tag", "" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SyncConversationsResponse> SyncConversationsAsync(SyncConversationsRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MessagingCoreServiceClient.SyncConversationsAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<CreateContentMessageResponse> CreateContentMessageAsync(CreateContentMessageRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MessagingCoreServiceClient.CreateContentMessageAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCBoltv2GetUploadLocationsResponse> GetUploadLocationsAsync(SCBoltv2GetUploadLocationsRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MediaDeliveryServiceClient.getUploadLocationsAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCChangeUsernamePbChangeUsernameResponse> ChangeUsernameAsync(SCChangeUsernamePbChangeUsernameRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ChangeUsernameServiceClient.ChangeUsernameAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCReportSendReportResponse> ReportUserAsync(SCReportSendReportRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var attestation = await Sign("/snapchat.abuse.support.ReportService/SendReport");
        var OS = m_SnapchatClient.SnapchatConfig.OS == SnapchatLib.OS.android ? 1 : 0;
        return await ReportServiceClient.SendReportAsync(request, new Metadata { { "device_model", Base64.Base64Decode(m_SnapchatClient.SnapchatConfig.DeviceProfile).Split(":")[1] }, { "country_code", m_SnapchatClient.SnapchatConfig.AccountCountryCode }, { "locale", "en_US" }, { "os_type", OS.ToString() }, { "user_id", m_SnapchatClient.SnapchatConfig.user_id }, { "x-snapchat-att", attestation.Attestation }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCPBSecurityGetUrlReputationResponse> CheckUrl(SCPBSecurityGetUrlReputationRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var attestation = await Sign("/com.snapchat.proto.security.UrlReputationService/GetUrlReputation");
        return await UrlReputationServiceClient.GetUrlReputationAsync(request, new Metadata { { "x-snapchat-att", attestation.Attestation }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCChangeUsernamePbGetLatestUsernameChangeDateResponse> GetLatestUsernameChangeDate(SCChangeUsernamePbGetLatestUsernameChangeDateRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ChangeUsernameServiceClient.GetLatestUsernameChangeDateAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCJanusLoginWithPasswordResponse> LoginAsync(SCJanusLoginWithPasswordRequest request)
    {
        return LoginServiceClient.LoginWithPasswordAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCJanusRegisterWithUsernamePasswordResponse> RegisterAsync(SCJanusRegisterWithUsernamePasswordRequest request)
    {
        return RegistrationServiceClient.RegisterWithUsernamePasswordAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCJanusSendODLVCodeResponse> SendEmail2FA(SCJanusSendODLVCodeRequest request)
    {
        return LoginServiceClient.SendODLVCodeAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCJanusVerifyODLVResponse> LoginEmail2FA(SCJanusVerifyODLVRequest request)
    {
        return LoginServiceClient.VerifyODLVAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCCofConfigTargetingResponse> CofAsync(SCCofConfigTargetingRequest request)
    {
        return CircumstancesClient.targetingQueryAsync(request, new Metadata { { "accept-language", "en_US" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCSuggestUsernamePbSuggestUsernameResponse> SuggestUsernameAsync(SCSuggestUsernamePbSuggestUsernameRequest request)
    {
        return SuggestUsernameServiceClient.SuggestUsernameAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
}