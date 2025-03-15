using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IUpdateFeatureSettingsEndpoint
{
    Task EnableRequestLocation();
    Task DisableRequestLocation();
}

internal class UpdateFeatureSettingsEndpoint : EndpointAccessor, IUpdateFeatureSettingsEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new () { Url = "/bq/update_feature_settings", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent };

    public UpdateFeatureSettingsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    private async Task ChangeRequestLocation(bool value)
    {
        var serileme = new Dictionary<string, object>
        {
            {"setting_name", "allow_incoming_friend_location_requests"},
            {"setting_value", value ? "true" : "false"},
            {"latest_version", "946598401"}
        };

        var parameters = new Dictionary<string, string> {{"updated_settings_v2", "[" + m_Utilities.JsonSerializeObject(serileme) + "]"}};
        await Send(EndpointInfo, parameters);
    }

    public Task EnableRequestLocation()
    {
        return ChangeRequestLocation(true);
    }

    public Task DisableRequestLocation()
    {
        return ChangeRequestLocation(false);
    }
}