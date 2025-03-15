using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

public interface IUserExistsEndpoint
{
    Task<string> UserExists(string username);
    Task<bool> DoesUserExists(string username);
    Task<string> ReturnUserID(string username);
    Task<string> ReturnDisplayName(string username);
}

internal class UserExistsEndpoint : EndpointAccessor, IUserExistsEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new () { Url = "/bq/user_exists", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

    public UserExistsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> UserExists(string username)
    {
        var parameters = new Dictionary<string, string>
        {
            {"include_public_story", "false"},
            {"request_username", username}
        };
        var resp = await Send(EndpointInfo, parameters);
        return await resp.Content.ReadAsStringAsync();
    }

    private async Task<UserExistsResponse> GetUserExistsObject(string username)
    {
        var response = await UserExists(username);
        return m_Utilities.JsonDeserializeObject<UserExistsResponse>(response);
    }

    public async Task<bool> DoesUserExists(string username)
    {
        var response = await GetUserExistsObject(username);
        return response.exists;
    }

    public async Task<string> ReturnUserID(string username)
    {
        var response = await GetUserExistsObject(username);
        return response.FriendJson.user_id;
    }

    public async Task<string> ReturnDisplayName(string username)
    {
        var response = await GetUserExistsObject(username);
        return response.FriendJson.display;
    }
}