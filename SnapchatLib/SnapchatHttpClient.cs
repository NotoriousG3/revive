using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST;
using SnapchatLib.REST.Endpoints;

namespace SnapchatLib.Exceptions
{
    public class DeadAccountException : Exception
    {
        public DeadAccountException(string text) : base($"Response received a status for Forbidden. This seems to be a dead account. \n{text}")
        {
        }
    }
    public class UnauthorizedAuthTokenException : Exception
    {
        public UnauthorizedAuthTokenException() : base() { }

        public UnauthorizedAuthTokenException(string message) : base(message) { }
    }

    public class BannedProxyForUploadException : Exception
    {
        public BannedProxyForUploadException() : base("Banned proxy for upload")
        {
        }
    }

    public class MalformedRequestException : Exception
    {
        public MalformedRequestException() : base("Malformed Request This could be due to Signer issue or your request")
        {
        }
    }

    public class BadProxyException : Exception
    {
        public BadProxyException() : base("Bad Proxy")
        {
        }
    }

    public class ProxyTimeoutException : Exception
    {
        public ProxyTimeoutException() : base("The proxy seems to have timeout")
        {
        }
    }

    public class ProxyAuthRequiredException : Exception
    {
        public ProxyAuthRequiredException() : base("The configured proxy requires authentication")
        {
        }
    }

    public class EmailDomainBannedException : Exception
    {
        public EmailDomainBannedException() : base("Email Domain Banned")
        {
        }
    }

    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException() : base("Password Invalid")
        {
        }
    }

    public class RateLimitedException : Exception
    {
        public RateLimitedException() : base("Rate Limited. You are firing too many requests")
        {
        }
    }

    public class FailedHttpRequestException : Exception
    {
        public FailedHttpRequestException(HttpStatusCode statusCode, string info) : base($"Unhandled status code for HttpStatusCode: {statusCode}\n{info}")
        {
        }
    }
}

namespace SnapchatLib
{

    internal interface ISnapchatHttpClient
    {
        IAccessTokenEndpoint AccessToken { get; }
        IChangeEmailEndpoint ChangeEmail { get; }
        IChangePasswordEndpoint ChangePassword { get; }
        IChangeUsernameEndpoint ChangeUsername { get; }
        ICheckUrlEndpoint CheckUrl { get; }
        IConversationsEndpoint Conversations { get; }
        ICreateAvatarDataEndpoint CreateAvatarData { get; }
        IDeviceEndpoint Device { get; }
        IFindUsersEndpoint FindUsers { get; }
        IFriendEndpoint Friend { get; }
        ILoginEndpoint Login { get; }
        IPhoneVerifyEndpoint PhoneVerify { get; }
        IPostStoryEnpoint PostStory { get; }
        IPreviewEndpoint Preview { get; }
        IReauthEndpoint Reauth { get; }
        IRegisterV2Endpoint RegisterV2 { get; }
        IReportingEndpoint Reporting { get; }
        ISearchEndpoint Search { get; }
        ISettingsEndpoint Settings { get; }
        ISignEndpoint Sign { get; }
        ISnapchatterPublicInfoEndpoint SnapchatterPublicInfo { get; }
        ISuggestFriendEndpoint SuggestFriend { get; }
        ISuggestUsernameEndpoint SuggestUsername { get; }
        IUpdateFeatureSettingsEndpoint UpdateFeatureSettings { get; }
        IUserExistsEndpoint UserExists { get; }
        IValidateEmailEndpoint ValidateEmail { get; }
        IGetBusinessProfileEndpoint GetBusinessProfileEndpoint { get; }
        IGetTokens Get_Tokens { get; }
        Task<HttpResponseMessage> Send(string url, HttpRequestMessage request, bool useProxyClient = true);
        Task<HttpResponseMessage> SendPut(string url, HttpRequestMessage request, bool useProxyClient = false);
        HttpClient webClient { get; set; }
        HttpClient m_UnproxiedHttpClient { get; }

        Task DownloadImageAsync(string directoryPath, string fileName, Uri uri);

        IMessagingEndpoint Messaging { get; }
        IGetUploadUrlsEndpoint GetUploadUrls { get; }
        IDirectSnapEndpoint DirectSnap { get; }

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool useProxiedClient);
    }

    internal class SnapchatHttpClient : ISnapchatHttpClient
    {
        private readonly SnapchatClient SnapchatClient;
        private readonly SnapchatLockedConfig SnapchatConfig;

        public virtual HttpClient webClient { get; set; }
        public virtual HttpClient m_UnproxiedHttpClient { get; set; }

        private bool configuredClients;

        internal readonly IClientLogger m_Logger;
        public IAccessTokenEndpoint AccessToken { get; }
        public IChangeEmailEndpoint ChangeEmail { get; }
        public IChangePasswordEndpoint ChangePassword { get; }
        public IChangeUsernameEndpoint ChangeUsername { get; }
        public ICheckUrlEndpoint CheckUrl { get; }
        public ICreateAvatarDataEndpoint CreateAvatarData { get; }
        public IConversationsEndpoint Conversations { get; }
        public IDeviceEndpoint Device { get; }
        public IFindUsersEndpoint FindUsers { get; }
        public IFriendEndpoint Friend { get; }
        public ILoginEndpoint Login { get; }
        public IPhoneVerifyEndpoint PhoneVerify { get; }
        public IPostStoryEnpoint PostStory { get; }
        public IPreviewEndpoint Preview { get; }
        public IReauthEndpoint Reauth { get; }
        public IRegisterV2Endpoint RegisterV2 { get; }
        public IReportingEndpoint Reporting { get; }
        public ISearchEndpoint Search { get; }
        public ISettingsEndpoint Settings { get; }
        public ISignEndpoint Sign { get; }
        public ISnapchatterPublicInfoEndpoint SnapchatterPublicInfo { get; }
        public ISuggestFriendEndpoint SuggestFriend { get; }
        public ISuggestUsernameEndpoint SuggestUsername { get; }
        public IGetTokens Get_Tokens { get; }
        public IUpdateFeatureSettingsEndpoint UpdateFeatureSettings { get; }
        public IUserExistsEndpoint UserExists { get; }
        public IValidateEmailEndpoint ValidateEmail { get; }
        public IMessagingEndpoint Messaging { get; }
        public IGetUploadUrlsEndpoint GetUploadUrls { get; }
        public IDirectSnapEndpoint DirectSnap { get; }
        public IGetBusinessProfileEndpoint GetBusinessProfileEndpoint { get; }

        internal SnapchatHttpClient(SnapchatClient snapchatClient, ISnapchatGrpcClient grpcClient, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator)
        {
            SnapchatClient = snapchatClient;
            SnapchatConfig = snapchatClient.SnapchatConfig;
            m_Logger = logger;

            GetBusinessProfileEndpoint = new GetBusinessProfileEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            AccessToken = new AccessTokenEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            ChangeEmail = new ChangeEmailEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            ChangePassword = new ChangePasswordEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            ChangeUsername = new ChangeUsernameEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            CheckUrl = new CheckUrlEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            CreateAvatarData = new CreateAvatarDataEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Conversations = new ConversationsEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Device = new DeviceEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            FindUsers = new FindUsersEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Friend = new FriendEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Login = new LoginEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            PhoneVerify = new PhoneVerifyEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            PostStory = new PostStoryEnpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Preview = new PreviewEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Reauth = new ReauthEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            RegisterV2 = new RegisterV2Endpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Reporting = new ReportingEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Search = new SearchEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Settings = new SettingsEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Sign = new SignEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            SnapchatterPublicInfo = new SnapchatterPublicInfoEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            SuggestFriend = new SuggestFriendEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            SuggestUsername = new SuggestUsernameEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            UpdateFeatureSettings = new UpdateFeatureSettingsEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            UserExists = new UserExistsEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            ValidateEmail = new ValidateEmailEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Messaging = new MessagingEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            GetUploadUrls = new GetUploadUrlsEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            Get_Tokens = new GetTokensEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);
            DirectSnap = new DirectSnapEndpoint(snapchatClient, this, grpcClient, SnapchatConfig, logger, utilities, configurator);

            SetupClients();
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool useProxiedClient)
        {
            EnsureClientConfigured();

            var client = useProxiedClient ? webClient : m_UnproxiedHttpClient;
            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendPut(string url, HttpRequestMessage request, bool useProxyClient = true)
        {
            EnsureClientConfigured();

            var client = useProxyClient ? webClient : m_UnproxiedHttpClient;

            m_Logger.Debug(client == webClient ? "SendPut: using Proxied client" : "SendPut: using unproxied client");
            var response = await client.SendAsync(request);
            await RaiseForResponse(url, response);
            return response;
        }

        public async Task<HttpResponseMessage> Send(string url, HttpRequestMessage request, bool useProxyClient = true)
        {
            EnsureClientConfigured();

            var client = useProxyClient ? webClient : m_UnproxiedHttpClient;
            m_Logger.Debug(client == webClient ? "Using Proxied client" : "Using unproxied client");
            var response = await client.SendAsync(request);
            await RaiseForResponse(url, response);
            return response;
        }

        // TODO: Seems unused
        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        private HttpClient CreateHttpClient(bool useProxy)
        {
            var httpHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            var client = new HttpClient(httpHandler);
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            client.Timeout = TimeSpan.FromSeconds(SnapchatConfig.Timeout);

            // Setup default User-Agent.
            // 
            // client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgentCreator.CreateUserAgent(this.SnapchatConfig, false));

            if (SnapchatConfig.Proxy == null || !useProxy) return client;

            // Setup proxy stuff
            httpHandler.Proxy = SnapchatConfig.Proxy;
            httpHandler.UseProxy = true;

            return client;
        }

        private void EnsureClientConfigured()
        {
            if (configuredClients)
            {
                return;
            }

            if (webClient == null || m_UnproxiedHttpClient == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(SnapchatConfig.DeviceProfile))
            {
                return;
            }

            webClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgentCreator.CreateUserAgent(this.SnapchatConfig, false));
            m_UnproxiedHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgentCreator.CreateUserAgent(this.SnapchatConfig, false));

            configuredClients = true;
        }

        private void SetupClients()
        {
            if (webClient != null && m_UnproxiedHttpClient != null)
                return;

            webClient = CreateHttpClient(true);
            m_UnproxiedHttpClient = CreateHttpClient(false);
        }

        private async Task RaiseForResponse(string endpoint, HttpResponseMessage response)
        {
            m_Logger.Debug($"Endpoint: {endpoint}");
            m_Logger.Debug($"Status Code: {response.StatusCode}");

            var resp = await response.Content.ReadAsStringAsync();
            if (resp.Contains("Due to repeated attempts or other suspicious"))
            {
                throw new RateLimitedException();
            }

            if (response.IsSuccessStatusCode) return;

            // Custom messages for bad status codes
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAuthTokenException();
                case HttpStatusCode.Forbidden:
                    if (resp.Contains("FORCE_LOGOUT"))
                        throw new UnauthorizedAuthTokenException();
                    if (endpoint.Contains("cdn"))
                        throw new BannedProxyForUploadException();
                    throw new UnauthorizedAuthTokenException();
                case HttpStatusCode.BadGateway:
                    throw new Exception("Snapchat seems to be down response -> BadGateway" + "[LOG] ->" + endpoint);
                case HttpStatusCode.BadRequest:
                    if (endpoint == ChangeEmailEndpoint.IosEndpointInfo.Url || endpoint == ChangeEmailEndpoint.AndroidEndpointInfo.Url) throw new EmailDomainBannedException();
                    if (endpoint == ChangePasswordEndpoint.EndpointInfo.Url) throw new InvalidPasswordException();
                    throw new Exception(await response.Content.ReadAsStringAsync() + "[LOG] ->" + endpoint);
                case HttpStatusCode.GatewayTimeout:
                    throw new ProxyTimeoutException();
                case HttpStatusCode.InternalServerError:
                    throw new Exception(await response.Content.ReadAsStringAsync() + "[LOG] ->" + endpoint);
                case HttpStatusCode.ProxyAuthenticationRequired:
                    throw new ProxyAuthRequiredException();
                case HttpStatusCode.ServiceUnavailable:
                    throw new Exception("Snapchat seems to be down response -> ServiceUnavailable" + "[LOG] ->" + endpoint);
                case HttpStatusCode.HttpVersionNotSupported:
                    throw new Exception("HttpVersionNotSupported" + "[LOG] ->" + endpoint);
                case HttpStatusCode.TooManyRequests:
                    throw new RateLimitedException();
                default:
                    throw new FailedHttpRequestException(response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            // Create file path and ensure directory exists
            var path = Path.Combine(directoryPath, fileName);
            Directory.CreateDirectory(directoryPath);

            // Download the image and write to the file
            var imageBytes = await webClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(path, imageBytes);
        }
    }
}