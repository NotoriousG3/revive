using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapProto.Com.Snapchat.Proto.Security;

namespace SnapchatLib.REST.Endpoints;

public interface ICheckUrlEndpoint
{
    Task<SCPBSecurityGetUrlReputationResponse.Types.SCPBSecurityUrlType> CheckUrl(string url);
} 

internal class CheckUrlEndpoint : EndpointAccessor, ICheckUrlEndpoint
{
    public CheckUrlEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<SCPBSecurityGetUrlReputationResponse.Types.SCPBSecurityUrlType> CheckUrl(string url)
    {
       var resp = await SnapchatGrpcClient.CheckUrl(new SCPBSecurityGetUrlReputationRequest { URL = ByteString.CopyFromUtf8(url) });

        return resp.URLType;
    }
}