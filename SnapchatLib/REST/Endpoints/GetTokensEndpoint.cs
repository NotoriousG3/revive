using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using Google.Protobuf;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using SnapProto.Snap.Security;
using System.Net;

namespace SnapchatLib.REST.Endpoints
{
    internal interface IGetTokens
    {
        Task<string> GetArgosTokenCached();
    }

    internal class GetTokensEndpoint : EndpointAccessor, IGetTokens
    {
        internal static readonly EndpointInfo EndpointInfo = new() { Url = "/snap.security.ArgosService/GetTokens", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private string _currentToken = null;
        private DateTime _refreshAt;

        public GetTokensEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        private async Task<ArgosGetTokensResponse> GetArgosTokens()
        {
            var sign = await SnapchatGrpcClient.Sign(EndpointInfo.Url);
            var req = new ArgosGetTokensRequest
            {
                AttestationToken = ByteString.CopyFrom(Convert.FromBase64String(sign.Attestation.Replace('-', '+').Replace('_', '/')))
            };

            return await SnapchatGrpcClient.GetTokensAsync(req);
        }

        private bool MustRefreshToken()
        {
            return string.IsNullOrEmpty(_currentToken) || _refreshAt < DateTime.UtcNow;
        }

        public async Task<string> GetArgosTokenCached()
        {
            if (MustRefreshToken())
            {
                var hasLock = false;

                try
                {
                    hasLock = await _lock.WaitAsync(TimeSpan.FromSeconds(15));
                    if (hasLock)
                    {
                        if (MustRefreshToken())
                        {
                            var res = await GetArgosTokens();

                            _refreshAt = DateTime.UtcNow.AddSeconds(res.TokenB.ExpirySeconds - 30);
                            _currentToken = Convert.ToBase64String(res.TokenB.Token.Span).Replace('+', '-').Replace('/', '_');

                            //Console.WriteLine("TokenA: " + Convert.ToBase64String(res.TokenA.Token.Span).Replace('+', '-').Replace('/', '_'));
                            //Console.WriteLine("TokenB: " + Convert.ToBase64String(res.TokenB.Token.Span).Replace('+', '-').Replace('/', '_'));
                        }
                    }
                }
                finally
                {
                    if (hasLock)
                    {
                        _lock.Release();
                    }
                }
            }

            return _currentToken;
        }
    }
}