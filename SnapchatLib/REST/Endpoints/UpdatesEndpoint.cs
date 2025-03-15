using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IUpdatesEndpoint
{
    Task<string> GetFidUpdates(string out_beta);
}

internal class UpdatesEndpoint : EndpointAccessor, IUpdatesEndpoint
{
    public static readonly EndpointInfo EndpointInfo = new () { Url = "/fid/updates", BaseEndpoint = RequestConfigurator.MediaApiBaseEndpoint, Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

    public UpdatesEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> GetFidUpdates(string out_beta)
    {
        var step1 = new Dictionary<string, object>
        {
            {"out_beta", out_beta}
        };
        
        var parameters = new Dictionary<string, string>
        {
            {"json", m_Utilities.JsonSerializeObject(step1)},
            {"snapchat_user_id", Config.user_id}
        };
        var post = await Send(EndpointInfo, parameters);
        return await post.Content.ReadAsStringAsync();
    }
}