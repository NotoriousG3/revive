using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Ranking.Storymanagement;
using SnapProto.Snapchat.Search;

namespace SnapchatLib.REST.Endpoints;

public interface ISearchEndpoint
{
    Task<string> GetUserId(string username);
    Task<SCS2SearchResponse> FindUser(string username);
    Task<bool> IsUserActive(string username);
}

internal class SearchEndpoint : EndpointAccessor, ISearchEndpoint
{
    internal static readonly EndpointInfo SearchEndpointInfo = new()
    {
        Url = "/search/search",
        BaseEndpoint = RequestConfigurator.SearchBaseEndpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapchatUUID
    };

    internal static readonly EndpointInfo PreSearchEndpointInfo = new()
    {
        Url = "/search/pretype",
        BaseEndpoint = RequestConfigurator.SearchBaseEndpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapchatUUID
    };

    internal static readonly EndpointInfo GetActiveStoriesEndpointInfo = new()
    {
        Url = "/story-management-service/get_active_story_status",
        BaseEndpoint = RequestConfigurator.SearchBaseEndpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapchatUUID
    };
    public SearchEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task<string> GetUserId(string username)
    {
        // Validate the input
        if (string.IsNullOrEmpty(Config.TimeZone))
        {
            throw new ArgumentNullException(nameof(Config.TimeZone), "TimeZone cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(Config.AccountCountryCode))
        {
            throw new ArgumentNullException(nameof(Config.AccountCountryCode), "AccountCountryCode cannot be null or empty.");
        }

        if (Config.Horoscope == SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset)
        {
            throw new ArgumentNullException(nameof(Config.Horoscope), "Horoscope cannot be unset.");
        }

        if (Config.Age == 0)
        {
            throw new ArgumentNullException(nameof(Config.Age), "Age cannot be zero.");
        }

        // If sessionQueryId is zero, make a request to the pre-search endpoint
        if (SnapchatClient.sessionQueryId == 0)
        {
            await SendPresearchRequest();
        }

        // Increment sessionQueryId by the length of the username
        SnapchatClient.sessionQueryId += username.Length;

        // Send a request to the search endpoint
        var searchRequest = new SCS2SearchRequest
        {
            QueryString = username,
            Origin = SCS2SearchRequest.Types.SCS2Origin.OriginFriendFeed,
            RequestOptions = new SCS2RequestOptions { IsSnapchatPlusEligible = true },
            SessionId = SnapchatClient.sessionId,
            SessionQueryId = SnapchatClient.sessionQueryId.ToString(),
            UserInfo = new SCS2UserInfo { Age = Config.Age, CountryCode = Config.AccountCountryCode, Location = new SCS2GeoLocation(), AstrologicalSign = Config.Horoscope, Timezone = Config.TimeZone },
            CognacClientInfo = new SCS2CognacClientInfo { UserContextToken = await SnapchatHttpClient.Get_Tokens.GetArgosTokenCached() }
        };

        using var searchRequestContent = new ByteArrayContent(searchRequest.ToByteArray());
        searchRequestContent.Headers.Add("Content-Type", "application/x-protobuf");
        var searchResponse = await Send(SearchEndpointInfo, searchRequestContent);
        var data = await searchResponse.Content.ReadAsStreamAsync();

        if (data.Length == 0)
        {
            throw new RateLimitedException();
        }

        var response = SCS2SearchResponse.Parser.ParseFrom(data);

        // Search for the user in the response
        foreach (var section in response.SectionsArray)
        {
            foreach (var result in section.ResultsArray)
            {
                try
                {
                    var user = result.User;

                    if (user?.Username == username || user?.MutableUsername == username)
                    {
                        return user.IdP;
                    }
                }
                catch (NullReferenceException)
                {
                    // The result doesn't contain a user
                }
                try
                {
                    var snapProEntity = result.SnapProEntity;

                    if (snapProEntity?.Profile?.BusinessProfile?.HostAccountUsername == username || snapProEntity?.Profile?.BusinessProfile?.HostAccountMutableUsername == username)
                    {
                        return snapProEntity.Profile.BusinessProfile.HostAccountUserId;
                    }
                }
                catch (NullReferenceException)
                {
                    // The result doesn't contain a Snap Pro entity
                }
            }
        }

        throw new UsernameNotFoundException(username);
    }


    public async Task<SCS2SearchResponse> FindUser(string username)
    {
        if (string.IsNullOrEmpty(Config.TimeZone))
        {
            throw new ArgumentNullException(nameof(Config.TimeZone));
        }

        if (string.IsNullOrEmpty(Config.AccountCountryCode))
        {
            throw new ArgumentNullException(nameof(Config.AccountCountryCode));
        }

        if (Config.Horoscope == SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset)
        {
            throw new ArgumentNullException(nameof(Config.Horoscope));
        }

        if (Config.Age == 0)
        {
            throw new ArgumentNullException(nameof(Config.Age));
        }

        // If sessionQueryId is zero, make a request to the pre-search endpoint
        if (SnapchatClient.sessionQueryId == 0)
        {
            await SendPresearchRequest();
        }


        SnapchatClient.sessionQueryId += username.Length;

        var searchRequest = new SCS2SearchRequest
        {
            Origin = SCS2SearchRequest.Types.SCS2Origin.OriginFriendFeed,
            QueryString = username,
            RequestOptions = new SCS2RequestOptions { },
            SessionId = SnapchatClient.sessionId,
            SessionQueryId = SnapchatClient.sessionQueryId.ToString(),
            UserInfo = new SCS2UserInfo { Age = Config.Age, CountryCode = Config.AccountCountryCode, Location = new SCS2GeoLocation { }, AstrologicalSign = Config.Horoscope, Timezone = Config.TimeZone },
            CognacClientInfo = new SCS2CognacClientInfo { UserContextToken = await SnapchatHttpClient.Get_Tokens.GetArgosTokenCached() }
        };

        using var content = new ByteArrayContent(searchRequest.ToByteArray());
        content.Headers.Add("Content-Type", "application/x-protobuf");

        var response = await Send(SearchEndpointInfo, content);

        return SCS2SearchResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());
    }

    private async Task SendPresearchRequest()
    {
        if (SnapchatClient.sessionQueryId == 0)
        {
            var presearchRequest = new SCS2SearchRequest
            {
                Origin = SCS2SearchRequest.Types.SCS2Origin.OriginFriendFeed,
                RequestOptions = new SCS2RequestOptions { IsSnapchatPlusEligible = true },
                SessionId = SnapchatClient.sessionId,
                SessionQueryId = SnapchatClient.sessionQueryId.ToString(),
                UserInfo = new SCS2UserInfo { Age = Config.Age, CountryCode = Config.AccountCountryCode, Location = new SCS2GeoLocation { }, AstrologicalSign = Config.Horoscope, Timezone = Config.TimeZone, CameosFeatureRestricted = true },
                CognacClientInfo = new SCS2CognacClientInfo { UserContextToken = await SnapchatHttpClient.Get_Tokens.GetArgosTokenCached() }
            };

            using var content = new ByteArrayContent(presearchRequest.ToByteArray());
            content.Headers.Add("Content-Type", "application/x-protobuf");

            var response = await Send(PreSearchEndpointInfo, content);
        }
    }

    public async Task<bool> IsUserActive(string username)
    {
        // Validate input parameters
        if (string.IsNullOrEmpty(Config.TimeZone))
        {
            throw new ArgumentNullException(nameof(Config.TimeZone));
        }

        if (string.IsNullOrEmpty(Config.AccountCountryCode))
        {
            throw new ArgumentNullException(nameof(Config.AccountCountryCode));
        }

        if (Config.Horoscope == SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset)
        {
            throw new ArgumentNullException(nameof(Config.Horoscope));
        }

        if (Config.Age == 0)
        {
            throw new ArgumentNullException(nameof(Config.Age));
        }

        try
        {
            var friendUserId = await SnapchatClient.FindUserFromCache(username);
            var sCCOREUUIDs = new List<SnapProto.Ranking.Core.SCSCOREUUID>
            {
                new SnapProto.Ranking.Core.SCSCOREUUID
                {
                    HighBits = GuidUtils.GetHighBits(friendUserId),
                    LowBits = GuidUtils.GetLowBits(friendUserId)
                }
            };

            var request = new GetActiveStoryStatusRequest
            {
                RequestMetadata = new SnapProto.Ranking.Core.SCSCORERequestMetadata
                {
                    Origin = SnapProto.Ranking.Core.SCSCORERequestMetadata.Types.SCSCORERequestOrigin_Enum.SnapchatApp,
                    ClientInfo = new SnapProto.Ranking.Core.SCSCOREClientInfo
                    {
                        UserId = new SnapProto.Ranking.Core.SCSCOREUUID
                        {
                            HighBits = GuidUtils.GetHighBits(Config.user_id),
                            LowBits = GuidUtils.GetLowBits(Config.user_id)
                        }
                    }
                },
                UserIdsArray = { sCCOREUUIDs }
            };

            // send data via HTTP
            using var byteArrayContent = new ByteArrayContent(request.ToByteArray());
            byteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(GetActiveStoriesEndpointInfo, byteArrayContent);
            var activeStatusResponse = GetActiveStoryStatusResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

            foreach (var latestPostTimestampMs in activeStatusResponse.LatestPostTimestampsMsArray)
            {
                if (latestPostTimestampMs != 0)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}