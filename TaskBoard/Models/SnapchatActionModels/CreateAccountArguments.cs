using Newtonsoft.Json;
using SnapchatLib;

namespace TaskBoard.Models.SnapchatActionModels;

// This needs to be in sync with the radio buttons in AccountTools.cshtml
public enum AccountOSSelection
{
    None = 0,
    Android = 2,
    Random = 3
}

public enum PhoneVerificationService
{
    None = 0,
    FiveSim = 1,
    Twilio = 2,
    TextVerified = 3,
    SMSActivate = 4,
    SmsPool = 5
}

public enum EmailVerificationService
{
    None = 0,
    Provided = 1,
    Kopeechka = 2
}

public enum NameCreationService
{
    Random = 0,
    Manager = 1
}

public enum UserNameCreationService
{
    Random = 0,
    Manager = 1
}

public enum Gender
{
    Random,
    Male,
    Female
}

public enum BitmojiSelection
{
    None,
    Random
}

public class CreateAccountArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public AccountOSSelection OSSelection { get; set; }
    public BitmojiSelection BitmojiSelection { get; set; }
    public int CustomBitmojiSelection { get; set; }
    public PhoneVerificationService PhoneVerificationService { get; set; }
    public EmailVerificationService EmailVerificationService { get; set; }
    public SnapchatVersion SnapchatVersion { get; set; }
    public string? CountryISO { get; set; }
    public Gender Gender { get; set; }
    public string? FirstName { get; set; } 
    public string? LastName { get; set; }
    public NameCreationService NameCreationService { get; set; }
    public UserNameCreationService UserNameCreationService { get; set; }
    public int? BoostScore { get; set; }
    public string? CustomPassword { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            /*if (OSSelection == AccountOSSelection.None)
                throw new ArgumentException(
                    "No OS is available for account generation");*/
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(CreateAccountArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator CreateAccountArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<CreateAccountArguments>(arguments)!;
    }
}