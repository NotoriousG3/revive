using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ChangeUsernameArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string? AccID { get; set; }
    public string? OldName { get; set; }
    public string? NewName { get; set; }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(ChangeUsernameArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ChangeUsernameArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ChangeUsernameArguments>(arguments)!;
    }
}