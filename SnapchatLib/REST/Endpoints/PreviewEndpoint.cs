using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Ranking.Serving.Jaguar;

namespace SnapchatLib.REST.Endpoints;

public interface IPreviewEndpoint
{
    Task<string> ViewPublicStoryByID(string story_id, int screenshotcount);
    Task<StoryJson> GetPublicStories(string viewuser);
    Task<string> ViewPublicStory(string viewuser, int screenshotcount);
    Task<SCSSMStoriesBatchResponse> GetStories(string username);
}

internal class PreviewEndpoint : EndpointAccessor, IPreviewEndpoint
{
    internal static readonly EndpointInfo UpdateStoriesEndpointInfo = new() { Url = "/bq/update_stories" };
    internal static readonly EndpointInfo PreviewEndpointInfo = new() { Url = "/bq/preview", Requirements = EndpointRequirements.Username };
    internal static readonly EndpointInfo UpdateStoriesV2EndpointInfo = new() { Url = "/bq/update_stories_v2" };

    internal static readonly EndpointInfo EndpointInfo = new()
    {
        Url = "/df-mixer-prod/soma/batch_stories",
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapAccessToken
    };

    public PreviewEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ViewPublicStoryByID(string story_id, int screenshotcount)
    {
        var serileme = new Dictionary<string, object>
        {
            {"id", story_id},
            {"is_friend", "false"},
            {"screenshot_count", screenshotcount.ToString()},
            {"is_public_story", "true"},
            {"is_friend_view_of_public_story", "false"},
            {"screen_recorded", "false"},
            {"saved", "false"},
            {"is_subscribed", "false"},
            {"timestamp", m_Utilities.UtcTimestamp()},
            {"is_offical_story", "false"}
        };

        var parameters = new Dictionary<string, string> { { "friend_stories", "[" + m_Utilities.JsonSerializeObject(serileme) + "]" } };
        var response = await Send(UpdateStoriesEndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> GetPublicStoriesResponse(string viewuser)
    {
        var parameters = new Dictionary<string, string> { { "previewed_username", viewuser } };
        var response = await Send(PreviewEndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<StoryJson> GetPublicStories(string viewuser)
    {
        var data = await GetPublicStoriesResponse(viewuser);
        return m_Utilities.JsonDeserializeObject<StoryJson>(data);
    }

    public async Task<SCSSMStoriesBatchResponse> GetStories(string username)
    {
        m_Logger.Debug("Starting GetAccessToken configuration");
        var sCSSMStoryLookupRequestItems = new List<SCSSMStoryLookupRequestItem>
        {
            new SCSSMStoryLookupRequestItem
            {
                CompositeStoryId = new SnapProto.Ranking.Core.SCCORECompositeStoryId
                {
                    Corpus = SnapProto.Ranking.Core.SCCORECompositeStoryId.Types.SCFEEDStoryCorpus_Corpus.FriendStory,
                    IdP = await SnapchatClient.FindUserFromCache(username)
                },
                DeltaFetchInfo = new SCSSMStoryLookupRequestItem.Types.SCSSMStoryLookupRequestItem_DeltaFetchInfo { SequenceBegin = 2 }

            }
        };
        var _SCSSMStoriesRequest = new SCSSMStoriesRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RequestTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ClientInfo = new SCSSMClientInfo
            {
                AppInfo = new SCSSMAppInfo
                {
                    AppVersion = Config.SnapchatVersion.ToString().Replace("V", "").Replace("_", "."),
                    OsType = SCSSMAppInfo.Types.SCSCOREOsType_Type.OsAndroid
                },
                IsNewUser = true,
                CameosFeatureRestricted = SCSSMClientInfo.Types.SCSSMCameosFeatureStatus_Enum.Restricted
            },
            Origin = SCSSMStoriesRequest.Types.SCSSMStoriesRequest_Origin.OriginSnapchatApp,
            DeltaFetchInfo = new SCSSMStoriesRequest.Types.SCSSMStoriesRequest_DeltaFetchInfo
            {
                DeltaTokenArray = { sCSSMStoryLookupRequestItems },
            },
            FeedTypesArray = { new List<int> { 5 } },
            RequestSource = SCSSMStoriesRequest.Types.SCSSMStoriesRequest_RequestSource_Enum.DiscoverPage
        };
        // send data via HTTP
        using var _ByteArrayContent = new ByteArrayContent(_SCSSMStoriesRequest.ToByteArray());
        _ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
        var response = await Send(EndpointInfo, _ByteArrayContent);
        var result = SCSSMStoriesBatchResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

        return result;
    }

    public async Task<string> ViewPublicStory(string viewuser, int screenshotcount)
    {
        var data = await GetPublicStoriesResponse(viewuser);
        var storyData = m_Utilities.JsonDeserializeObject<StoryJson>(data);
        foreach (var story in storyData.public_stories.stories)
        {
            var serileme = new Dictionary<string, object>
            {
                {"flushable_story_id", story.flushable_story_id},
                {"is_friend", "false"},
                {"screenshot_count", screenshotcount.ToString()},
                {"is_public_story", "true"},
                {"timestamp", m_Utilities.UtcTimestamp()},
                {"is_offical_story", "false"}
            };

            var parameters = new Dictionary<string, string> { { "encrypted_story_view_records", "[" + m_Utilities.JsonSerializeObject(serileme) + "]" } };
            await Send(UpdateStoriesV2EndpointInfo, parameters);
        }

        return data;
    }
}