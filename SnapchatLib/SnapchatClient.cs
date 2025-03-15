using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.Models;
using SnapchatLib.REST;
using SnapchatLib.REST.Models;
using SnapProto.Com.Snapchat.Proto.Security;
using SnapProto.Ranking.Serving.Jaguar;
using SnapProto.Snapchat.Activation.Api;
using SnapProto.Snapchat.Content.V2;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snapchat.Search;
using static SnapchatLib.REST.Models.SyncResponse;

namespace SnapchatLib;

public class SnapchatClient : IDisposable
{
    internal SnapchatClient() { }

    internal IClientLogger m_Logger;

    internal SnapchatClient(SnapchatConfig config, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator)
    {
        SnapchatConfig = new SnapchatLockedConfig(config);
        HttpClient = httpClient;
        GrpcClient = grpcClient;
        m_Logger = logger;
        m_Utilities = utilities;
        m_RequestConfigurator = configurator;

    }

    public SnapchatClient(SnapchatConfig config)
    {
        SnapchatConfig = new SnapchatLockedConfig(config);
        m_Logger = new ClientLogger(SnapchatConfig);
        m_Utilities = config.Utilities;

        GrpcClient = new SnapchatGrpcClient(this, m_Utilities);
        m_RequestConfigurator = new RequestConfigurator(m_Logger, m_Utilities);

        HttpClient = new SnapchatHttpClient(this, GrpcClient, m_Logger, m_Utilities, m_RequestConfigurator);
        m_UserFinder = new UserFinder(HttpClient, m_Utilities);
    }



    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    // Use C# finalizer syntax for finalization code.
    // This finalizer will run only if the Dispose method
    // does not get called.
    // It gives your base class the opportunity to finalize.
    // Do not provide finalizer in types derived from this class.
    ~SnapchatClient()
    {
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(disposing: false) is optimal in terms of
        // readability and maintainability.
        Dispose();
    }

    private void ClientSetup()
    {
        m_Logger.Debug("//==Debug Info==\\");
        m_Logger.Debug("APIKEY: " + Convert.ToBase64String(Encoding.UTF8.GetBytes(SnapchatConfig.ApiKey)));
        m_Logger.Debug("Username: " + SnapchatConfig.Username);
        m_Logger.Debug("AuthToken: " + SnapchatConfig.AuthToken);
        m_Logger.Debug("[FLAG] BandwithSaver: " + SnapchatConfig.BandwithSaver);
        m_Logger.Debug("[FLAG] OS: " + SnapchatConfig.OS);
        m_Logger.Debug("Payload: " + payload);
        m_Logger.Debug("Mac: " + mac);
        m_Logger.Debug("IV: " + IV);
        m_Logger.Debug("KEY: " + KEY);
        m_Logger.Debug("User-Id: " + SnapchatConfig.user_id);
        m_Logger.Debug("Environment Version: " + Environment.Version);
    }
    #region Fields
    internal string sessionId { get; set; } = Guid.NewGuid().ToString();
    internal int sessionQueryId { get; set; } = 0;
    internal long launchTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    internal string IV { get; set; }
    internal string KEY { get; set; }
    public string payload { get; set; }
    public string mac { get; set; }
    internal int s_a { get; set; } = 0;
    internal int s_r { get; set; } = 0;
    internal string odlvToken { get; set; }
    internal bool HasLoggedIn { get; set; }
    internal List<int> mcs_cof_ids_bin = new List<int>();

    internal List<int> cofConfigData_Android = new List<int>();
    internal string loginFlowSessionId { get; set; }
    internal string ConfigResultsEtag { get; set; }
    internal string authenticationSessionId { get; set; }
    internal virtual ISnapchatHttpClient HttpClient { get; }
    internal virtual ISnapchatGrpcClient GrpcClient { get; }
    public virtual SnapchatLockedConfig SnapchatConfig { get; }

    private UserFinder m_UserFinder;
    internal readonly IUtilities m_Utilities;
    private readonly IRequestConfigurator m_RequestConfigurator;

    #endregion

    public async Task InitClient()
    {
        try
        {
            ClientSetup();

            if (string.IsNullOrEmpty(SnapchatConfig.Access_Token))
            {
                await GetAccessTokens();
            }

            if (SnapchatConfig.AuthToken == null)
            {
                throw new AuthTokenNotSetException();
            }

            if (string.IsNullOrEmpty(SnapchatConfig.user_id))
            {
                throw new Exception("user_id cannot be null");
            }
            await m_UserFinder.CacheFriendsList();
            GrpcClient.SetupServiceClients();
        }
        catch (Exception ex)
        {
            if (SnapchatConfig.Debug)
            {
                throw new Exception(ex.ToString());
            }

            throw new Exception("Failed to InitClient", ex);
        }
    }
    #region Settings / Profile

    public virtual async Task ChangeUsername(string username, string password)
    {
        await HttpClient.ChangeUsername.ChangeUsername(username, password);
    }

    public virtual async Task GetLatestUsername()
    {
        await HttpClient.ChangeUsername.GetLatestUsernameChangeDate();
    }

    [Obsolete("Please use ChangeEmail, which will call the appropriate function based on the value of SnapchatConfig.Android")]
    public virtual async Task<string> ChangeEmailIOS(string email)
    {
        return await HttpClient.ChangeEmail.ChangeEmailIOS(email);
    }

    [Obsolete("Please use ChangeEmail, which will call the appropriate function based on the value of SnapchatConfig.Android")]
    public virtual async Task<string> ChangeEmailAndroid(string email)
    {
        return await HttpClient.ChangeEmail.ChangeEmailAndroid(email);
    }

    public virtual async Task<string> ChangeEmail(string email)
    {
        return await HttpClient.ChangeEmail.ChangeEmail(email);
    }

    public virtual async Task<SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname)
    {
        return await HttpClient.RegisterV2.Register(username, password, firstname, lastname);
    }

    public virtual async Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword)
    {
        return await HttpClient.RegisterV2.RegisterWeb(firstname, lastname, username, password, email, emailpassword);
    }

    public virtual async Task<ValidationStatus> ValidateEmail(string emailpop3host, int emailpop3port, bool useSsl, bool usePuppeteer, string email, string password)
    {
        return await HttpClient.RegisterV2.ValidateEmail(emailpop3host, emailpop3port, useSsl, usePuppeteer, email, password);
    }

    public virtual async Task<ValidationStatus> ValidateEmail(string link, bool useUndetectedBrowser)
    {
        return await HttpClient.RegisterV2.ValidateEmail(link, false);
    }

    public virtual async Task<string> GetProfileInfo(string username)
    {
        return await HttpClient.SnapchatterPublicInfo.GetProfileInfo(username);
    }

    public virtual async Task<SCSuggestUsernamePbSuggestUsernameResponse> SuggestUsername(string first_name, string last_name)
    {
        return await HttpClient.SuggestUsername.SuggestUsername(first_name, last_name);
    }
    public virtual async Task<SCPBSecurityGetUrlReputationResponse.Types.SCPBSecurityUrlType> CheckUrl(string url)
    {
        return await HttpClient.CheckUrl.CheckUrl(url);
    }

    public virtual async Task<string> ChangePassword(string oldpassword, string newpassword)
    {
        return await HttpClient.ChangePassword.ChangePassword(oldpassword, newpassword);
    }

    public virtual async Task<SCJanusLoginWithPasswordResponse> Login(string username, string password)
    {
        return await HttpClient.Login.Login(username, password);
    }
    public virtual async Task<SCJanusVerifyODLVResponse> Login2FA(string twofactorcode)
    {
        return await HttpClient.Login.Login2FA(twofactorcode);
    }
    public virtual async Task<string> ReAuth(string password)
    {
        return await HttpClient.Reauth.ReAuth(password);
    }

    public virtual async Task<string> ChangePhone(string number, string countrycode)
    {
        return await HttpClient.PhoneVerify.ChangePhone(number, countrycode);
    }

    public virtual async Task<string> VerifyPhone(string code)
    {
        return await HttpClient.PhoneVerify.VerifyPhone(code);
    }

    public virtual async Task EnableRequestLocation()
    {
        await HttpClient.UpdateFeatureSettings.EnableRequestLocation();
    }

    public virtual async Task DisableRequestLocation()
    {
        await HttpClient.UpdateFeatureSettings.DisableRequestLocation();
    }

    public virtual async Task<string> ValidateEmail(string email)
    {
        return await HttpClient.ValidateEmail.ValidateEmail(email);
    }

    public virtual async Task<bool> IsValidEmail(string email)
    {
        return await HttpClient.ValidateEmail.IsValidEmail(email);
    }

    #endregion

    #region Find / Search

    public string FindUserFromFriendsListCache(string username)
    {
        return m_UserFinder.FindUserFromFriendsListCache(username);
    }

    public virtual async Task<string> FindUserFromCache(string username)
    {
        return await m_UserFinder.FindUserFromCache(username);
    }

    public virtual async Task<SCFriendingContactBookUploadResponse> FindUsersViaPhone(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.FindUsersViaPhone(number, CountryCode, randomfirstname);
    }

    public virtual async Task<SCFriendingContactBookUploadResponse> FindUsersViaEmail(string email, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.FindUsersViaEmail(email, CountryCode, randomfirstname);
    }
    public virtual async Task<List<string>> ReturnUsernameViaEmail(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.ReturnUsernameViaEmail(number, CountryCode, randomfirstname);
    }
    public virtual async Task<List<string>> ReturnUsernameViaPhone(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.ReturnUsernameViaPhone(number, CountryCode, randomfirstname);
    }
    public virtual async Task<string> GetUserID(string username)
    {
        return await HttpClient.Search.GetUserId(username);
    }
    public virtual async Task<bool> IsUserActive(string username)
    {
        return await HttpClient.Search.IsUserActive(username);
    }
    public virtual async Task<SCS2SearchResponse> FindUsersViaSearch(string username)
    {
        return await HttpClient.Search.FindUser(username);
    }
    public virtual async Task<string> UserExists(string username)
    {
        return await HttpClient.UserExists.UserExists(username);
    }

    public virtual async Task<bool> DoesUserExists(string username)
    {
        return await HttpClient.UserExists.DoesUserExists(username);
    }

    public virtual async Task<string> ReturnUserID(string username)
    {
        return await HttpClient.UserExists.ReturnUserID(username);
    }

    public virtual async Task<string> ReturnDisplayName(string username)
    {
        return await HttpClient.UserExists.ReturnDisplayName(username);
    }

    #endregion

    #region Init / Setup
    internal virtual async Task SetDeviceInfo()
    {
        m_Logger.Debug("Fetching device token");
        var deviceToken = await HttpClient.Device.GetDeviceToken();
        SnapchatConfig.dtoken1i = deviceToken.dtoken1i;
        SnapchatConfig.dtoken1v = deviceToken.dtoken1v;
        m_Logger.Debug("Device tokens set");
    }
    public virtual async Task Validate()
    {
        await HttpClient.AccessToken.Validate();
    }

    public virtual async Task GetAccessTokens()
    {
        await HttpClient.AccessToken.GetAccessTokens();
    }
    #endregion

    #region CreateAvatarData
    public async Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9)
    {
        await HttpClient.CreateAvatarData.CreateBitmoji(male, style, body, bottom, bottom_tone1, bottom_tone10, bottom_tone2, bottom_tone3, bottom_tone4, bottom_tone5, bottom_tone6, bottom_tone7, bottom_tone8, bottom_tone9, brow, clothing_type, ear, eyelash, face_proportion, footwear, footwear_tone1, footwear_tone10, footwear_tone2, footwear_tone3, footwear_tone4, footwear_tone5, footwear_tone6, footwear_tone7, footwear_tone8, footwear_tone9, hair, hair_tone, is_tucked, jaw, mouth, nose, pupil, pupil_tone, skin_tone, sock, sock_tone1, sock_tone2, sock_tone3, sock_tone4, top, top_tone1, top_tone10, top_tone2, top_tone3, top_tone4, top_tone5, top_tone6, top_tone7, top_tone8, top_tone9);
    }

    #endregion

    #region Settings

    public virtual async Task<string> ResendVerifyEmail()
    {
        return await HttpClient.Settings.ResendVerifyEmail();
    }

    public virtual async Task<string> DisableQuickAdd()
    {
        return await HttpClient.Settings.DisableQuickAdd();
    }

    public virtual async Task<string> EnableQuickAdd()
    {
        return await HttpClient.Settings.EnableQuickAdd();
    }

    public virtual async Task<string> MakeStoryPublic()
    {
        return await HttpClient.Settings.MakeStoryPublic();
    }

    public virtual async Task<string> MakeStoryFriendsOnly()
    {
        return await HttpClient.Settings.MakeStoryFriendsOnly();
    }

    public virtual async Task MakeSnapPublic()
    {
        await HttpClient.Settings.MakeSnapPublic();
    }

    public virtual async Task MakeSnapPrivate()
    {
        await HttpClient.Settings.MakeSnapPrivate();
    }
    #endregion

    #region Reporting
    public virtual async Task ReportUserRandom(string username)
    {
        await HttpClient.Reporting.ReportUserRandom(username);
    }
    public virtual async Task ReportBusinessStoryRandom(string username)
    {
        await HttpClient.Reporting.ReportBusinessStoryRandom(username);
    }

    #endregion

    #region Friend
    public virtual async Task<FriendRequestJson> ChangeYourDisplayName(string newname)
    {
        return await HttpClient.Friend.ChangeYourDisplayName(newname);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string user_id, string newname)
    {
        return await HttpClient.Friend.ChangeFriendDisplayName(user_id, newname);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddBySearch(string username)
    {
        return await HttpClient.Friend.AddBySearch(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string username_or_user_id)
    {
        return await HttpClient.Friend.AddByQuickAdd(username_or_user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> Subscribe(string username)
    {
        return await HttpClient.Friend.Subscribe(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> SubscribeFromSearch(string username)
    {
        return await HttpClient.Friend.SubscribeFromSearch(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id)
    {
        return await HttpClient.Friend.AcceptFriend(user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id)
    {
        return await HttpClient.Friend.RemoveFriend(user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> BlockFriend(string username_or_user_id)
    {
        return await HttpClient.Friend.BlockFriend(username_or_user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> UnBlockFriend(string username_or_user_id)
    {
        return await HttpClient.Friend.UnBlockFriend(username_or_user_id);
    }

    public virtual async Task<ami_friends> SyncFriends()
    {
        return await HttpClient.Friend.SyncFriends();
    }
    public virtual async Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token)
    {
        return await HttpClient.Friend.SyncFriends(added_friends_sync_token, friends_sync_token);
    }
    public virtual async Task<suggest_friend_high_availability> GetSuggestions()
    {
        return await HttpClient.SuggestFriend.GetSuggestions();
    }

    #endregion

    #region Story
    public virtual async Task PostStory(string inputfile, string swipeupurl = null, List<string> mentioned = null)
    {
        await HttpClient.PostStory.PostStory(inputfile, swipeupurl, mentioned);
    }
    public virtual async Task<string> ViewPublicStoryByID(string story_id, int screenshotcount)
    {
        return await HttpClient.Preview.ViewPublicStoryByID(story_id, screenshotcount);
    }

    public virtual async Task<SCSSMStoriesBatchResponse> GetStories(string username)
    {
        return await HttpClient.Preview.GetStories(username);
    }

    public virtual async Task<string> ViewPublicStory(string viewuser, int screenshotcount)
    {
        return await HttpClient.Preview.ViewPublicStory(viewuser, screenshotcount);
    }
    public virtual async Task ViewBuisnessStory(string username)
    {
        await HttpClient.GetBusinessProfileEndpoint.ViewStory(username);
    }
    public virtual async Task<StoryJson> GetPublicStories(string viewuser)
    {
        return await HttpClient.Preview.GetPublicStories(viewuser);
    }

    #endregion

    #region Sign
    internal virtual async Task<string> SignRequest(string t, string p)
    {
        return await HttpClient.Sign.SignRequest(t, p);
    }
    internal virtual async Task SendMetrics(string key, string value)
    {
        await HttpClient.Sign.SendMetrics(key, value);
    }
    internal virtual async Task GetDevice()
    {
        await HttpClient.Sign.GetDeviceInfo();
    }

    #endregion

    #region Messaging

    public virtual async Task SendMention(string username_or_user_id, HashSet<string> users)
    {
        await HttpClient.Messaging.SendMention(username_or_user_id, users);
    }

    public virtual async Task SendLink(string link, HashSet<string> users)
    {
        await HttpClient.Messaging.SendLink(link, users);
    }

    public virtual async Task SendMessage(string message, HashSet<string> users)
    {
        await HttpClient.Messaging.SendMessage(message, users);
    }

    #endregion

    #region DirectSnap

    public virtual async Task PostDirect(string inputfile, HashSet<string> users, string swipeupurl = null, HashSet<string> mentioned = null)
    {
        await HttpClient.DirectSnap.PostDirect(inputfile, users, swipeupurl, mentioned);
    }

    #endregion

    #region GetUploadUrls
    public virtual async Task<SCBoltv2UploadLocation> GetUploadUrls()
    {
        return await HttpClient.GetUploadUrls.GetUploadUrls();
    }

    #endregion

    #region Conversations
    public virtual async Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend)
    {
        return await HttpClient.Conversations.GetConversationID(friend);
    }
    #endregion

}