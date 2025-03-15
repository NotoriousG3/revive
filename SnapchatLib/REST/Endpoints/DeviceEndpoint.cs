using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

internal interface IDeviceEndpoint
{
    Task<DeviceToken> GetDeviceToken();
}

internal class DeviceEndpoint : EndpointAccessor, IDeviceEndpoint
{
    internal static readonly EndpointInfo DeviceIdEndpointInfo = new() { Url = "/loq/device_id", Requirements = EndpointRequirements.RequestToken | EndpointRequirements.UseTempRequestToken };

    public DeviceEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<DeviceToken> GetDeviceToken()
    {
        var response = await Send(DeviceIdEndpointInfo, new Dictionary<string, string>());
        return m_Utilities.JsonDeserializeObject<DeviceToken>(await response.Content.ReadAsStringAsync());
    }
}