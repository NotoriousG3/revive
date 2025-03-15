using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Messaging;
using System.Linq;

namespace SnapchatLib.REST.Endpoints;

internal interface IConversationsEndpoint
{
    Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend);
}

internal class ConversationsEndpoint : EndpointAccessor, IConversationsEndpoint
{
    public ConversationsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<HashSet<ConversationInfo>> CreateConversation(HashSet<string> friend)
    {
        var conversationInfo = new HashSet<ConversationInfo>();
        var conversationId = m_Utilities.NewGuid().ToString();
        var deltaSyncRequests = new HashSet<DeltaSyncRequest>();

        // Create delta sync requests for each friend
        foreach (var friendUsername in friend)
        {
            var friendUserId = SnapchatClient.FindUserFromFriendsListCache(friendUsername);
            deltaSyncRequests.Add(new DeltaSyncRequest
            {
                StartingVersion = 0,
                ConversationId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(conversationId)) },
                OtherParticipantUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(friendUserId)) },
                SendingUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)) },
            });
        }

        // Send batch delta sync request for the initial conversation metadata

        var batchDeltaSyncRequest = new BatchDeltaSyncRequest
        {
            Requests = { deltaSyncRequests }
        };
        var batchDeltaSyncResponse = await SnapchatGrpcClient.BatchDeltaSyncAsync(batchDeltaSyncRequest);

        // Create delta sync requests for any conversations that require updating
        var deltaSyncRequestsToUpdate = new HashSet<DeltaSyncRequest>();
        foreach (var response in batchDeltaSyncResponse.Responses)
        {
            if (response.SuccessResponse.CurrentVersion != response.SuccessResponse.ConversationMetadata.Version)
            {
                var participants = response.SuccessResponse.ConversationMetadata.Participants
                    .Where(p => p.UserId.Id != ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)))
                    .ToList();
                var friendId = participants.FirstOrDefault()?.UserId;
                deltaSyncRequestsToUpdate.Add(new DeltaSyncRequest
                {
                    StartingVersion = response.SuccessResponse.ConversationMetadata.Version,
                    ConversationId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(conversationId)) },
                    OtherParticipantUserId = friendId,
                    SendingUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)) },
                });
            }
            else
            {
                conversationInfo.Add(new ConversationInfo
                {
                    ConversationId = response.SuccessResponse.ConversationMetadata.ConversationId,
                    ConversationVersion = response.SuccessResponse.ConversationMetadata.Version
                });
            }
        }

        // Send batch delta sync request for the updated conversation metadata

        if (deltaSyncRequestsToUpdate.Count != 0)
        {
            var batchDeltaSyncRequestToUpdate = new BatchDeltaSyncRequest
            {
                Requests = { deltaSyncRequestsToUpdate }
            };
            var batchDeltaSyncResponseToUpdate = await SnapchatGrpcClient.BatchDeltaSyncAsync(batchDeltaSyncRequestToUpdate);
            foreach (var response in batchDeltaSyncResponseToUpdate.Responses)
            {
                conversationInfo.Add(new ConversationInfo
                {
                    ConversationId = response.SuccessResponse.ConversationMetadata.ConversationId,
                    ConversationVersion = response.SuccessResponse.ConversationMetadata.Version
                });
            }
        }
        return conversationInfo;
    }

    public async Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend)
    {
        var conversations = new HashSet<ConversationInfo>();
        var conversationInfo = await CreateConversation(friend);

        foreach (var convo in conversationInfo)
        {
            if (convo.ConversationId != null)
            {
                conversations.Add(convo);
            }
        }
        return conversations;
    }
}