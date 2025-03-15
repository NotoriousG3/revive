using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Com.Snapchat.Auth.Proto;
using SnapProto.Com.Snapchat.Proto.Snaptoken;

namespace SnapchatLib.Exceptions
{
    public class NoAccessTokensReceivedException : Exception
    {
        public NoAccessTokensReceivedException() : base("The response contained 0 access token")
        {
        }
    }

    public class NoMatchingAccessTokenException : Exception
    {
        public NoMatchingAccessTokenException() : base("A matching access token could not be found")
        {
        }
    }

    public class EmptyAccessTokenException : Exception
    {
        public EmptyAccessTokenException() : base("An empty access token has been received")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    internal interface IAccessTokenEndpoint
    {
        Task GetAccessTokens();
        Task Validate();
    }

    internal class AccessTokenEndpoint : EndpointAccessor, IAccessTokenEndpoint
    {
        internal static readonly EndpointInfo EndpointInfo = new()
        {
            BaseEndpoint = RequestConfigurator.ApiGCPEndpoint,
            Url = "/snap_token/pb/snap_session",
            Requirements = EndpointRequirements.RequestToken | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUserId | EndpointRequirements.ParamsAsHeaders | EndpointRequirements.XSnapchatUUID | EndpointRequirements.Username
        };

        internal static readonly EndpointInfo EndpointInfo2 = new()
        {
            BaseEndpoint = RequestConfigurator.ApiGCPEndpoint,
            Url = "/scauth/validate",
            Requirements = EndpointRequirements.RequestToken | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUserId | EndpointRequirements.ParamsAsHeaders | EndpointRequirements.XSnapchatUUID | EndpointRequirements.Username
        };

        public AccessTokenEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task GetAccessTokens()
        {
            m_Logger.Debug("Starting GetAccessToken configuration");

            var _ScopesAsEnumsArray = new List<SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope>
            {
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.BusinessAccounts,
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.Blizzard,
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.ApiGateway
            };
            var _Session = new SCPBSnaptokenSnapSessionRequest
            {
                ScopesAsEnumsArray = { _ScopesAsEnumsArray },
                DeviceId = Config.dtoken1i
            };

            // send data via HTTP
            using var ByteArrayContent = new ByteArrayContent(_Session.ToByteArray());
            ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(EndpointInfo, ByteArrayContent);
            var result = SCPBSnaptokenSnapSessionResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

            m_Logger.Debug("Processing tokens from response");
            if (result.SnapAccessTokensArray.Count == 0) throw new EmptyIEnumerableException();

            var match = result.SnapAccessTokensArray.Where(i => i.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway").FirstOrDefault();
            if (match == null) throw new NoMatchingAccessTokenException();
            var match2 = result.SnapAccessTokensArray.Where(i => i.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts").FirstOrDefault();
            if (match2 == null) throw new NoMatchingAccessTokenException();
            Config.Access_Token = match.AccessToken;
            Config.BusinessAccessToken = match2.AccessToken;
        }

        public async Task Validate()
        {
            m_Logger.Debug("Starting GetAccessToken configuration");

            var _Session = new SCAuthUserSessionValidationRequest
            {
                RefreshToken = Config.refreshToken
            };
            // send data via HTTP
            using var ByteArrayContent = new ByteArrayContent(_Session.ToByteArray());
            ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(EndpointInfo2, ByteArrayContent);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Please Relogin");

        }
    }
}