using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IReauthEndpoint
{
    Task<string> ReAuth(string password);
}

internal class ReauthEndpoint : EndpointAccessor, IReauthEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new ()
    {
        Url = "/scauth/reauth", 
        BaseEndpoint = RequestConfigurator.ApiGCPEndpoint, 
        Requirements = EndpointRequirements.Username | EndpointRequirements.SignAndroid | EndpointRequirements.RequestToken
    };

    public ReauthEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ReAuth(string password)
    {
        var parameters = new Dictionary<string, string>
        {
            {"password", password},
            {"snapchat_user_id", Config.user_id}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }
}