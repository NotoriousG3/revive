using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class EmailSearchArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string Address { get; set; }
    public int ActionsPerAccount { get; set; }
    public bool OnlyActive { get; set; }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            CheckEmail(Address);
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(EmailSearchArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator EmailSearchArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<EmailSearchArguments>(arguments)!;
    }
}