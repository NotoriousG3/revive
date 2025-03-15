using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class SendMentionArguments : MessagingArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public string User { get; set; } = null!;

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            if (string.IsNullOrWhiteSpace(User)) throw new ArgumentException("User must not be null nor whitespace");
            
            if(RandomUsers && FriendsOnly) throw new ArgumentException("Can not mention FriendsOnly and RandomTargets in one job.");
            
            if ((RandomUsers || FriendsOnly) && Users.Any())
            {
                throw new ArgumentException("If you're posting to random users or friends you must leave targets blank.");
            }

            if (RandomUsers && (RandomTargetAmount > 828 || RandomTargetAmount < 1))
            {
                throw new ArgumentException("You can only target between 1-828 random targets.");
            }

            if(!RandomUsers && !FriendsOnly)
            {
                CheckUserList(Users);
            }

            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(SendMentionArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator SendMentionArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<SendMentionArguments>(arguments)!;
    }
}