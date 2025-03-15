using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.Exceptions
{
    public class EmailTakenException : Exception
    {
        public EmailTakenException(string message) : base(message) { }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    public interface IChangeEmailEndpoint
    {
        Task<string> ChangeEmail(string email);
        Task<string> ChangeEmailAndroid(string email);
        public Task<string> ChangeEmailIOS(string email);

    }

    internal class ChangeEmailEndpoint : EndpointAccessor, IChangeEmailEndpoint
    {
        private static readonly EndpointRequirements EndpointRequirements = EndpointRequirements.Username | EndpointRequirements.XSnapAccessToken | EndpointRequirements.RequestToken | EndpointRequirements.XSnapchatUUID;

        public static readonly EndpointInfo IosEndpointInfo = new() { Url = "/loq/change_email", Requirements = EndpointRequirements };
        public static readonly EndpointInfo AndroidEndpointInfo = new() { Url = "/loq/and/change_email", Requirements = EndpointRequirements };

        public ChangeEmailEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task<string> ChangeEmail(string email)
        {
            var result = await ChangeAddressInternal(email, Config.OS == OS.android ? AndroidEndpointInfo : IosEndpointInfo);
            return result.message;
        }

        public async Task<string> ChangeEmailAndroid(string email)
        {
            var result = await ChangeAddressInternal(email, AndroidEndpointInfo);
            return result.message;
        }

        public async Task<string> ChangeEmailIOS(string email)
        {
            var result = await ChangeAddressInternal(email, IosEndpointInfo);
            return result.message;
        }

        private async Task<change_email> ChangeAddressInternal(string email, EndpointInfo endpointInfo)
        {
            var parameters = new Dictionary<string, string>
            {
                {"prompted", "false"},
                {"email", email},
                {"snapchat_user_id", Config.user_id}
            };
            var post = await Send(endpointInfo, parameters);
            var content = await post.Content.ReadAsStringAsync();
            var data = m_Utilities.JsonDeserializeObject<change_email>(content);

            if (data.logged) return data;

            if (data.message.Contains("email is already associated with a username"))
                throw new EmailTakenException(data.message);

            return data;
        }
    }
}