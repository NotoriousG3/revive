using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ViewBusinessPublicStoryArguments : ActionArguments
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
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(ViewBusinessPublicStoryArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ViewBusinessPublicStoryArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ViewBusinessPublicStoryArguments>(arguments)!;
    }
}