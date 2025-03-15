using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;

namespace SnapchatLib.Exceptions
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string username) : base($"Failed to get user id for username: {username}")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    public interface ISnapchatterPublicInfoEndpoint
    {
        Task<string> GetProfileInfo(string username);
        Task<string> GetProfileInfoDeprecated(string username);
    }

    internal class SnapchatterPublicInfoEndpoint : EndpointAccessor, ISnapchatterPublicInfoEndpoint
    {
        internal static readonly EndpointInfo EndpointInfo = new()
        {
            Url = "/loq/snapchatter_public_info",
            BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
            Requirements = EndpointRequirements.Username | EndpointRequirements.XSnapAccessToken | EndpointRequirements.RequestToken
        };

        public SnapchatterPublicInfoEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task<string> GetProfileInfo(string username)
        {
            var userid = await SnapchatClient.GetUserID(username);

            if (string.IsNullOrWhiteSpace(userid)) throw new ProfileNotFoundException(username);
            
            return await GetProfile(userid);
        }

        [Obsolete("GetProfileInfoDeprecated is deprecated, And can break anytime NO SUPPORT Given for this method")]
        public async Task<string> GetProfileInfoDeprecated(string username)
        {
            var friendUserId = await SnapchatClient.FindUserFromCache(username);
            
            if (string.IsNullOrWhiteSpace(friendUserId)) throw new ProfileNotFoundException(username);

            return await GetProfile(friendUserId);
        }

        private async Task<string> GetProfile(string userId)
        {
            var parameters = new Dictionary<string, string>
            {
                {"source", "PROFILE"},
                {"snapchat_user_id", Config.user_id},
                {"user_ids", "[\"" + userId + "\"]"}
            };
            var response = await Send(EndpointInfo, parameters);
            return await response.Content.ReadAsStringAsync();
        }
    }
}