using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ReportUserPublicProfileRandomArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string Username { get; set; } = null!;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

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

    public static implicit operator string(ReportUserPublicProfileRandomArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ReportUserPublicProfileRandomArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ReportUserPublicProfileRandomArguments>(arguments)!;
    }
}