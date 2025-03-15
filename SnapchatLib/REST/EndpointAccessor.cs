using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST;

internal abstract class EndpointAccessor
{
    protected ISnapchatHttpClient SnapchatHttpClient { get; }
    protected ISnapchatGrpcClient SnapchatGrpcClient { get; }
    protected SnapchatLockedConfig Config { get; }
    protected SnapchatClient SnapchatClient { get; }
    protected IClientLogger m_Logger;
    protected IUtilities m_Utilities;

    private readonly IRequestConfigurator m_Configurator;

    protected EndpointAccessor() { }

    protected EndpointAccessor(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator requestConfigurator)
    {
        SnapchatHttpClient = httpClient;
        SnapchatGrpcClient = grpcClient;
        Config = config;
        SnapchatClient = client;
        m_Logger = logger;
        m_Utilities = utilities;
        m_Configurator = requestConfigurator;
    }

    protected virtual async Task<HttpResponseMessage> Send(EndpointInfo endpointInfo, Dictionary<string, string> parameters, bool isMulti = false)
    {
        parameters ??= new Dictionary<string, string>();

        var request = await m_Configurator.Configure(endpointInfo, parameters, HttpMethod.Post, SnapchatClient, SnapchatHttpClient, isMulti);
        return await SnapchatHttpClient.Send(endpointInfo.Url, request);
    }

    protected virtual async Task<HttpResponseMessage> Send(EndpointInfo endpointInfo, HttpContent streamContent, bool isMulti = false)
    {
        var request = await m_Configurator.Configure(endpointInfo, streamContent, HttpMethod.Post, SnapchatClient, SnapchatHttpClient, isMulti);
        return await SnapchatHttpClient.Send(endpointInfo.Url, request);
    }

    protected virtual async Task<HttpResponseMessage> SendPut(EndpointInfo endpointInfo, Stream stream)
    {
        m_Logger.Debug(SnapchatClient.KEY);
        m_Logger.Debug(SnapchatClient.IV);

        using var fileStreamContent = new StreamContent(stream);

        var request = await m_Configurator.Configure(endpointInfo, fileStreamContent, HttpMethod.Put, SnapchatClient, SnapchatHttpClient);

        m_Logger.Debug($"Calling SendPut to {endpointInfo.Url}. Request Version: {request.Version}. Request Url: {request.RequestUri}");
        return await SnapchatHttpClient.SendPut(endpointInfo.Url, request, !Config.BandwithSaver);
    }
}