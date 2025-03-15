using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

public interface ISuggestFriendEndpoint
{
    Task<suggest_friend_high_availability> GetSuggestions();
}

internal class SuggestFriendEndpoint : EndpointAccessor, ISuggestFriendEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new ()
    {
        Url = "/suggest_friend_high_availability", 
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint, 
        Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUserId | EndpointRequirements.ArgosHeader
    };

    public SuggestFriendEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task<suggest_friend_high_availability> GetSuggestions()
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "list"},
            {"impression_id", "0"},
            {"impression_time_ms", "0"},
            {"last_sync_timestamp", "0"},
            {"snapchat_user_id", Config.user_id},
            {"suggested_friend_ranking_tweak", "0"},
        };
        var response = await Send(EndpointInfo, parameters);
        return m_Utilities.JsonDeserializeObject<suggest_friend_high_availability>(await response.Content.ReadAsStringAsync());
    }
}