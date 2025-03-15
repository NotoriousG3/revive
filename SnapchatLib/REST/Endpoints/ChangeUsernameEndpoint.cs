using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Activation.Api;

namespace SnapchatLib.REST.Endpoints
{
    public class UsernameUnavailableException : Exception
    {
        public UsernameUnavailableException(string username) : base($"The username: {username} is unavailable")
        {
        }
    }
    
    public class UsernameInvalidException : Exception
    {
        public UsernameInvalidException(string username) : base($"The username: {username} is invalid")
        {
        }
    }
    
    public class UsernameChangedRecentlyException : Exception
    {
        public UsernameChangedRecentlyException() : base($"The username has been changed recently")
        {
        }
    }

    internal interface IChangeUsernameEndpoint
    {
        Task ChangeUsername(string username, string password);
        Task GetLatestUsernameChangeDate();
    }

    internal class ChangeUsernameEndpoint: EndpointAccessor, IChangeUsernameEndpoint
    {
        internal static readonly EndpointInfo EndpointInfo = new() { Url = "/snapchat.activation.api.ChangeUsernameService/ChangeUsername", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };
        public ChangeUsernameEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }
        
        public async Task ChangeUsername(string username, string password)
        {

            await SnapchatClient.ReAuth(password);

            var request = new SCChangeUsernamePbChangeUsernameRequest
            {
                NewUsername = username,
            };
            await SnapchatGrpcClient.Sign(EndpointInfo.Url);
            var reply = await SnapchatGrpcClient.ChangeUsernameAsync(request);
            await GetLatestUsernameChangeDate();
            switch (reply.StatusCode)
            {
                case SCChangeUsernamePbChangeUsernameResponse.Types.SCChangeUsernamePbChangeUsernameResponse_Status.ErrorUsernameUnavailable:
                    throw new UsernameUnavailableException(username);
                case SCChangeUsernamePbChangeUsernameResponse.Types.SCChangeUsernamePbChangeUsernameResponse_Status.ErrorUsernameInvalid:
                    throw new UsernameInvalidException(username);
                case SCChangeUsernamePbChangeUsernameResponse.Types.SCChangeUsernamePbChangeUsernameResponse_Status.ErrorChangedRecently:
                    throw new UsernameChangedRecentlyException();
                case SCChangeUsernamePbChangeUsernameResponse.Types.SCChangeUsernamePbChangeUsernameResponse_Status.Success:
                    return;
                default:
                    throw new Exception("Unhandled exception when trying to change username");
            }
        }

        public async Task GetLatestUsernameChangeDate()
        {
            var request = new SCChangeUsernamePbGetLatestUsernameChangeDateRequest { };
            await SnapchatGrpcClient.Sign(EndpointInfo.Url);
            await SnapchatGrpcClient.GetLatestUsernameChangeDate(request);
        }
    }
}