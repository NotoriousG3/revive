using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ReportUserRandomArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string Username { get; set; } = null!;

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            CheckUsername(Username);
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(ReportUserRandomArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ReportUserRandomArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ReportUserRandomArguments>(arguments)!;
    }
}