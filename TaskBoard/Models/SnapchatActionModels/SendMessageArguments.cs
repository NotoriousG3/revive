using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class SendMessageArguments : MessagingArguments
{
    public SendMessageSingleMessage[] Messages;
    public bool EnableMacros { get; set; }

    public SendMessageArguments() {}
    public SendMessageArguments(SendMessageArguments arguments)
    {
        Users = arguments.Users;
        RandomUsers = arguments.RandomUsers;
        RandomTargetAmount = arguments.RandomTargetAmount;
        FriendsOnly = arguments.FriendsOnly;
        CountryFilter = arguments.CountryFilter;
        GenderFilter = arguments.GenderFilter;
        RaceFilter = arguments.RaceFilter;
        Messages = arguments.Messages;
        EnableMacros = arguments.EnableMacros;
    }
    
    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            if (Messages.Length == 0) throw new ArgumentException("You need to set at least 1 message to send");
            
            if(RandomUsers && FriendsOnly) throw new ArgumentException("Can not message FriendsOnly and RandomTargets in one job.");
            
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

    public static implicit operator string(SendMessageArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator SendMessageArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<SendMessageArguments>(arguments)!;
    }
}