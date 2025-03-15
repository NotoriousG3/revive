using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Messaging;
using System;

namespace SnapchatLib.REST.Endpoints
{
    internal interface IMessagingEndpoint
    {
        Task SendMention(string friend, HashSet<string> users);
        Task SendLink(string link, HashSet<string> users);
        Task SendMessage(string message, HashSet<string> users);
    }

    internal class MessagingEndpoint : EndpointAccessor, IMessagingEndpoint
    {

        private readonly ISnapchatGrpcClient m_SnapchatGrpcClient;
        public MessagingEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
            m_SnapchatGrpcClient = grpcClient;
        }

        private CreateContentMessageRequest CreateContentMessageRequest(List<DeliveryDestination> destinations, SCMessagingContents contents, ContentEnvelope.Types.ContentType contentType)
        {
            return new CreateContentMessageRequest
            {
                SenderId = new UUID
                {
                    Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                },
                ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999),
                Destinations = { destinations },
                Contents = new ContentEnvelope
                {
                    ContentType = contentType,
                    Contents = contents.ToByteString(),
                    SavePolicy = ContentEnvelope.Types.SavePolicy.Lifetime
                }
            };
        }

        public async Task SendMention(string username_or_user_id, HashSet<string> users)
        {
            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var friendUserId = username_or_user_id;

            if (!Guid.TryParse(username_or_user_id, out _))
                friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);

            var contents = new SCMessagingContents
            {
                Share = new SCMessagingShare { User = new SCMessagingUserShare { UserId = new SCMessagingUUID { IdP = ByteString.CopyFrom(GuidUtils.ToBytes(friendUserId)) } } },
            };

            var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentEnvelope.Types.ContentType.Share);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
        }

        public async Task SendLink(string link, HashSet<string> users)
        {
            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var sCMessagingTextAttribute = new SCMessagingTextAttribute();

            if (Config.StealthMode) //if below is patched new method http://m.vk.com/away.php?to=https://onlyfans.com
            {
                sCMessagingTextAttribute.URLAttribute = new SCMessagingTextUrlAttribute { URL = "https://l.wl.co/l?u=" + link + "?" + m_Utilities.RandomString(new Random().Next(1, 25)) };
                var length = link.Length;
                sCMessagingTextAttribute.Range = new SCMessagingRange { Length = (uint)length };

                var contents = new SCMessagingContents
                {
                    Text = new SCMessagingText { Text = "https://pornhub.com", AttributesArray = { sCMessagingTextAttribute } },
                };

                var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentEnvelope.Types.ContentType.Chat);

                await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
            }
            else
            {
                sCMessagingTextAttribute.URLAttribute = new SCMessagingTextUrlAttribute { URL = link };
                var length = link.Length;
                sCMessagingTextAttribute.Range = new SCMessagingRange { Length = (uint)length };

                var contents = new SCMessagingContents
                {
                    Text = new SCMessagingText { Text = link, AttributesArray = { sCMessagingTextAttribute } },
                };

                var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentEnvelope.Types.ContentType.Chat);

                await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
                sCMessagingTextAttribute.URLAttribute = new SCMessagingTextUrlAttribute { URL = link };
            }

        }

        public async Task SendMessage(string message, HashSet<string> users)
        {
            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var contents = new SCMessagingContents
            {
                Text = new SCMessagingText { Text = message },
            };

            var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentEnvelope.Types.ContentType.Chat);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
        }
    }
}
