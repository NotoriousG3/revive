using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

public interface IAllUpdatesEndpoint
{
    Task<UpdatesJson> GetUpdates();
    Task<string> GetScore();
}

internal class AllUpdatesEndpoint : EndpointAccessor, IAllUpdatesEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new() { Url = "/loq/all_updates", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

    public AllUpdatesEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<UpdatesJson> GetUpdates()
    {
        var parameters = SetUpdateParams();
        var post = await Send(EndpointInfo, parameters);
        var parsedData = m_Utilities.JsonDeserializeObject<UpdatesJson>(await post.Content.ReadAsStringAsync());
        if (parsedData == null) throw new DeserializationException(nameof(UpdatesJson));
        return parsedData;
    }

    public async Task<string> GetScore()
    {
        var parameters = SetUpdateParams();
        var post = await Send(EndpointInfo, parameters);
        var parsedData = m_Utilities.JsonDeserializeObject<UpdatesJson>(await post.Content.ReadAsStringAsync());
        if (parsedData == null) throw new DeserializationException(nameof(UpdatesJson));

        return parsedData.score.ToString();
    }

    private Dictionary<string, string> SetUpdateParams()
    {
        var result = new Dictionary<string, string>
        {
            {"checksums_dict", ""},
            {"exclude_conversations", "true"},
            {"exclude_stories", "true"},
            {"features_map", "{\"stories_delta_v2_response\":true,\"study_settings_v2\":true,\"conversations_delta_response\":true}"},
            {"friends_request", "{\"friends_sync_token\":\"8\"}"},
            {"group_delta_requests", "[]"},
            {"height", "2075"},
            {"max_video_height", "2075"},
            {"max_video_width", "1080"},
            {"screen_height_in", "5.708663"},
            {"screen_height_px", "2280"},
            {"screen_width_in", "2.6771703"},
            {"screen_width_px", "1080"},
            {"width", "1080"}
        };

        return result;
    }
}