using Org.BouncyCastle.Utilities.Collections;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using SnapchatLib;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Activation.Api;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Search;
using TaskBoard.Models;
using ValidationStatus = TaskBoard.Models.ValidationStatus;

namespace TaskBoard;

public enum SnapchatClientStatus
{
    NotInitialized,
    Initializing,
    Initialized
}

public interface ISnapchatClient : IDisposable
{
    string UserId { get; set; }
    SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod VerificationMethod { get; set; }
    SnapchatLockedConfig SnapchatConfig { get; }
    SnapchatClient SnapchatClient { get; }
    Task WaitForInitClient(CancellationToken token);
    Task<SCSuggestUsernamePbSuggestUsernameResponse?> SuggestUsername(string firstname, string lastname);
    Task<bool> Register(string username, string password, string firstname, string lastname, string email);
    Task<bool> RegisterWeb(AppSettings settings, string username, string password, string firstname, string lastname, string email, string emailPassword);
    Task<string> ChangeEmail(string email);
    Task Subscribe(string username);
    Task<SyncResponse.ami_friends?> SyncFriends();
    Task<bool> ChangeUsername(string newName, string password);
    Task<bool> RefreshFriends(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup);
    Task<bool> RelogAccounts(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup);
    Task<suggest_friend_high_availability?> GetSuggestions();
    Task ReportUserRandom(string username);
    Task ReportUserStoryRandom(string username);
    Task PostDirect(string inputFile, string? postDirectswipeUpUrl, HashSet<string> users);
    Task<SCS2SearchResponse> FindUsersViaSearch(string search);
    Task<bool> IsUserActive(string name);
    Task<SCFriendingContactBookUploadResponse> PhoneToUsername(string number, string countryCode);
    Task<SCFriendingContactBookUploadResponse> EmailToUsername(string address);
    Task<SCFriendingFriendsActionResponse> AddByUsername(string username);
    Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string username);
    Task<SCFriendingFriendsActionResponse> AcceptFriend(string username);
    Task<SCFriendingFriendsActionResponse> RemoveFriend(string username);
    Task InitClient();
    Task GetAccessTokens();
    Task Login2FA(string code);
    Task SendMessage(string message, HashSet<string> users);
    Task SendMention(string user, HashSet<string> users);
    Task SendLink(string message, HashSet<string> users);
    Task ViewBusinessPublicStory(string username);
    Task ViewPublicStory(string username);
    Task<string> ChangePhone(string phone, string country);
    Task VerifyPhone(string phone);
    Task ResendVerifyEmail();
    Task Login(SnapchatAccountManager manager, SnapchatAccountModel account);
    Task Login(string username, string password);
    Task PostStoryLegacy(string inputFile, string swipeUpUrl = null, List<string> mentioned = null);
    Task<bool> CreateCustomBitmoji(ApplicationDbContext context, int id);
    Task<bool> Validate();
    Task<ValidationStatus> ValidateEmail(string link, bool penis);
}

public class SnapchatClientWrapper : ISnapchatClient
{
    private readonly SnapchatClient _client;

    public SnapchatClientWrapper(SnapchatClient client)
    {
        _client = client;
    }

    public SnapchatClientStatus ClientStatus { get; set; }
    public SnapchatLockedConfig SnapchatConfig => _client.SnapchatConfig;
    public SnapchatClient SnapchatClient => _client;
    public SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod VerificationMethod { get; set; }

    public string UserId
    {
        get => _client.SnapchatConfig.user_id;
        set => _client.SnapchatConfig.user_id = value;
    }

    public async Task<bool> CreateCustomBitmoji(ApplicationDbContext context, int id)
    {
        try
        {
            BitmojiModel bitmoji = context.Bitmojis.Where(b => b.Id.Equals(id)).Take(1).FirstOrDefault()!;

            
            if (bitmoji != null)
            {
                var isMale = bitmoji.Gender == 2 ? false : true;
                
                await _client.CreateBitmoji(isMale, bitmoji.Style, bitmoji.Body,
                    bitmoji.Bottom, bitmoji.BottomTone1, bitmoji.BottomTone10, bitmoji.BottomTone2,
                    bitmoji.BottomTone3,
                    bitmoji.BottomTone4, bitmoji.BottomTone5, bitmoji.BottomTone6, bitmoji.BottomTone7,
                    bitmoji.BottomTone8, bitmoji.BottomTone9, bitmoji.Brow, 1, bitmoji.Ear,
                    bitmoji.Eyelash,
                    bitmoji.FaceProportion, bitmoji.Footwear, bitmoji.FootwearTone1, bitmoji.FootwearTone10,
                    bitmoji.FootwearTone2, bitmoji.FootwearTone3, bitmoji.FootwearTone4, bitmoji.FootwearTone5,
                    bitmoji.FootwearTone6,
                    bitmoji.FootwearTone7, bitmoji.FootwearTone8, bitmoji.FootwearTone9, bitmoji.Hair, bitmoji.HairTone, bitmoji.IsTucked, bitmoji.Jaw, bitmoji.Mouth, bitmoji.Nose, bitmoji.Pupil, bitmoji.PupilTone,
                    bitmoji.SkinTone,
                    bitmoji.Sock, bitmoji.SockTone1, bitmoji.SockTone2, bitmoji.SockTone3, bitmoji.SockTone4,
                    bitmoji.Top, bitmoji.TopTone1, bitmoji.TopTone10, bitmoji.TopTone2, bitmoji.TopTone3,
                    bitmoji.TopTone4, bitmoji.TopTone5,
                    bitmoji.TopTone6, bitmoji.TopTone7, bitmoji.TopTone8, bitmoji.TopTone9);
            }
            else
            {
                throw new Exception($"Bitmoji with id ${id} can't be found.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            return false;
        }
    }
    
    public async Task<string> ChangePhone(string phone, string country)
    {
        var result = await _client.ChangePhone(phone, country);
        return result;
    }

    public async Task<SCS2SearchResponse> FindUsersViaSearch(string search)
    {
        var result = await _client.FindUsersViaSearch(search);
        
        return result;
    }

    public async Task<bool> IsUserActive(string name)
    {
        return await _client.IsUserActive(name);
    }
    
    public async Task<SCFriendingContactBookUploadResponse> PhoneToUsername(string number, string countryCode)
    {
        var randomizerFirstName = RandomizerFactory.GetRandomizer(new FieldOptionsFirstName());
        var result = await _client.FindUsersViaPhone(number, countryCode, randomizerFirstName.Generate());
        
        return result;
    }
    
    public async Task<SCFriendingContactBookUploadResponse> EmailToUsername(string address)
    {
        var randomizerFirstName = RandomizerFactory.GetRandomizer(new FieldOptionsFirstName());
        var result = await _client.FindUsersViaEmail(address, "US", randomizerFirstName.Generate());

        return result;
    }

    public async Task VerifyPhone(string phone)
    {
        await _client.VerifyPhone(phone);
    }
    
    public async Task ResendVerifyEmail()
    {
        await _client.ResendVerifyEmail();
    }

    public async Task<SCSuggestUsernamePbSuggestUsernameResponse?> SuggestUsername(string firstname, string lastname)
    {
        return await _client.SuggestUsername(firstname, lastname);
    }
    
    public async Task<bool> RegisterWeb(AppSettings settings, string username, string password, string firstname, string lastname, string email, string emailPassword = "WebRegister")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(settings.KopeechkaApiKey))
            {
                throw new Exception("There is no API key for Kopeechka verification service");
            }

            var resp = await _client.RegisterWeb(firstname, lastname, username, password, email, "WebRegister");

            Console.WriteLine($"{firstname},{lastname},{username},{password},{email},{emailPassword}");
            Console.WriteLine(resp.ToString());
            
            return resp == WebCreationStatus.Created;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            return false;
        }
    }
    
    public async Task<bool> Register(string username, string password, string firstname, string lastname, string email)
    {
        try
        {
            var resp = await _client.Register(username, password, firstname, lastname);

            Console.WriteLine(resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod.ToString());
            
            if(resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod == SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod.PhoneOnly || 
               resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod == SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod.PhoneFirstEmailSkippable || 
               resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod == SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod.PhoneFirstEmailBypassed || 
               resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod == SCJanusVerificationStatus.Types.SCJanusVerificationStatus_VerificationMethod.PhoneFirstEmailSkippable)
            {
                VerificationMethod =
                    resp.BootstrapData.UserState.VerificationStatus.PreferredVerificationMethod;
                
                await _client.ChangeEmail(email);
                
                return true;
            }
            else
            {
                // Change email address
                await _client.ChangeEmail(email);

                return true;
            }

            //Console.WriteLine($"{resp.ErrorData.HumanReadableErrorMessage}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            if (ex.Message.Contains("taken!"))
            {
                throw new Exception("Username is already taken.");
            }

            if (ex.Message.Contains("status code '502'"))
            {
                throw new Exception("Proxy error 502.");
            }
            
            return false;
        }
    }
    
    public async Task<string> ChangeEmail(string email)
    {
        var result = await _client.ChangeEmail(email);
        return result;
    }

    public async Task Subscribe(string username)
    {
        var result = await _client.Subscribe(username);
    }

    public async Task<SyncResponse.ami_friends?> SyncFriends()
    {
        var info = await _client.SyncFriends();
        
        return await _client.SyncFriends(info.added_friends_sync_token, info.friends_sync_token);
    }

    public async Task<bool> RelogAccounts(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup)
    {
        await _logger.LogDebug(work, "Starting RelogAccount task", account);
        
        try
        {
            if (work.CancellationTokenSource.IsCancellationRequested) return false;

            await _runner.Login(_accountManager, account, proxyGroup, work.CancellationTokenSource.Token);
            
            await _accountManager.UpdateAccount(account);
        
            await _logger.LogInformation(work, $"{account.Username} account auth has been refreshed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            return false;
        }

        return true;
    }
    
    public async Task<bool> RefreshFriends(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup)
    {
        await _logger.LogDebug(work, "Starting RefreshFriend task", account);
        
        var info = await _runner.SyncFriends(account, proxyGroup, work.CancellationTokenSource.Token);

        if (info == null) return false;
        
        var mutualFriends = new HashSet<string>();
        var incomingFriends = new HashSet<string>();
        var outgoingFriends = new HashSet<string>();
        var subscribers = new HashSet<string>();
        var subscribed = new HashSet<string>();
        foreach (var entry in info.friends)
        {
            if (work.CancellationTokenSource.IsCancellationRequested) break;

            if (entry.mutable_username.Equals("teamsnapchat") || entry.mutable_username.Equals(account.Username))
            {
                continue;
            }

            switch (entry.type)
            {
                case (int)FriendsEnums.Mutual:
                    mutualFriends.Add(entry.mutable_username);
                    continue;
                case (int)FriendsEnums.Outgoing:
                    outgoingFriends.Add(entry.mutable_username);
                    continue;
                case (int)FriendsEnums.Subscribed:
                    subscribed.Add(entry.mutable_username);
                    continue;
            }
        }

        foreach (var entry in info.added_friends)
        {
            if (work.CancellationTokenSource.IsCancellationRequested) break;

            if (entry.mutable_username.Equals("teamsnapchat") || entry.mutable_username.Equals(account.Username))
            {
                continue;
            }
            
            switch (entry.type)
            {
                case (int) AddedFriendsEnums.Pending:
                    incomingFriends.Add(entry.mutable_username);
                    continue;
                case (int) AddedFriendsEnums.Mutual:
                    mutualFriends.Add(entry.mutable_username);
                    continue;
                case (int) AddedFriendsEnums.Subscribers:
                    subscribers.Add(entry.mutable_username);
                    continue;
            }
        }
        
        account.IncomingFriendCount = incomingFriends.Count();
        //account.OutgoingFriendCount = outgoingFriends.Distinct().Count();
        account.FriendCount = mutualFriends.Count();

        await _accountManager.UpdateAccount(account);
        
        await _logger.LogInformation(work, $"{account.Username} finished counting {incomingFriends.Count()} pending friend(s).");
        await _logger.LogInformation(work, $"{account.Username} finished counting {outgoingFriends.Count()} outgoing friend(s).");
        await _logger.LogInformation(work, $"{account.Username} finished counting {mutualFriends.Count()} friend(s).");
        await _logger.LogInformation(work, $"{account.Username} finished counting {subscribers.Count()} subscribers(s).");
        await _logger.LogInformation(work, $"{account.Username} finished counting {subscribed.Count()} subscribed to.");

        return true;
    }
    
    public async Task<suggest_friend_high_availability?> GetSuggestions()
    {
        return await _client.GetSuggestions();
    }

    public async Task ReportUserRandom(string username)
    {
        await _client.ReportUserRandom(username);
    }
    
    public async Task ReportUserStoryRandom(string username)
    {
        await _client.ReportBusinessStoryRandom(username);
    }

    public async Task PostDirect(string inputFile, string? postDirectswipeUpUrl, HashSet<string> users)
    {
        await _client.PostDirect(inputFile, users, postDirectswipeUpUrl, null);
    }

    public async Task<SCFriendingFriendsActionResponse> AddByUsername(string username)
    {
        return await _client.AddBySearch(username);
    }

    public async Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string username)
    {
        return await _client.AddByQuickAdd(username);
    }

    public async Task<SCFriendingFriendsActionResponse> AcceptFriend(string username)
    {
        return await _client.AcceptFriend(username);
    }
    
    public async Task<SCFriendingFriendsActionResponse> RemoveFriend(string username)
    {
        return await _client.RemoveFriend(username);
    }

    public async Task GetAccessTokens()
    {
        try
        {
            await _client.GetAccessTokens();
            //await _client.Validate();
        }
        catch
        {
            throw;
        }
    }
    
    public async Task Login2FA(string code)
    {
        try
        {
            await _client.Login2FA(code);
            //await _client.Validate();
        }
        catch
        {
            throw;
        }
    }
    
    public async Task InitClient()
    {
        ClientStatus = SnapchatClientStatus.Initializing;
        try
        {
            await _client.InitClient();
            //await _client.Validate();
        }
        catch (Exception e) when (e is UnauthorizedAuthTokenException or BannedProxyForUploadException or DeadProxyException or ProxyTimeoutException or ProxyAuthRequiredException or RateLimitedException or FailedHttpRequestException)
        {
            ClientStatus = SnapchatClientStatus.NotInitialized;
            throw;
        }
        catch
        {
            ClientStatus = SnapchatClientStatus.NotInitialized;
            throw;
        }

        ClientStatus = SnapchatClientStatus.Initialized;
    }

    public async Task<ValidationStatus> ValidateEmail(string link, bool penis)
    {
        return (ValidationStatus)await _client.ValidateEmail(link, penis);
    }
    public async Task<bool> Validate()
    {
        try
        {
            await _client.Validate();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task SendMessage(string message, HashSet<string> users)
    {
        await _client.SendMessage(message, users);
    }
    public async Task<bool> ChangeUsername(string newName, string password)
    {
        try
        {
            await _client.ChangeUsername(newName, password);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    public async Task SendMention(string user, HashSet<string> users)
    {
        await _client.SendMention(user, users);
    }
    public async Task SendLink(string message, HashSet<string> users)
    {
        await _client.SendLink(message, users);
    }
    public async Task ViewBusinessPublicStory(string username)
    {
        await _client.ViewBuisnessStory(username);
    }
    public async Task ViewPublicStory(string username)
    {
        await _client.ViewPublicStory(username, 0);
    }
    
    public async Task Login(string username, string password)
    {
        try
        {
            SCJanusLoginWithPasswordResponse loginResult = await _client.Login(username, password);
            
            if(loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.LoginSuccess)
            {
                await _client.GetAccessTokens();
                await _client.InitClient();
                await _client.Validate();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}{ex.StackTrace}");
        }
    }
        
    public async Task Login(SnapchatAccountManager manager, SnapchatAccountModel account)
    {
        try
        {
            SCJanusLoginWithPasswordResponse loginResult = await _client.Login(account.Username, account.Password);
            
            if(loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.LoginSuccess)
            {
                account.AuthToken = _client.SnapchatConfig.AuthToken;
                account.AccessToken = _client.SnapchatConfig.Access_Token;
                account.DToken1I = _client.SnapchatConfig.dtoken1i;
                account.DToken1V = _client.SnapchatConfig.dtoken1v;
                account.InstallTime = _client.SnapchatConfig.install_time;
                account.UserId = _client.SnapchatConfig.user_id;
                account.BusinessAccessToken = _client.SnapchatConfig.BusinessAccessToken;
                account.AccountCountryCode = _client.SnapchatConfig.AccountCountryCode;
                account.Horoscope = _client.SnapchatConfig.Horoscope;
                account.TimeZone = _client.SnapchatConfig.TimeZone;
                account.ClientID = _client.SnapchatConfig.ClientID;
                account.Age = _client.SnapchatConfig.Age;
                account.refreshToken = _client.SnapchatConfig.refreshToken;
                account.SetStatus(manager, AccountStatus.OKAY);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                    .SCJanusLoginWithPasswordResponse_StatusCode.AccountLocked)
            {
                account.SetStatus(manager, AccountStatus.LOCKED);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                         .SCJanusLoginWithPasswordResponse_StatusCode.AccountDeactivated)
            {
                account.SetStatus(manager, AccountStatus.BANNED);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                         .SCJanusLoginWithPasswordResponse_StatusCode.AndroidSafetynetRequested)
            {
                account.SetStatus(manager, AccountStatus.NEEDS_CHECKED);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                         .SCJanusLoginWithPasswordResponse_StatusCode.TwoFaRequired)
            {
                account.SetStatus(manager, AccountStatus.NEEDS_CHECKED);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                         .SCJanusLoginWithPasswordResponse_StatusCode.OdlvRequired)
            {
                account.SetStatus(manager, AccountStatus.NEEDS_CHECKED);
            }
            else if (loginResult.StatusCode == SCJanusLoginWithPasswordResponse.Types
                         .SCJanusLoginWithPasswordResponse_StatusCode.LoginCodeSent)
            {
                account.SetStatus(manager, AccountStatus.NEEDS_CHECKED);
            }
            else if (loginResult.StatusCode ==
                     SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.Unset)
            {
                account.SetStatus(manager, AccountStatus.BAD_PROXY);
            }
            else
            {
                account.SetStatus(manager, AccountStatus.BAD_PROXY);
            }
            
        }
        catch (DeadAccountException e)
        {
            // This is a exception in justxn's code please parse me >,<

            account.AccountStatus = AccountStatus.BANNED;
            throw new AccountBannedException(account.Username, e);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains(
                    "Due to repeated failed attempts or other unusual activity, your access to Snapchat is temporarily disabled."))
            {
                account.SetStatus(manager,AccountStatus.LOCKED);
            }
            else
            {
                account.SetStatus(manager, AccountStatus.NEEDS_CHECKED);
            }

            throw new Exception($"{ex.Message}{ex.StackTrace}");
        }
    }

    public async Task WaitForInitClient(CancellationToken token)
    {
        while (ClientStatus == SnapchatClientStatus.Initializing) await Task.Delay(1000, token);
    }

    public async Task PostStoryLegacy(string inputFile, string swipeUpUrl = null, List<string> mentioned = null)
    {
        await _client.PostStory(inputFile, swipeUpUrl, mentioned);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _client.Dispose();
    }
}