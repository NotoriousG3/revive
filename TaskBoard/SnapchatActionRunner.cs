using Microsoft.IdentityModel.Tokens;
using SnapchatLib;
using SnapchatLib.Exceptions;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Activation.Api;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Search;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard;

public class MaxRetriesException : Exception
{
    public MaxRetriesException(int retries) : base($"Unable to execute the specified action after {retries} attempts")
    {
    }
}

public class AccountBannedException : Exception
{
    public AccountBannedException(string username, Exception innerException) : base($"The account {username} seems to have been banned", innerException)
    {
    }
}

public class SnapchatActionRunner
{
    private const int RetryDelayMs = 1500;
    private readonly SnapchatClientFactory _factory;
    private readonly ILogger<SnapchatActionRunner> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IProxyManager _proxyManager;
    private readonly SnapchatAccountManager _accountManager;
    private readonly EmailManager _emailManager;

    private const SnapchatVersion _defaultSnapchatVersion = SnapchatVersion.V12_27_0_8;

    public SnapchatActionRunner() {}
    public SnapchatActionRunner(IServiceProvider provider, SnapchatClientFactory factory, ILogger<SnapchatActionRunner> logger, IProxyManager proxyManager, SnapchatAccountManager accountManager, EmailManager emailManager)
    {
        _factory = factory;
        _logger = logger;
        _serviceProvider = provider;
        _proxyManager = proxyManager;
        _accountManager = accountManager;
        _emailManager = emailManager;
    }

    private async Task<ISnapchatClient> GetClient(OS os, SnapchatVersion snapchatVersion, ProxyGroup? proxyGroup, CancellationToken cancellationToken = default)
    {
        var options = new CreateSnapchatClientOptions()
        {
            OS = os,
            SnapchatVersion = SnapchatVersion.V12_28_0_22,
            ProxyGroup = proxyGroup
        };
        
        return await _factory.Create(options, options.SnapchatVersion, cancellationToken, false);
    }

    private async Task<ISnapchatClient> GetClient(SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken = default, bool initClient = true)
    {
        var options = new CreateSnapchatClientOptions()
        {
            OS = account.OS,
            SnapchatVersion = account.SnapchatVersion,
            ProxyGroup = proxyGroup,
            Account = account
        };
        
        account.SnapClient = await _factory.Create(options, options.SnapchatVersion, cancellationToken, initClient);

        return account.SnapClient;
    }

    public async Task<SCSuggestUsernamePbSuggestUsernameResponse?> SuggestUsername(OS os, string firstname, string lastname, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        using var client = await GetClient(OS.android, _defaultSnapchatVersion, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.SuggestUsername(firstname, lastname), null, proxyGroup, cancellationToken);
    }

    public async Task<SnapchatAccountModel> Register(string username, string password, string firstname, string lastname, string email, string? emailPassword, OS os, SnapchatVersion snapchatVersion, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        bool validated = false;
        
        return await RunWithRetry(async () =>
        {
            var settings = await AppSettings.GetSettingsFromProvider(_serviceProvider);
            
            var client = await GetClient(os, snapchatVersion, proxyGroup, cancellationToken);

            EmailModel eModel = new();

            if(settings.EnableWebRegister){
                TwoFaSingleRequest kopRequest = new(settings, _proxyManager);

                var Email = kopRequest.Address;
                email = Email;
                
                if (await client.RegisterWeb(settings, username, password, firstname, lastname, Email, emailPassword))
                {
                    await client.Login(username, password);
                    
                    var msg = await kopRequest.WaitForValidationCode(client.SnapchatClient);
                    
                    await client.Login2FA(msg);
                    await client.GetAccessTokens();
                    await client.InitClient();
                    await client.Validate();
                    
                    await client.ResendVerifyEmail();

                    await kopRequest?._api.ReOrderMail("snapchat.com", Email, @"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*", "Confirm Your Email Address");
                    
                    var status = await kopRequest?.WaitForValidationEmail(client.SnapchatClient);

                    if (status != SnapchatLib.Extras.ValidationStatus.Validated)
                    {
                        throw new Exception("Register failed.");
                    }

                    eModel.Address = email;
                    validated = true;
                }
            }
            else{
                await client.Register(username, password, firstname, lastname, email);
            }
            
            var proxyInfo = await _proxyManager.GetProxyFromDatabase(client.SnapchatConfig.Proxy);

            if (client.SnapchatConfig.AuthToken.IsNullOrEmpty())
            {
                throw new Exception("Register failed.");
            }
            
            var account = new SnapchatAccountModel
            {
                Username = username,
                Password = password,
                AuthToken = client.SnapchatConfig.AuthToken,
                Device = client.SnapchatConfig.Device,
                Install = client.SnapchatConfig.Install,
                DToken1I = client.SnapchatConfig.dtoken1i,
                DToken1V = client.SnapchatConfig.dtoken1v,
                InstallTime = client.SnapchatConfig.install_time,
                SnapchatVersion = client.SnapchatConfig.SnapchatVersion,
                UserId = client.UserId,
                OS = os,
                CreationDate = DateTime.UtcNow,
                Proxy = proxyInfo,
                DeviceProfile = client.SnapchatConfig.DeviceProfile,
                AccessToken = client.SnapchatConfig.Access_Token,
                BusinessAccessToken = client.SnapchatConfig.BusinessAccessToken,
                AccountCountryCode = client.SnapchatConfig.AccountCountryCode,
                Horoscope = client.SnapchatConfig.Horoscope,
                TimeZone = client.SnapchatConfig.TimeZone,
                ClientID = client.SnapchatConfig.ClientID,
                Age = client.SnapchatConfig.Age,
                refreshToken = client.SnapchatConfig.refreshToken
            };

            if (validated)
            {
                _emailManager.AssignEmail(account, eModel);
                
                account.EmailValidated = ValidationStatus.Validated;
            }

            return account;
        }, null, proxyGroup, cancellationToken);
    }

    private static bool IsProxySwapingException(Exception e)
    {
        return e is BadProxyException || e.Message == "Proxy Dead" || e.Message.Contains("blocked because of suspicious") || e is BannedProxyForUploadException;
    }

    private static bool IsRetriableException(Exception e)
    {
        return e is ProxyTimeoutException or MalformedRequestException or EmailDomainBannedException or InvalidPasswordException ||
               e.Message.Contains("Object reference not set to an instance of an object.") ||
               e.Message.Contains("Grpc.Core.RpcException") ||
               e.Message.Contains("DeadlineExceeded") ||
               e.Message.Contains("Error starting gRPC call.") ||
               e.Message.Contains("Expected delimiter: \". Path 'cof_response.base64_encoded_response', line 1, position") ||
               e.Message.Contains("Unhandled status code for HttpStatusCode: 430") ||
               e.Message.Contains("request was canceled due to the configured HttpClient.Timeout") ||
               e.Message.Contains("failed with status code '502'") ||
               e.Message.Contains("SSL connection could not be established");
    }

    private async Task DeleteAccount(string reason, SnapchatAccountModel account, bool saveAsBanned = false)
    {
        _logger.LogInformation($"Account {account.Username} will be deleted because of {reason}");
        // When unauthorized, the account is no longer considered valid so we just delete it
        using var scope = _serviceProvider.CreateScope();
        var accountManager = scope.ServiceProvider.GetRequiredService<SnapchatAccountManager>();
        await accountManager.Delete(account.Id, saveAsBanned);
    }

    private async Task<T> RunWithRetry<T>(Func<Task<T>> func, SnapchatAccountModel? account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var settings = await AppSettings.GetSettingsFromProvider(_serviceProvider);

        var attempts = 0;
        while (attempts < settings.MaxRetries)
        {
            
            // We need to stop trying when a cancellation is requested
            if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

            try
            {
                var result = await func();
                return result;
            }
            catch (NullReferenceException)
            {
                try
                {
                    var client = await GetClient(account, proxyGroup, cancellationToken);


                    client.InitClient();
                    client.Validate();
                }
                catch (Exception e)
                {
                    
                }
            }
            catch (UnauthorizedAuthTokenException)
            {
                try
                {
                    // IDK WHY THE HECK THIS NEEDS TO BE LIKE THIS
                    if (account == null)
                    {
                        attempts++;
                        continue;
                    }

                    var client = await GetClient(account, proxyGroup, cancellationToken);

                    //Rewrite login to not do a infinite run with retry.

                    await client.Login(_accountManager, account);
                }
                catch (DeadAccountException e)
                {
                    // IDK WHY THE HECK THIS NEEDS TO BE LIKE THIS
                    if (account == null)
                    {
                        attempts++;
                        continue;
                    }

                    account.AccountStatus = AccountStatus.BANNED;
                    _logger.LogInformation($"{account.Username} is banned");
                    await DeleteAccount("Dead Account Exception", account);
                    throw new AccountBannedException(account.Username, e);
                }
                catch (Exception e)
                {
                    // IDK WHY THE HECK THIS NEEDS TO BE LIKE THIS
                    if (account == null)
                    {
                        attempts++;
                        continue;
                    }
                    //account.SetStatus(_accountManager, AccountStatus.NEEDS_CHECKED);
                    _logger.LogError($"Unexpected error when trying to run a task: {e}");
                    throw;
                }
            }
            catch (DeadAccountException e)
            {
                // IDK WHY THE HECK THIS NEEDS TO BE LIKE THIS
                if (account == null)
                {
                    attempts++;
                    continue;
                }

                account.SetStatus(_accountManager, AccountStatus.BANNED);
                await DeleteAccount("Dead Account Exception", account);
                _logger.LogInformation($"{account.Username} is banned");
                throw new AccountBannedException(account.Username, e);
            }
            catch (RateLimitedException)
            {
                // IDK WHY THE HECK THIS NEEDS TO BE LIKE THIS
                if (account == null)
                {
                    attempts++;
                    continue;
                }

                _logger.LogInformation("Account or Proxy is RateLimited stopping task for this account......");
                account.AccountStatus = AccountStatus.RATE_LIMITED;
                // Do not retry ratelimited
                break;
            }
            catch (Exception e) when (IsProxySwapingException(e))
            {
                _logger.LogInformation("The used proxy seems unusable, will attempt to retry with a different proxy");
                // If we have an account, then we have a reference to a proxy, so we should remove it
                if (account != null)
                {
                    var proxyManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IProxyManager>();

                    if(account.Proxy != null)
                        await proxyManager.DeleteProxy(account.Proxy.Id);
                }

                attempts++;
                await Task.Delay(RetryDelayMs, cancellationToken);
            }
            catch (Exception e) when (IsRetriableException(e))
            {
                _logger.LogInformation($"Attempt {attempts} of {settings.MaxRetries}. Caught an exception that we will retry.\n{e}");
                attempts++;
                await Task.Delay(RetryDelayMs, cancellationToken);
            }
            catch (Exception e)
            {
                //account.AccountStatus = AccountStatus.NEEDS_CHECKED;
                _logger.LogError($"Unexpected error when trying to run a task: {e}");
                throw;
            }
        }

        throw new MaxRetriesException(settings.MaxRetries);
    }

    public async Task<bool> Login(SnapchatAccountManager manager, SnapchatAccountModel account, ProxyGroup proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken, false);

        return await RunWithRetry(async () =>
        {
            await client.Login(manager, account);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<string> ChangePhone(SnapchatAccountModel account, string phone, string country, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.ChangePhone(phone, country), account, proxyGroup, cancellationToken);
    }
    
    public async Task<string> ChangeEmail(SnapchatAccountModel account, string email, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.ChangeEmail(email), account, proxyGroup, cancellationToken);
    }

    public async Task<bool> VerifyPhone(SnapchatAccountModel account, string code, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.VerifyPhone(code);
            return true;
        }, account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> ResendVerifyEmail(SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ResendVerifyEmail();
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> Subscribe(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.Subscribe(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<SyncResponse.ami_friends?> SyncFriends(SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.SyncFriends(), account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> ChangeUsername(SnapchatAccountModel account, string newName, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.ChangeUsername(newName, account.Password), account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> RefreshFriends(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup)
    {
        var client = await GetClient(account, proxyGroup, work.CancellationTokenSource.Token);
        return await RunWithRetry(async () => await client.RefreshFriends(account, work, _accountManager, _runner, _logger, proxyGroup), account, proxyGroup, work.CancellationTokenSource.Token);
    }
    
    public async Task<bool> RelogAccount(SnapchatAccountModel account, WorkRequest work, SnapchatAccountManager _accountManager, SnapchatActionRunner _runner, WorkLogger _logger, ProxyGroup? proxyGroup)
    {
        var client = await GetClient(account, proxyGroup, work.CancellationTokenSource.Token,false);
        return await RunWithRetry(async () => await client.RelogAccounts(account, work, _accountManager, _runner, _logger, proxyGroup), account, proxyGroup, work.CancellationTokenSource.Token);
    }
    
    public async Task<suggest_friend_high_availability?> GetSuggestions(SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.GetSuggestions(), account, proxyGroup, cancellationToken);
    }

    public async Task<bool> ReportUserPublicProfileRandom(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ReportUserRandom(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> ReportUserRandom(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ReportUserRandom(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> ReportUserStoryRandom(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ReportUserStoryRandom(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> PostDirect(SnapchatAccountModel account, string inputFile, string? postDirectswipeUpUrl, HashSet<string> users, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.PostDirect(inputFile, postDirectswipeUpUrl, users);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public string FixLink(string originalLink)
    {
        if (!originalLink.Contains("http"))
        {
            originalLink = "https://" + originalLink;
        }
        
        if (!originalLink.Contains("https"))
        {
            originalLink = originalLink.Replace("http","https");
        }

        return originalLink;
    }
    
    public async Task<bool> CreateCustomBitmoji(SnapchatAccountModel account, ApplicationDbContext context, int bitmoji_id, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.CreateCustomBitmoji(context,bitmoji_id), account, proxyGroup, cancellationToken);
    }
    
    public async Task<SCS2SearchResponse> FindUsersViaSearch(SnapchatAccountModel account, string search, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.FindUsersViaSearch(search), account, proxyGroup, cancellationToken);
    }

    public async Task<bool> IsUserActive(SnapchatAccountModel account, string search, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.IsUserActive(search), account, proxyGroup, cancellationToken);
    }
    
    public async Task<SCFriendingContactBookUploadResponse> PhoneToUsername(SnapchatAccountModel account, string number, string countryCode, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.PhoneToUsername(number, countryCode), account, proxyGroup, cancellationToken);
    }
    
    public async Task<SCFriendingContactBookUploadResponse> EmailToUsername(SnapchatAccountModel account, string address, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.EmailToUsername(address), account, proxyGroup, cancellationToken);
    }

    public async Task<SCFriendingFriendsActionResponse> AddFriend(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.AddByUsername(username), account, proxyGroup, cancellationToken);
    }

    public async Task<SCFriendingFriendsActionResponse> AddByQuickAdd(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.AddByQuickAdd(username), account, proxyGroup, cancellationToken);
    }

    public async Task<SCFriendingFriendsActionResponse> AcceptFriend(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.AcceptFriend(username), account, proxyGroup, cancellationToken);
    }
    
    public async Task<SCFriendingFriendsActionResponse> RemoveFriend(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () => await client.RemoveFriend(username), account, proxyGroup, cancellationToken);
    }

    public async Task<bool> InitClient(ISnapchatClient client, SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        return await RunWithRetry(async () =>
        {
            await client.InitClient();
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> SendMessage(SnapchatAccountModel account, string message, HashSet<string> users, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.SendMessage(message, users);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<ValidationStatus> ValidateEmail(SnapchatAccountModel account, string link, bool penis, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await client.ValidateEmail(link, penis);
    }
    
    public async Task<bool> SendMention(SnapchatAccountModel account, string user, HashSet<string> users, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.SendMention(user, users);
            return true;
        }, account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> SendLink(SnapchatAccountModel account, string message, HashSet<string> users, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.SendLink(message, users);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> ViewBusinessPublicStory(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ViewBusinessPublicStory(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> ViewPublicStory(SnapchatAccountModel account, string username, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.ViewPublicStory(username);
            return true;
        }, account, proxyGroup, cancellationToken);
    }
    
    public async Task<bool> PostStoryLegacy(SnapchatAccountModel account, string inputFile, string? swipeUpUrl, List<string>? mentioned, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            await client.PostStoryLegacy(inputFile, swipeUpUrl, mentioned);
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task<bool> CheckAccountStatus(WorkRequest work, WorkLogger logger, WorkScheduler scheduler, SnapchatAccountModel account)
    {
        if (account.AccountStatus != AccountStatus.OKAY)
        {
            await logger.LogError(work, $"Accounts status is no longer fit to run this job.", account);
            await scheduler.FailWorkAccount(work, account);
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> CallBitmoji(SnapchatAccountModel account, BitmojiSelection selection, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        var client = await GetClient(account, proxyGroup, cancellationToken);
        return await RunWithRetry(async () =>
        {
            switch (selection)
            {
                default:
                    break;
            }
            return true;
        }, account, proxyGroup, cancellationToken);
    }

    public async Task Test(SnapchatAccountModel account, ProxyGroup? proxyGroup, CancellationToken cancellationToken)
    {
        await GetClient(account, proxyGroup, cancellationToken);
    }
}