using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ViewPublicStoryArguments : ActionArguments
{
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

    public static implicit operator string(ViewPublicStoryArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ViewPublicStoryArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ViewPublicStoryArguments>(arguments)!;
    }
}