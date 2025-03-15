using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class PhoneSearchArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    
    public string Randomizer { get; set; }
    public string Number { get; set; }
    public string CountryCode { get; set; }
    public int ActionsPerAccount { get; set; }
    public bool OnlyActive { get; set; }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            CheckPhoneNumber(Number, CountryCode);
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(PhoneSearchArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator PhoneSearchArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<PhoneSearchArguments>(arguments)!;
    }
}