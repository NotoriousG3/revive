using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapchatLib.Models;
using SnapProto.Snapchat.Core;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Messaging;
using static SnapchatLib.REST.Models.SyncResponse;

namespace SnapchatLib.REST.Endpoints;

public interface IFriendEndpoint
{
    Task<ami_friends> SyncFriends();
    Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token);
    Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id);
    Task<SCFriendingFriendsActionResponse> AddBySearch(string username);
    Task<SCFriendingFriendsActionResponse> SubscribeFromSearch(string username);
    Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string user_id);
    Task<SCFriendingFriendsActionResponse> BlockFriend(string username_or_user_id);
    Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string user_id, string newname);
    Task<FriendRequestJson> ChangeYourDisplayName(string newname);
    Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id);
    Task<SCFriendingFriendsActionResponse> Subscribe(string username);
    Task<SCFriendingFriendsActionResponse> UnBlockFriend(string username_or_user_id);

}
internal class SyncClass
{
    public string friends_sync_token { get; set; }
    public string added_friends_sync_token { get; set; }
}


internal class FriendEndpoint : EndpointAccessor, IFriendEndpoint
{
    internal static readonly EndpointInfo FriendSyncEndpointInfo = new() { Url = "/ami/friends", Requirements = EndpointRequirements.Username | EndpointRequirements.XSnapchatUUID | EndpointRequirements.RequestToken | EndpointRequirements.XSnapAccessToken, BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint };

    internal static readonly EndpointInfo FriendEndpointInfo = new() { Url = "/bq/friend", Requirements = EndpointRequirements.Username | EndpointRequirements.XSnapchatUUID | EndpointRequirements.RequestToken | EndpointRequirements.XSnapAccessToken, BaseEndpoint = RequestConfigurator.MediaApiBaseEndpoint };
    public FriendEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<FriendRequestJson> ChangeYourDisplayName(string newname)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "display"},
            {"display", newname},
            {"friend_id", Config.user_id},
            {"snapchat_user_id", Config.user_id},
        };

        var response = await Send(FriendEndpointInfo, parameters);
        return JsonSerializer.Deserialize<FriendRequestJson>(await response.Content.ReadAsStringAsync());
    }

    public async Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string username_or_user_id, string newname)
    {

        var friendUserId = username_or_user_id;

        if (!Guid.TryParse(username_or_user_id, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);


        List<SCFriendingFriendDisplayNameParam> _param = new List<SCFriendingFriendDisplayNameParam>
        {
            new SCFriendingFriendDisplayNameParam(new SCFriendingFriendDisplayNameParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(friendUserId), LowBits = GuidUtils.GetLowBits(friendUserId) }, DisplayName = newname })
        };
        var _SCFriendingFriendsDisplayNameChangeRequest = new SCFriendingFriendsDisplayNameChangeRequest
        {
            ParamsArray = { _param }
        };
        return await SnapchatGrpcClient.ChangeDisplayNameForFriendAsync(_SCFriendingFriendsDisplayNameChangeRequest);
    }

    private async Task<SCFriendingFriendsActionResponse> AddFriendGrpcInternalUserID(string user_id, string page, SCFriendingFriendAddParam.Types.SCFriendingAddSource AddSource)
    {

        List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>
        {
            new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(user_id), LowBits = GuidUtils.GetLowBits(user_id) } })
        };
        var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
        {
            Page = page,
            ParamsArray = { _param }
        };
        var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);

        if (SnapchatClient.mcs_cof_ids_bin.Count != 0)
        {
            var friendsync = await SnapchatGrpcClient.SyncConversationsAsync(new SyncConversationsRequest
            {
                SendingUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)) },
                SyncToken = ByteString.CopyFromUtf8("UseV3")
            });

            foreach (var convo in friendsync.Conversations)
            {
                foreach (var friendz in convo.Participants)
                {
                    if (friendz.Id == ByteString.CopyFrom(GuidUtils.ToBytes(user_id)))
                    {
                        await SnapchatGrpcClient.DeltaSyncAsync(new DeltaSyncRequest
                        {
                            StartingVersion = 1,
                            ConversationId = convo.ConversationInfo.ConversationId,
                        });
                    }
                }
            }
        }
        return resp;
    }

    private async Task<SCFriendingFriendsActionResponse> AddFriendGrpcInternalNonUserID(string username, string page, SCFriendingFriendAddParam.Types.SCFriendingAddSource AddSource)
    {
        using (var _c = new HttpClient(new HttpClientHandler { Proxy = SnapchatClient.SnapchatConfig.Proxy }))
        {
            _c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            var parser = await _c.GetAsync($"https://www.snapchat.com/add/{username}");
            if (parser.IsSuccessStatusCode)
            {
                var content = await parser.Content.ReadAsStringAsync();
                if (content.Contains("https://images.bitmoji.com/3d/avatar/") || content.Contains("https://cf-st.sc-cdn.net/aps/bolt/"))
                {
                    var friendUserId = await SnapchatClient.FindUserFromCache(username);
                    List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>
                    {
                        new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(friendUserId), LowBits = GuidUtils.GetLowBits(friendUserId) } })
                    };
                    var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
                    {
                        Page = page,
                        ParamsArray = { _param }
                    };
                    var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);

                    if (SnapchatClient.mcs_cof_ids_bin.Count != 0)
                    {
                        var friendsync = await SnapchatGrpcClient.SyncConversationsAsync(new SyncConversationsRequest
                        {
                            SendingUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)) },
                            SyncToken = ByteString.CopyFromUtf8("UseV3")
                        });

                        foreach (var convo in friendsync.Conversations)
                        {
                            foreach (var friendz in convo.Participants)
                            {
                                if (friendz.Id == ByteString.CopyFrom(GuidUtils.ToBytes(friendUserId)))
                                {
                                    await SnapchatGrpcClient.DeltaSyncAsync(new DeltaSyncRequest
                                    {
                                        StartingVersion = 1,
                                        ConversationId = convo.ConversationInfo.ConversationId,
                                    });
                                }
                            }
                        }
                    }
                    return resp;
                }
            }
        }
        throw new Exception("User Dosen't Have Bitmoji");
    }

    public Task<SCFriendingFriendsActionResponse> AddBySearch(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "search", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByUsername);
    }

    public Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string user_id)
    {
        return AddFriendGrpcInternalUserID(user_id, "profile", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedBySuggested);
    }
    public Task<SCFriendingFriendsActionResponse> Subscribe(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "subscription_user_story_on_discover_feed", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedFromPublicProfile);
    }

    public Task<SCFriendingFriendsActionResponse> SubscribeFromSearch(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "SCFriendingAddSource_AddedBySubscription", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedFromPublicProfile);
    }

    public Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id)
    {
        return AddFriendGrpcInternalUserID(user_id, "added_me_notification", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByAddedMeBack);
    }

    public async Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id)
    {

        List<SCFriendingFriendRemoveParam> _param = new List<SCFriendingFriendRemoveParam>
        {
            new SCFriendingFriendRemoveParam(new SCFriendingFriendRemoveParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(user_id), LowBits = GuidUtils.GetLowBits(user_id) } })
        };
        var _SCFriendingFriendsRemoveRequest = new SCFriendingFriendsRemoveRequest
        {
            ParamsArray = { _param }
        };
        var reply = await SnapchatGrpcClient.RemoveFriendAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }

    public async Task<SCFriendingFriendsActionResponse> BlockFriend(string username_or_user_id)
    {
        var friendUserId = username_or_user_id;

        if (!Guid.TryParse(username_or_user_id, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);

        List<SCFriendingFriendBlockParam> _param = new List<SCFriendingFriendBlockParam>
        {
            new SCFriendingFriendBlockParam(new SCFriendingFriendBlockParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(username_or_user_id), LowBits = GuidUtils.GetLowBits(username_or_user_id) } })
        };
        var _SCFriendingFriendsRemoveRequest = new SCFriendingFriendsBlockRequest
        {
            ParamsArray = { _param }
        };
        var reply = await SnapchatGrpcClient.BlockFriendsAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }

    public async Task<SCFriendingFriendsActionResponse> UnBlockFriend(string username_or_user_id)
    {
        var friendUserId = username_or_user_id;

        if (!Guid.TryParse(username_or_user_id, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);

        List<SCFriendingFriendUnblockParam> _param = new List<SCFriendingFriendUnblockParam>
        {
            new SCFriendingFriendUnblockParam(new SCFriendingFriendUnblockParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(username_or_user_id), LowBits = GuidUtils.GetLowBits(username_or_user_id) } })
        };
        var _SCFriendingFriendsRemoveRequest = new SCFriendingFriendsUnblockRequest
        {
            ParamsArray = { _param }
        };
        var reply = await SnapchatGrpcClient.UnblockFriendsAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }


    public async Task<ami_friends> SyncFriends()
    {
        var sync = new SyncClass { added_friends_sync_token = "", friends_sync_token = "" };
        var parameters = new Dictionary<string, string> { { "action", m_Utilities.JsonSerializeObject(sync) } };
        var response = await Send(FriendSyncEndpointInfo, parameters);
        var result = m_Utilities.JsonDeserializeObject<ami_friends>(await response.Content.ReadAsStringAsync());
        return result;
    }

    public async Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token)
    {
        var sync = new SyncClass { added_friends_sync_token = added_friends_sync_token, friends_sync_token = friends_sync_token };
        var parameters = new Dictionary<string, string> { { "action", m_Utilities.JsonSerializeObject(sync) } };
        var response = await Send(FriendSyncEndpointInfo, parameters);
        var result = m_Utilities.JsonDeserializeObject<ami_friends>(await response.Content.ReadAsStringAsync());
        return result;
    }
}