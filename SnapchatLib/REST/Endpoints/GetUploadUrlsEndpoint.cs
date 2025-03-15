using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Content.V2;

namespace SnapchatLib.REST.Endpoints
{
    internal interface IGetUploadUrlsEndpoint
    {
        Task<SCBoltv2UploadLocation> GetUploadUrls();
    }

    internal class GetUploadUrlsEndpoint : EndpointAccessor, IGetUploadUrlsEndpoint
    {
        private readonly ISnapchatGrpcClient m_SnapchatGrpcClient;

        public GetUploadUrlsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
            m_SnapchatGrpcClient = grpcClient;
        }

        public async Task<SCBoltv2UploadLocation> GetUploadUrls()
        {
            m_Logger.Debug("Creating request");

            if (string.IsNullOrEmpty(Config.AccountCountryCode))
            {
                throw new Exception("Account Country Code Required");
            };

            var locationsRequest = new SCBoltv2GetUploadLocationsRequest
            {
                UserContext = new SCBoltv2UserContext
                {
                    UserCountry = Config.AccountCountryCode
                },
                ContentReferenceResultOption = SCBoltv2GetUploadLocationsRequest.Types.SCBoltv2ContentReferenceResultOption.ContentUrlandObject
            };

            m_Logger.Debug("Sending request with getUploadLocationsAsync");
            var reply = await SnapchatGrpcClient.GetUploadLocationsAsync(locationsRequest);
            m_Logger.Debug($"Reply: {reply}");
            m_Logger.Debug($"Length of reply array: {reply.UploadLocationsArray.Count}");
            //  Console.WriteLine(reply.UploadLocationsArray[0].ContentReference.ContentURL);
            return reply.UploadLocationsArray.PickRandom();
        }
    }
}