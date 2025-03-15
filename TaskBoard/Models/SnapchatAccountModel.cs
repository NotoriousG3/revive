using System.ComponentModel.DataAnnotations.Schema;
using SnapchatLib;
using SnapProto.Snapchat.Search;
using SnapProto.Snapchat.Storydoc;
using TaskBoard.Controllers;

namespace TaskBoard.Models;

// If updating this, make sure to update ValidationStatus in accountmanager.js
public enum ValidationStatus
{
    NotValidated = 0,
    Validated = 1,
    FailedValidation = 2,
    PartiallyValidated = 3
}

public enum AccountStatus
{
    OKAY = 0,
    BAD_PROXY = 1,
    NEEDS_RELOG = 2,
    RATE_LIMITED = 3,
    LOCKED = 4,
    BANNED = 5,
    NEEDS_FRIEND_REFRESH = 6,
    BUSY = 7,
    NEEDS_CHECKED = 8
}

public class SnapchatAccountModel
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Device { get; set; } = null!;
    public string? Install { get; set; } = null!;
    public string? UserId { get; set; } = null!;
    public string? AuthToken { get; set; } = null!;
    public string? DToken1I { get; set; } = null!;
    public string? DToken1V { get; set; } = null!;
    public long InstallTime { get; set; }
    public OS OS { get; set; }
    public SnapchatVersion SnapchatVersion { get; set; }
    public DateTime? CreationDate { get; set; } = null;
    [ForeignKey("Proxy")]
    public int? ProxyId { get; set; }
    public Proxy? Proxy { get; set; }
    public AccountStatus AccountStatus { get; set; }

    public string GetAccountStatus
    {
        get
        {
            switch (AccountStatus)
            {
                case AccountStatus.OKAY:
                    return "OKAY";
                case AccountStatus.BUSY:
                    return "BUSY";
                case AccountStatus.BANNED:
                    return "BANNED";
                case AccountStatus.LOCKED :
                    return "LOCKED";
                case AccountStatus.BAD_PROXY:
                    return "BAD PROXY";
                case AccountStatus.NEEDS_RELOG:
                    return "NEEDS RELOG";
                case AccountStatus.RATE_LIMITED:
                    return "RATE LIMITED";
                case AccountStatus.NEEDS_CHECKED:
                    return "NEEDS CHECKED";
                case AccountStatus.NEEDS_FRIEND_REFRESH:
                    return "NEEDS FRIEND REFRESH";
                default:
                    return "Unknown";
            }
        }
    }

    public int FriendCount { get; set; }
    public int IncomingFriendCount { get; set; }
    public int OutgoingFriendCount { get; set; }
    public string? DeviceProfile { get; set; }
    public string? AccessToken { get; set; } = null!;
    public string? BusinessAccessToken { get; set; } = null!;
    public string? AccountCountryCode { get; set; } = null!;
    public SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign Horoscope { get; set; }
    public string? TimeZone { get; set; } = null!;
    public string? ClientID { get; set; } = null!;
    public string? refreshToken { get; set; } = null!;
    public int Age { get; set; }
    public virtual ICollection<AccountGroup> Groups { get; set; }
    public ValidationStatus PhoneValidated { get; set; }
    public ValidationStatus EmailValidated { get; set; }
    public ValidationStatus ValidationStatus
    {
        get
        {
            if (PhoneValidated == ValidationStatus.Validated && EmailValidated == ValidationStatus.Validated)
                return ValidationStatus.Validated;

            if (PhoneValidated == ValidationStatus.NotValidated && EmailValidated == ValidationStatus.NotValidated)
                return ValidationStatus.NotValidated;

            if (PhoneValidated == ValidationStatus.FailedValidation || EmailValidated == ValidationStatus.FailedValidation)
                return ValidationStatus.FailedValidation;
            
            return ValidationStatus.PartiallyValidated;
        }
    }
    public bool hasAdded { get; set; }
    
    [NotMapped]
    public bool WasModified { get; set; }
    [NotMapped]
    public ISnapchatClient SnapClient { get; set; } = null!;

    public void SetStatus(SnapchatAccountManager manager, AccountStatus status)
    {
        AccountStatus = status;
        
        switch (status)
        {
            case AccountStatus.BUSY:
                break;
            case AccountStatus.NEEDS_CHECKED:
                break;
            case AccountStatus.OKAY:
                break;
            case AccountStatus.BANNED:
                break;
            case AccountStatus.LOCKED:
                break;
            case AccountStatus.NEEDS_RELOG:
                break;
            case AccountStatus.RATE_LIMITED:
                break;
            case AccountStatus.BAD_PROXY:
                break;
            case AccountStatus.NEEDS_FRIEND_REFRESH:
                break;
        }

        manager.UpdateAccount(this).Wait();
    }
    
    public string ToExportString(IEnumerable<EmailModel> emails, IEnumerable<Proxy> proxies)
    {
        var values = new List<string>();
        var proxy = proxies.FirstOrDefault(e => e.Accounts.Contains(this));
        var email = emails.FirstOrDefault(e => e.Account == this);
        foreach (AccountImportField v in Enum.GetValues(typeof(AccountImportField)))
        {
            switch (v)
            {
                case AccountImportField.Device:
                    values.Add(Device);
                    break;
                case AccountImportField.Email:
                    values.Add(email != null ? email.Address : "");
                    break;
                case AccountImportField.Install:
                    values.Add(Install);
                    break;
                case AccountImportField.Os:
                    values.Add(OS.ToString());
                    break;
                case AccountImportField.Password:
                    values.Add(Password);
                    break;
                case AccountImportField.Username:
                    values.Add(Username);
                    break;
                case AccountImportField.AuthToken:
                    values.Add(AuthToken);
                    break;
                case AccountImportField.DeviceProfile:
                    values.Add(DeviceProfile);
                    break;
                case AccountImportField.DToken1i:
                    values.Add(DToken1I);
                    break;
                case AccountImportField.DToken1v:
                    values.Add(DToken1V);
                    break;
                case AccountImportField.InstallTime:
                    values.Add(InstallTime.ToString());
                    break;
                case AccountImportField.ProxyAddress:
                    values.Add(proxy == null ? "" : proxy.Address.ToString());
                    break;
                case AccountImportField.ProxyPassword:
                    values.Add(proxy == null ? "" : proxy.Password);
                    break;
                case AccountImportField.ProxyUser:
                    values.Add(proxy == null ? "" : proxy.User);
                    break;
                case AccountImportField.SnapchatVersion:
                    values.Add(SnapchatVersion.ToString());
                    break;
                case AccountImportField.UserId:
                    values.Add(UserId);
                    break;
                case AccountImportField.AccessToken:
                    values.Add(AccessToken);
                    break;
                case AccountImportField.BusinessToken:
                    values.Add(BusinessAccessToken);
                    break;
                case AccountImportField.AccountCountryCode:
                    values.Add(AccountCountryCode);
                    break;
                case AccountImportField.Horoscope:
                    values.Add(Horoscope.ToString());
                    break;
                case AccountImportField.TimeZone:
                    values.Add(TimeZone);
                    break;
                case AccountImportField.ClientID:
                    values.Add(ClientID);
                    break;
                case AccountImportField.Age:
                    values.Add(Age.ToString());
                    break;
                case AccountImportField.refreshToken:
                    values.Add(refreshToken);
                    break;
            }
        }

        return string.Join('*', values);
    }
}