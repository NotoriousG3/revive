using System.IO;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Impala.Stories;
using SnapchatLib.Exceptions;
using Google.Protobuf;
using SnapProto.Ranking.Pii.Readreceipt;
using SnapProto.Ranking.Core;
using System.Collections.Generic;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Core;

namespace SnapchatLib.REST.Endpoints;

public interface IGetBusinessProfileEndpoint
{
    Task ViewStory(string username);
}

internal class GetBusinessProfileEndpoint : EndpointAccessor, IGetBusinessProfileEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new()
    {
        Url = "/rpc/getBusinessStoryManifest",
        BaseEndpoint = RequestConfigurator.ProStoriesEndpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.UseBusinessAccessToken
    };
    internal static readonly EndpointInfo EndpointInfo2 = new()
    {
        Url = "/readreceipt-indexer/batchuploadreadreceipts",
        BaseEndpoint = RequestConfigurator.ApiAWSEast1Endpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapAccessToken | EndpointRequirements.ArgosHeader
    };
    public GetBusinessProfileEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task ViewStory(string username)
    {
        var finduser = await SnapchatClient.FindUsersViaSearch(username);
        await using var stream = new MemoryStream();

        using var streamContent = new StreamContent(stream);
        var owner_id = "";

        try
        {
            foreach (var SectionsArray in finduser.SectionsArray)
            {
                foreach (var ResultsArray in SectionsArray.ResultsArray)
                {
                    try
                    {
                        if (ResultsArray.SnapProEntity.Profile.BusinessProfile.HostAccountUsername == username || ResultsArray.SnapProEntity.Profile.BusinessProfile.HostAccountMutableUsername == username)
                        {
                            owner_id = ResultsArray.SnapProEntity.Profile.BusinessProfile.IdP;
                        }
                    }
                    catch (Exception) { }

                }
            }
        }
        catch (Exception)
        {
            throw new UsernameNotFoundException(username);
        }

        // send data via HTTP
        var IMPGetBusinessStoryManifestRequestRequest_ = new IMPGetBusinessStoryManifestRequest
        {
            BusinessId = owner_id,
        };
        using var ByteArrayContent = new ByteArrayContent(IMPGetBusinessStoryManifestRequestRequest_.ToByteArray());
        streamContent.Headers.Add("Content-Type", "application/x-protobuf");
        var response = await Send(EndpointInfo, ByteArrayContent);
        var result = IMPGetBusinessStoryManifestResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());


        List<SnapReadReceipt> readReceipt = new List<SnapReadReceipt>();

        // Instead of multiple loops, just try to get the first item of each list
        foreach (var story in result.Manifest.ElementsArray)
        {
            readReceipt.Add(new SnapReadReceipt { SnapId = story.IdP, ViewerUserId = new SCSCOREUUID { HighBits = GuidUtils.GetHighBits(Config.user_id), LowBits = GuidUtils.GetLowBits(Config.user_id) }, FriendLinkState = SnapReadReceipt.Types.FriendLinkState_Enum.Other, SnapExpirationTimeMs = story.Timestamp, ViewTimeMs = m_Utilities.UtcTimestamp(), StoryType = SnapReadReceipt.Types.StoryType_Enum.User, ReadReceiptState = new ReadReceiptState { } });
        }
        var BatchUploadReadReceiptsRequest_ = new BatchUploadReadReceiptsRequest
        {
            Metadata = new SCSCORERequestMetadata() { RequestId = Guid.NewGuid().ToString(), RequestTimestampMs = m_Utilities.UtcTimestamp(), Origin = SCSCORERequestMetadata.Types.SCSCORERequestOrigin_Enum.SnapchatApp, ClientInfo = new SCSCOREClientInfo { UserId = new SCSCOREUUID { HighBits = GuidUtils.GetHighBits(Config.user_id), LowBits = GuidUtils.GetLowBits(Config.user_id) } } },
            SnapReadReceiptsArray = { readReceipt }
        };
        using var ByteArrayContent2 = new ByteArrayContent(BatchUploadReadReceiptsRequest_.ToByteArray());
        ByteArrayContent2.Headers.Add("Content-Type", "application/x-protobuf");
        await Send(EndpointInfo2, ByteArrayContent2);
    }
}