using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class FindUsersViaSearchArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string Keyword { get; set; }
    public int ActionsPerAccount { get; set; }
    public bool OnlyActive { get; set; }
    public int SearchDelay { get; set; }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            CheckKeywords(Keyword);

            if (SearchDelay < 5)
            {
                throw new ArgumentException("Search delay must be a minimum of 5 seconds.");
            }
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(FindUsersViaSearchArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator FindUsersViaSearchArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<FindUsersViaSearchArguments>(arguments)!;
    }
}