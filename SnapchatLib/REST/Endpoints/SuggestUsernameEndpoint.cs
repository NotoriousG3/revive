using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Activation.Api;

namespace SnapchatLib.REST.Endpoints;

public interface ISuggestUsernameEndpoint
{
    Task<SCSuggestUsernamePbSuggestUsernameResponse> SuggestUsername(string first_name, string last_name);
}

internal class SuggestUsernameEndpoint : EndpointAccessor, ISuggestUsernameEndpoint
{
    public SuggestUsernameEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<SCSuggestUsernamePbSuggestUsernameResponse> SuggestUsername(string first_name, string last_name)
    {
        if (string.IsNullOrEmpty(Config.DeviceProfile))
        {
            await SnapchatClient.GetDevice();
            SnapchatGrpcClient.SetupServiceClients();
            if (Config.Device == null || Config.Install == null || Config.dtoken1i == null || Config.dtoken1v == null)
            {
                Config.Device = m_Utilities.NewGuid();
                Config.Install = m_Utilities.NewGuid();
                await SnapchatClient.SetDeviceInfo();
            }
        }

        var _SCFriendingFriendsRemoveRequest = new SCSuggestUsernamePbSuggestUsernameRequest
        {
            NameAndBirthdate = new SCSuggestUsernamePbNameAndBirthdate { FirstName = first_name, LastName = last_name}
        };

        var reply = await SnapchatGrpcClient.SuggestUsernameAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }
}