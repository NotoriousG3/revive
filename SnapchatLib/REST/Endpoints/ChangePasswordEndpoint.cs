using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IChangePasswordEndpoint
{
    Task<string> ChangePassword(string oldpassword, string newpassword);
}

internal class ChangePasswordEndpoint : EndpointAccessor, IChangePasswordEndpoint
{
    public static readonly EndpointInfo EndpointInfo = new() { Url = "/scauth/change_password", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

    public ChangePasswordEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ChangePassword(string oldpassword, string newpassword)
    {
        await SnapchatClient.ReAuth(oldpassword);
        
        var parameters = new Dictionary<string, string>
        {
            {"new_password", newpassword},
            {"snapchat_user_id", Config.user_id}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }
}