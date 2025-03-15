using System.Text;
using _5sim.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapchatLib;
using SnapProto.Snapchat.Search;
using TaskBoard.Data;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;
using TaskBoard.Models.SnapchatActionModels;
using TaskBoard.WorkTask;

namespace TaskBoard.Controllers;

public enum AccountImportField
{
    UserId,
    Username,
    Password,
    AuthToken,
    Email,
    Device,
    Install,
    DToken1i,
    DToken1v,
    InstallTime,
    Os,
    SnapchatVersion,
    ProxyAddress,
    ProxyUser,
    ProxyPassword,
    DeviceProfile,
    AccessToken,
    BusinessToken,
    AccountCountryCode,
    Horoscope,
    TimeZone,
    ClientID,
    Age,
    refreshToken
}

public class EmailAlreadyInUseException : Exception
{
    public EmailAlreadyInUseException(EmailModel email) : base($"The e-mail {email.Address} is already assigned in the system")
    {
    }
}

// Changing this enum ALSO requires an update in accounttools.js
public enum LineProcessStatus
{
    Ok = 0,
    UnknownError = 1,
    IndexOutOfRange = 2,
    DuplicatedAccount = 3,
}

public struct LineProcessResult
{
    public LineProcessStatus Status;
    public SnapchatAccountModel? Account;
    public Proxy? Proxy;
    public EmailModel? Email;
    public int LineNumber;
}

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/[controller]")]
[ApiController]
public class AccountController : ApiController
{
    private readonly SnapchatAccountManager _accountManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkScheduler _scheduler;
    private readonly AppSettingsLoader _settingsLoader;
    private readonly IProxyManager _proxyManager;
    private readonly EmailManager _emailManager;
    private readonly UploadManager _uploadManager;

    public AccountController(IServiceScopeFactory scopeFactory, SnapchatAccountManager manager, WorkScheduler scheduler, AppSettingsLoader settingsLoader, IProxyManager proxyManager, EmailManager emailManager, UploadManager uploadManager)
    {
        _scopeFactory = scopeFactory;
        _accountManager = manager;
        _scheduler = scheduler;
        _settingsLoader = settingsLoader;
        _proxyManager = proxyManager;
        _emailManager = emailManager;
        _uploadManager = uploadManager;
    }

    [NonAction]
    private UnauthorizedObjectResult MaximumManagedAccounts(int maxAccounts)
    {
        return UnauthorizedApi($"Maximum amount of {maxAccounts} managed accounts reached", null, ApiResponseCode.MaximumAccounts);
    }
    
    [HttpPost("data")]
    public async Task<ActionResult<IEnumerable<UIAccountModel>>> GetAllAccounts(DataTableAjaxModel model)
    {
        var settings = await _settingsLoader.Load();

        var isNumber = Int32.TryParse(model.search.value, out var searchNumber);

        var accountsList = SearchUtilities.SearchDataTablesEntities(model, 
            (await _accountManager.GetAllowedAccounts(settings)).AsQueryable(),
            (e => 
                e.Username.ToLowerInvariant().Contains(model.search.value) ||
                searchNumber == 0 && isNumber ||
                (searchNumber > 0 && e.FriendCount == searchNumber)), 
            out var filteredCount, 
            out var totalCount);
        
        // Now sort them out by creation date
        accountsList.ToList().Sort((x, y) =>
        {
            if (x.CreationDate == null && y.CreationDate == null) return 0;
            if (x.CreationDate == null) return -1;
            if (y.CreationDate == null) return 1;
            if (x.CreationDate > y.CreationDate) return 1;
            if (y.CreationDate > x.CreationDate) return -1;
            return 0;
        });
        
        // Now limit to maximums based on settings
        if (filteredCount > settings.MaxManagedAccounts)
            filteredCount = settings.MaxManagedAccounts;

        if (totalCount > settings.MaxManagedAccounts)
            totalCount = settings.MaxManagedAccounts;

        var matchingEmails = await _emailManager.GetAccountsEmail(accountsList);
        var accounts = UIAccountModel.ToEnumerable(accountsList, matchingEmails.ToList());
        return Ok(new DataTablesResponse
        {
            Draw = model.draw,
            RecordsFiltered = filteredCount,
            RecordsTotal = totalCount,
            Data = accounts
        });
    }

    // POST: api/Account
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<SnapchatAccountModel>> CreateAccounts(CreateAccountArguments arguments)
    {
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var settings = await _settingsLoader.Load();

        switch (arguments.PhoneVerificationService)
        {
            // We need to validate that our phone verification services are configured before being able to continue
            case PhoneVerificationService.FiveSim when string.IsNullOrWhiteSpace(settings.FiveSimApiKey):
                return BadRequestApi("There is no API key for FiveSIM verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
            case PhoneVerificationService.SmsPool when string.IsNullOrWhiteSpace(settings.SmsPoolApiKey):
                return BadRequestApi("There is no API key for SMSPool verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
            case PhoneVerificationService.Twilio when string.IsNullOrWhiteSpace(settings.TwilioApiKey):
                return BadRequestApi("There is no API key for Twilio verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
            case PhoneVerificationService.TextVerified when string.IsNullOrWhiteSpace(settings.TextVerifiedApiKey):
                return BadRequestApi("There is no API key for TextVerified verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
            case PhoneVerificationService.SMSActivate when string.IsNullOrWhiteSpace(settings.SmsActivateApiKey):
                return BadRequestApi("There is no API key for SMS-Activate verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
        }
        
        switch (arguments.EmailVerificationService)
        {
            case EmailVerificationService.Kopeechka when string.IsNullOrWhiteSpace(settings.KopeechkaApiKey):
                return BadRequestApi("There is no API key for Kopeechka verification service", null, ApiResponseCode.NoPhoneVerificationApiSet);
        }
        
        var currentAccounts = (await _accountManager.GetAllowedAccounts(settings)).Count;
        var remainingAccounts = settings.MaxManagedAccounts - currentAccounts;
        if (remainingAccounts <= 0)
            return MaximumManagedAccounts(settings.MaxManagedAccounts);

        // If we have less managed accounts available than what we request, only use the maximum we have available
        arguments.AccountsToUse = remainingAccounts > arguments.AccountsToUse ? arguments.AccountsToUse : remainingAccounts;

        var workRequest = await _scheduler.CreateAccounts(arguments);

        return OkApi("Scheduled CreateAccounts Job", workRequest.Id);
    }

    // RELOG: api/Account/relog/5
    [HttpPost("changeusername")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUsername(ChangeUsernameArguments args)
    {
        // Validate the arguments. BadRequest allows us to communicate back the error
        args.AccountsToUse = 1;
        args.ScheduledTime = DateTime.UtcNow;
        var validationResult = args.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);
        var workRequest = await _scheduler.ChangeUsername(args);

        return OkApi("Scheduled ChangeUsername Job", workRequest.Id);
    }
    
    // RELOG: api/Account/relog/5
    [HttpPost("relog/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RelogAccount(long id)
    {
        var relogResult = await _accountManager.Relog(id);
        switch (relogResult)
        {
            case RelogResult.AccountNotFound:
                return NotFound();
            case RelogResult.IncorrectAccountStatus:
                return BadRequestApi("Account must be status `Okay` to proceed.");
            default:
                return OkApi();
        }
    }
    
    // RELOG: api/Account/loadfriends/5
    [HttpPost("loadfriends/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReloadFriends(long id)
    {
        if (!await _accountManager.ReloadFriends(id)) return NotFound();
        
        return OkApi();
    }
    
    // DELETE: api/Account/5
    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSnapchatAccountModel(long id)
    {
        if (!await _accountManager.Delete(id)) return NotFound();
        
        return OkApi();
    }

    private async Task<LineProcessResult> ProcessLine(string line, Dictionary<string, SnapchatAccountModel> addedAccounts, long groupId = 0)
    {
        var fields = line.Split('*');

        var result = new LineProcessResult() { Status = LineProcessStatus.Ok };
        
        SnapchatAccountModel account;

        try
        {
            await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var userId = fields[(int) AccountImportField.UserId];
            var username = fields[(int)AccountImportField.Username];
            
            // We exit early if our account is duplicated
            if (await _accountManager.Exists(userId) || addedAccounts.ContainsKey(userId))
            {
                // need this to show in the UI
                result.Account = new SnapchatAccountModel() { Username = username };
                result.Status = LineProcessStatus.DuplicatedAccount;
                return result;
            }
            
            Proxy? proxy = null;
            var os = OS.android;
            long.TryParse(fields[(int) AccountImportField.InstallTime], out var installTime);
            int.TryParse(fields[(int) AccountImportField.Age], out var age);
            Enum.TryParse<SnapchatVersion>(fields[(int) AccountImportField.SnapchatVersion], out var version);
            Enum.TryParse<SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign>(fields[(int) AccountImportField.Horoscope], out var horoscope);
            var proxyAddress = fields[(int) AccountImportField.ProxyAddress];
            var proxyUser = fields[(int) AccountImportField.ProxyUser];
            var proxyPass = fields[(int) AccountImportField.ProxyPassword];
            
            if (!string.IsNullOrWhiteSpace(proxyAddress))
            {
                var builder = new UriBuilder(proxyAddress);
                
                proxy = await _proxyManager.GetProxyFromDatabase(builder.Uri, proxyUser, proxyPass);
            
                if (proxy == null)
                {
                    proxy = new Proxy() {Address = builder.Uri, User = proxyUser, Password = proxyPass};
                    result.Proxy = proxy;
                    await context.Proxies.AddAsync(proxy);
                    await context.SaveChangesAsync();
                }
            }
            
            account = new SnapchatAccountModel
            {
                UserId = userId,
                Username = username,
                AuthToken = fields[(int) AccountImportField.AuthToken],
                Password = fields[(int) AccountImportField.Password],
                Device = fields[(int) AccountImportField.Device],
                Install = fields[(int) AccountImportField.Install],
                OS = os,
                DToken1I = fields[(int) AccountImportField.DToken1i],
                DToken1V = fields[(int) AccountImportField.DToken1v],
                InstallTime = installTime,
                SnapchatVersion = version,
                Proxy = proxy,
                DeviceProfile = fields[(int) AccountImportField.DeviceProfile],
                AccessToken = fields[(int) AccountImportField.AccessToken],
                BusinessAccessToken = fields[(int) AccountImportField.BusinessToken].Length > 0 ? fields[(int) AccountImportField.BusinessToken] : "eyJpc3MiOiJodHRwczpcL1wvYXV0aC5zbmFwY2hhdC5jb21cL3NuYXBfdG9rZW5cL3Rva2VuIiwidHlwIjoiSldUIiwiZW5jIjoiQTEyOENCQy1IUzI1NiIsImFsZyI6ImRpciIsImtpZCI6InNuYXAtYWNjZXNzLXRva2VuLWExMjhjYmMtaHMyNTYuMCJ9..5LXbTGaHrzBCh5ar4ytyNA.as2B1NWwR4Qryyymmk1A1k3oONZnJ2gZ6wDWWdY7frqkbhc1g9Az4JwMCnSFXjcKVnrR9ymFCLF8tMN0LQjBLyXaPx98qj219cRp7Dxk17fWUIibxlF_7j6j8eYTh3ob5S3SiDa-J4o6TbVap-H0TPZKZeT5uJMZi47UlxRAUJZx8TOVJtXs2a4xITGJ9o-motIu8_VBegeJSKbOsYfkubD78SEI9ETnOHycBDFkxD-2P4EuAMSjD-LZGsqruvkaJqtKnBhbkILoYPyjpnWypI-5_w44DAiGNROoMgxnDc04PU1mHYk3m2gj1nOOCGPIfT8i09pencVuVJeOYFf7lWx32H9zGu5xapJ48tOUiWU911O1oURM_LMObRZmC0Iw5fFwqf8q1csmV4_LqMtsJJtBoDWz6TBRjoL8jIb4jkbdCkS4TOynC4zkN0KgnANi2Kc9twXK_jn2VZkARaNYM3-LKw7o_eKlOdNFOQQV1aqv6okBRF74tJX80h-nMPwhJ0CVtwjZ_CBT9mXyqU4z6tC4fEe7EsRd-Cia6vvsTtZgxGhgMciRoVZTHn4t-j_TGE35g52l4kRhCZkut62ELz7vld9O3BIxq2ew06q-iGFi_nIjczFBayPb37SYL_Mzb1F8VPOpTyxf2-g-PczvDGZW0l_rBtavslU1JKp8j6FoYLX0Z5sY4zer53b2m_LfSOVJvcd8qptxAxRQMLRnJA.d6XZ23O4x5l3WHr26M8ztw",
                AccountCountryCode = fields[(int) AccountImportField.AccountCountryCode],
                Horoscope = horoscope,
                TimeZone = fields[(int) AccountImportField.TimeZone],
                ClientID = fields[(int) AccountImportField.ClientID],
                Age = age,
                refreshToken = fields[(int) AccountImportField.refreshToken],
                CreationDate = DateTime.UtcNow,
            };

            var emailAddress = fields[(int) AccountImportField.Email];
            
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                //result.Status = LineProcessStatus.MissingEmail;
            }
            else
            {
                result.Email = await _emailManager.GetEmailFromDatabase(emailAddress);
            
                if (result.Email != null)
                {
                    result.Email.Account = account;
                }
                else
                {
                    result.Email = new EmailModel() {Address = emailAddress, Account = account, IsFake = true};
                }
                
                await context.Emails.AddAsync(result.Email, CancellationToken.None);
                
                _emailManager.AssignEmail(account, result.Email);
            }

            result.Account = account;
            
            await context.Accounts.AddAsync(result.Account);
            await context.SaveChangesAsync();
            
            // We append the group to the list, it should create the reference by itself.
            // And this needs to happen after the account is in the db, otherwise groups were duplicating themselves...
            if (groupId > 0)
            {
                var group = await context.AccountGroups.FindAsync(groupId);
                account.Groups = new List<AccountGroup> { group };
                await context.SaveChangesAsync();
            }
        }
        catch (IndexOutOfRangeException)
        {
            result.Status = LineProcessStatus.IndexOutOfRange;
        }
        catch (Exception)
        {
            result.Status = LineProcessStatus.UnknownError;
        }

        return result;
    }
    
    private async Task<AccountGroup?> GetOrCreateGroup(string groupName = "", long groupId = 0)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (groupId > 0)
        {
            return await context.AccountGroups.FindAsync(groupId);
        }

        if (string.IsNullOrWhiteSpace(groupName)) return null;

        var match = context.AccountGroups.Where(e => e.Name.ToLower() == groupName.ToLower());

        if (await match.AnyAsync()) return await match.FirstAsync();

        // create a new group with the given name
        var newGroup = new AccountGroup() { Name = groupName };
        await context.AccountGroups.AddAsync(newGroup);
        await context.SaveChangesAsync();
        return newGroup;
    }
    
    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAccounts(ImportAccountWithGroupArguments arguments)
    {
        if (arguments.UploadId == 0) return BadRequest("Invalid file id");
        var file = await _uploadManager.GetFile(arguments.UploadId);
        if (file == null) return BadRequest("The requested file does not exist. Please try again.");
        
        // fetch the appropriate group depending on name or id
        AccountGroup? group = null;
        if (!string.IsNullOrWhiteSpace(arguments.GroupName) || arguments.GroupId > 0)
        {
            group = await GetOrCreateGroup(arguments.GroupName, arguments.GroupId ?? 0);
        }

        var accountCount = _accountManager.Count();
        var settings = await _settingsLoader.Load();

        var processResults = new List<LineProcessResult>();
        try
        {
            if (accountCount >= settings.MaxManagedAccounts) return MaximumManagedAccounts(settings.MaxManagedAccounts);
            
            await using var stream = System.IO.File.OpenRead(file.ServerPath);
            var reader = new StreamReader(stream);
            var addedAccounts = new Dictionary<string, SnapchatAccountModel>();

            var lineNumber = 0;
            while (!reader.EndOfStream)
            {
                // Do not allow to add more accounts than we can manage
                if (addedAccounts.Count + accountCount >= settings.MaxManagedAccounts) break;
                var line = await reader.ReadLineAsync();

                lineNumber++;
                // We pass the group id here instead of the entity itself. Because it seems that when working with this
                // particular entity, we would have had to update it after each iteration to grab all accounts in the
                // group
                var result = await ProcessLine(line, addedAccounts, group?.Id ?? 0);
                result.LineNumber = lineNumber;

                if (result.Status != LineProcessStatus.Ok)
                {
                    processResults.Add(result);
                    continue;
                }

                processResults.Add(result);
                addedAccounts.Add(result.Account.Username, result.Account);
            }
        }
        finally
        {
            // We now delete the import file since we won't need it anymore
            await _uploadManager.DeleteFiles(new List<MediaFile>() { file });
        }

        return OkApi("", new UploadResult<LineProcessResult> { Results = processResults });
    }

    [HttpPost("purge")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purge()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var count = _accountManager.Count();
        foreach (var account in context.Accounts)
        {
            // Look for the corresponding email and remove it as well
            var email = context.Emails.Where(e => e.AccountId == account.Id);
            if (await email.AnyAsync())
                context.RemoveRange(email);
            context.Remove(account);
        }

        await context.SaveChangesAsync();
        
        return OkApi($"Accounts purged: {count}");
    }
    
    [HttpGet("cleanall")]
    public async Task<IActionResult> CleanAll()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var accounts = context.Accounts.Include(e => e.Proxy).Where(e => e.AccountStatus == AccountStatus.BANNED || e.AccountStatus == AccountStatus.RATE_LIMITED || e.AccountStatus == AccountStatus.LOCKED || e.AccountStatus == AccountStatus.NEEDS_CHECKED || e.UserId == null);
        
        var proxies = await context.Proxies.ToListAsync();
        var emails = await context.Emails.ToListAsync();
        var lines = accounts.Select(a => a.ToExportString(emails,proxies));
        var builder = new StringBuilder();
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));

        foreach (var account in accounts)
        {
            // Look for the corresponding email and remove it as well
            var email = context.Emails.Where(e => e.AccountId == account.Id);
            if (await email.AnyAsync())
                context.RemoveRange(email);
            context.Remove(account);
        }

        await context.SaveChangesAsync();
        
        return File(content, "plain/text", "accounts.txt");
    }
    
    [HttpPost("none")]
    public async Task<IActionResult> None()
    {
        return OkApi("Ty");
    }

    [HttpPost("total")]
    [ValidateAntiForgeryToken]
    public async Task<int> TotalAccounts()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        return context.Accounts.Count();
    }
    
    [HttpPost("acceptall")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptAll(AcceptFriendArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.AcceptFriend(arguments);

        return OkApi("Scheduled AcceptFriends Job", workRequest.Id);
    }
    
    [HttpPost("friendcleaner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FriendCleaner(FriendCleanerArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.FriendCleaner(arguments);

        return OkApi("Scheduled Friend Cleaner Job", workRequest.Id);
    }
    
    [HttpPost("quickadd")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAdd(QuickAddArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.QuickAdd(arguments);

        return OkApi("Scheduled QuickAdd Job", workRequest.Id);
    }
    
    [HttpPost("refreshall")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshAll(RefreshFriendArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.RefreshFriends(arguments);

        return OkApi("Scheduled RefreshFriends Job", workRequest.Id);
    }
    
    [HttpPost("exportfriends")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshAll(ExportFriendsArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.ExportFriends(arguments);

        return OkApi("Scheduled RefreshFriends Job", workRequest.Id);
    }
    
    [HttpPost("relogall")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RelogAccounts(RelogAccountArguments arguments)
    {
        var settings = await _settingsLoader.Load();
        
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi($"Arguments validation failed. {validationResult.Exception.Message}", null, ApiResponseCode.ArgumentsValidationFailed);

        var workRequest = await _scheduler.RelogAccounts(arguments);

        return OkApi("Scheduled RelogAccounts Job", workRequest.Id);
    }

    private IActionResult CreateExportFileResult(IEnumerable<SnapchatAccountModel> accounts, List<EmailModel> emails, List<Proxy> proxies)
    {
        var strings = accounts.Select(a => a.ToExportString(emails,proxies));
        
        var builder = new StringBuilder();
        foreach (var line in strings)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        return File(content, "plain/text", "accounts.txt");
    }
    
    [HttpPost("createbitmoji")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBitmoji(CreateBitmojiArguments arguments)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Bitmojis != null)
        {
            foreach (var bit in context.Bitmojis)
            {
                if (bit.Name.Contains(arguments.Name))
                {
                    return BadRequestApi("Bitmoji with this name already exists.");
                }
            }

            BitmojiModel bitmoji = new();

            bitmoji.Name = arguments.Name;
            bitmoji.Gender = arguments.Gender;
            bitmoji.Style = arguments.Style;
            bitmoji.Rotation = arguments.Rotation;
            bitmoji.Body = arguments.Body;
            bitmoji.Bottom = arguments.Bottom;
            bitmoji.BottomTone1 = arguments.BottomTone1;
            bitmoji.BottomTone2 = arguments.BottomTone2;
            bitmoji.BottomTone3 = arguments.BottomTone3;
            bitmoji.BottomTone4 = arguments.BottomTone4;
            bitmoji.BottomTone5 = arguments.BottomTone5;
            bitmoji.BottomTone6 = arguments.BottomTone6;
            bitmoji.BottomTone7 = arguments.BottomTone7;
            bitmoji.BottomTone8 = arguments.BottomTone8;
            bitmoji.BottomTone9 = arguments.BottomTone9;
            bitmoji.BottomTone10 = arguments.BottomTone10;
            bitmoji.Brow = arguments.Brow;
            bitmoji.ClothingType = arguments.ClothingType;
            bitmoji.Ear = arguments.Ear;
            bitmoji.Eye = arguments.Eye;
            bitmoji.Eyelash = arguments.Eyelash;
            bitmoji.FaceProportion = arguments.FaceProportion;
            bitmoji.Footwear = arguments.Footwear;
            bitmoji.FootwearTone1 = arguments.FootwearTone1;
            bitmoji.FootwearTone2 = arguments.FootwearTone2;
            bitmoji.FootwearTone3 = arguments.FootwearTone3;
            bitmoji.FootwearTone4 = arguments.FootwearTone4;
            bitmoji.FootwearTone5 = arguments.FootwearTone5;
            bitmoji.FootwearTone6 = arguments.FootwearTone6;
            bitmoji.FootwearTone7 = arguments.FootwearTone7;
            bitmoji.FootwearTone8 = arguments.FootwearTone8;
            bitmoji.FootwearTone9 = arguments.FootwearTone9;
            bitmoji.FootwearTone10 = arguments.FootwearTone10;
            bitmoji.Hair = arguments.Hair;
            bitmoji.HairTone = arguments.HairTone;
            bitmoji.IsTucked = arguments.IsTucked;
            bitmoji.Jaw = arguments.Jaw;
            bitmoji.Mouth = arguments.Mouth;
            bitmoji.Nose = arguments.Nose;
            bitmoji.Pupil = arguments.Pupil;
            bitmoji.PupilTone = arguments.PupilTone;
            bitmoji.SkinTone = arguments.SkinTone;
            bitmoji.Sock = arguments.Sock;
            bitmoji.SockTone1 = arguments.SockTone1;
            bitmoji.SockTone2 = arguments.SockTone2;
            bitmoji.SockTone3 = arguments.SockTone3;
            bitmoji.SockTone4 = arguments.SockTone4;
            bitmoji.Top = arguments.Top;
            bitmoji.TopTone1 = arguments.TopTone1;
            bitmoji.TopTone2 = arguments.TopTone2;
            bitmoji.TopTone3 = arguments.TopTone3;
            bitmoji.TopTone4 = arguments.TopTone4;
            bitmoji.TopTone5 = arguments.TopTone5;
            bitmoji.TopTone6 = arguments.TopTone6;
            bitmoji.TopTone7 = arguments.TopTone7;
            bitmoji.TopTone8 = arguments.TopTone8;
            bitmoji.TopTone9 = arguments.TopTone9;
            bitmoji.TopTone10 = arguments.TopTone10;

            await context.Bitmojis.AddAsync(bitmoji);
        }

        await context.SaveChangesAsync();

        return OkApi($"{arguments.Name} Bitmoji has been saved to the database.");
    }
    
    [HttpPost("purge_filtered")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeFiltered(PurgeAccountFilteredArguments arguments)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IQueryable<SnapchatAccountModel> targets = context.Accounts;
        int filterCount = 0;

        /*if (!arguments.OS.Equals("o"))
        {
            targets = targets.Where(t => t.OS != null && t.OS.Equals(arguments.OS == "0" ? SnapchatLib.OS.ios : SnapchatLib.OS.android));
        }*/
        
        if (!arguments.emailvalidation.Equals("o"))
        {
            targets = targets.Where(t => t.EmailValidated != null && t.EmailValidated.Equals((ValidationStatus)(Convert.ToInt32(arguments.emailvalidation))));
        }
        
        if (!arguments.phonevalidation.Equals("o"))
        {
            targets = targets.Where(t => t.PhoneValidated != null && t.PhoneValidated.Equals((ValidationStatus)(Convert.ToInt32(arguments.phonevalidation))));
        }
        
        if (!arguments.status.Equals("o"))
        {
            targets = targets.Where(t => t.AccountStatus != null && t.AccountStatus.Equals((AccountStatus)Convert.ToInt32(arguments.status)));
        }
        
        if (!arguments.hasadded.Equals("o"))
        {
            targets = targets.Where(t => t.hasAdded != null && t.hasAdded.Equals(Convert.ToBoolean(arguments.hasadded)));
        }
        filterCount = targets.Count();
        
        foreach (var account in targets)
        {
            // Look for the corresponding email and remove it as well
            var email = context.Emails.Where(e => e.AccountId == account.Id);
            if (await email.AnyAsync())
                context.RemoveRange(email);
            context.Remove(account);
        }
        
        await context.SaveChangesAsync();

        return OkApi($"{filterCount} Filtered accounts purged");
    }
    
    [HttpGet("export_filtered/{emailvalidation}/{phonevalidation}/{status}/{hasadded}")]
    public async Task<IActionResult> Export(string emailvalidation, string phonevalidation, string status, string hasadded)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IQueryable<SnapchatAccountModel> accounts = context.Accounts;
        int filterCount = 0;

        /*if (!OS.Equals("o"))
        {
            accounts = accounts.Where(t => t.OS != null && t.OS.Equals(OS == "0" ? SnapchatLib.OS.ios : SnapchatLib.OS.android));
        }*/
        
        if (!emailvalidation.Equals("o"))
        {
            accounts = accounts.Where(t => t.EmailValidated != null && t.EmailValidated.Equals((ValidationStatus)(Convert.ToInt32(emailvalidation))));
        }
        
        if (!phonevalidation.Equals("o"))
        {
            accounts = accounts.Where(t => t.PhoneValidated != null && t.PhoneValidated.Equals((ValidationStatus)(Convert.ToInt32(phonevalidation))));
        }
        
        if (!status.Equals("o"))
        {
            accounts = accounts.Where(t => t.AccountStatus != null && t.AccountStatus.Equals((AccountStatus)Convert.ToInt32(status)));
        }
        
        if (!hasadded.Equals("o"))
        {
            accounts = accounts.Where(t => t.hasAdded != null && t.hasAdded.Equals(Convert.ToBoolean(hasadded)));
        }
        
        var emails = await context.Emails.ToListAsync();
        var proxies = await context.Proxies.ToListAsync();
        var lines = accounts.Select(a => a.ToExportString(emails,proxies));
        var builder = new StringBuilder();
        
        foreach (var line in lines)
            builder.AppendLine(line);

        var content = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
        

        return File(content, "plain/text", "accounts.txt");
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dataSet = context.Accounts.Include(e => e.Proxy);
        var emails = await context.Emails.ToListAsync();
        var proxies = await context.Proxies.ToListAsync();

        return CreateExportFileResult(dataSet, emails, proxies);
    }

    [HttpGet("export/maxfriendsonly")]
    public async Task<IActionResult> ExportWithMaxFriendsOnly()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dataSet = context.Accounts.Where(a => a.FriendCount >= AppSettings.MaxFriends).Include(e => e.Proxy);
        var emails = await context.Emails.ToListAsync();
        var proxies = await context.Proxies.ToListAsync();
        
        return CreateExportFileResult(dataSet, emails, proxies);
    }

    [HttpGet("groups/{id}")]
    public async Task<IActionResult> GetAccountGroups(long id)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        if (account == null) return NotFound("Account with id {id} could not be found");
        await context.Entry(account).Collection(a => a.Groups).LoadAsync();
        
        // for some reason, just returning the groups yielded errors, so we give back the whole account instead...
        return OkApi(data: account);
    }

    [HttpPost("groups/add")]
    public async Task<IActionResult> AddAccountToGroup(AddAccountToGroupArguments args)
    {
        if (args.AccountId == 0 || args.GroupId == 0) return BadRequestApi("Invalid AccountId or GroupId");
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(args.AccountId);
        var group = await context.AccountGroups.FindAsync(args.GroupId);

        if (account == null) return NotFound($"Account with id {args.AccountId} not found");
        if (group == null) return NotFound($"Group with id {args.GroupId} not found");

        await context.Entry(account).Collection(e => e.Groups).LoadAsync();
        await context.Entry(group).Collection(e => e.Accounts).LoadAsync();

        if (account.Groups.Any(g => g.Id == args.GroupId))
            return BadRequestApi($"User is already part of group {group.Name}");
        
        if (account.Groups == null)
        {
            account.Groups = new List<AccountGroup>() { group };
        }
        else
        {
            account.Groups.Add(group);
        }

        await context.SaveChangesAsync();

        return OkApi(data: group);
    }
    
    [HttpPost("groups/remove")]
    public async Task<IActionResult> RemoveAccountFromGroup(RemoveAccountFromGroupArguments args)
    {
        if (args.AccountId == 0 || args.GroupId == 0) return BadRequestApi("Invalid AccountId or GroupId");
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(args.AccountId);

        if (account == null) return NotFound($"Account with id {args.AccountId} not found");

        await context.Entry(account).Collection(e => e.Groups).LoadAsync();

        var match = account.Groups.FirstOrDefault(g => g.Id == args.GroupId); 
        if (match == null)
            return BadRequestApi($"Could not find the indicated group");

        account.Groups.Remove(match);

        await context.SaveChangesAsync();

        return OkApi();
    }
}