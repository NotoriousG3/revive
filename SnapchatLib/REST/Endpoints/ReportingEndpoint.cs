using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Impala.Stories;
using SnapProto.Snapchat.Abuse.Support;
using static SnapProto.Snapchat.Abuse.Support.SCReportReport.Types;

namespace SnapchatLib.Exceptions
{
    public class NoStoriesFoundException : Exception
    {
        public NoStoriesFoundException() : base("No stories to report")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    public enum ReportType
    {
        Nudity,
        Impersonation,
        DrugUse,
        Extremism
    }

    public enum StoryType
    {
        Public,
        Private
    }

    public enum ProfileType
    {
        User,
        Profile
    }

    public interface IReportingEndpoint
    {
        Task ReportUserRandom(string username);
        Task ReportBusinessStoryRandom(string username);
    }

    internal class ReportingEndpoint : EndpointAccessor, IReportingEndpoint
    {

        internal static readonly EndpointInfo EndpointInfo = new()
        {
            Url = "/rpc/getBusinessStoryManifest",
            BaseEndpoint = RequestConfigurator.ProStoriesEndpoint,
            Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.UseBusinessAccessToken
        };

        internal static readonly EndpointInfo EndpointInfo2 = new() { Url = "/snapchat.abuse.support.ReportService/SendReport", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };
        public ReportingEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task ReportUser(string username, string Comment, SCReportReportReason Reason)
        {
            var friendUserId = await SnapchatClient.FindUserFromCache(username);
            if (string.IsNullOrWhiteSpace(friendUserId)) throw new UsernameNotFoundException(username);

            var request = new SCReportSendReportRequest
            {
                Report = new SCReportReport
                {
                    Reported = new SCReportReported
                    {
                        UserReportData = new SCReportUserReportData
                        {
                            ReportedUserId = friendUserId,
                            IsUserContentsReport = true
                        }
                    },
                    Comment = Comment,
                    Reason = Reason
                }
            };

            await SnapchatGrpcClient.ReportUserAsync(request);
        }

        public async Task ReportUserRandom(string username)
        {
            SCReportReportReason reason;

            do
            {
                reason = EnumerableExtension.RandomEnumValue<SCReportReportReason>();
            } while (reason == SCReportReportReason.ReasonUnset);

            await ReportUser(username, "", reason);
        }

        public async Task ReportBusinessStoryRandom(string username)
        {
            SCReportReportReason reason;

            do
            {
                reason = EnumerableExtension.RandomEnumValue<SCReportReportReason>();
            } while (reason == SCReportReportReason.ReasonUnset);

            await ReportBusinessStory(username, reason);
        }

        public async Task ReportBusinessStory(string username, SCReportReportReason Reason)
        {
            try
            {
                var finduser = await SnapchatClient.FindUsersViaSearch(username);

                string owner_id = "";

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
                // send data via HTTP
                var IMPGetBusinessStoryManifestRequestRequest_ = new IMPGetBusinessStoryManifestRequest
                {
                    BusinessId = owner_id,
                };
                using var ByteArrayContent = new ByteArrayContent(IMPGetBusinessStoryManifestRequestRequest_.ToByteArray());
                ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
                var response = await Send(EndpointInfo, ByteArrayContent);
                var result = IMPGetBusinessStoryManifestResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

                // Instead of multiple loops, just try to get the first item of each list
                foreach (var story in result.Manifest.ElementsArray)
                {

                    var request = new SCReportSendReportRequest
                    {
                        Report = new SCReportReport
                        {
                            Reported = new SCReportReported
                            {
                                PublicUserStorySnapReportData = new SCReportPublicUserStorySnapReportData
                                {
                                    SnapId = story.IdP,
                                }
                            },
                            Reason = Reason
                        }
                    };

                    await SnapchatGrpcClient.ReportUserAsync(request);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}