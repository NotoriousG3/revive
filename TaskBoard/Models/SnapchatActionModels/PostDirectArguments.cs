using Newtonsoft.Json;
using NuGet.Protocol.Plugins;

namespace TaskBoard.Models.SnapchatActionModels;

public class PostDirectArguments : MessagingArguments
{
    public PostDirectSingleSnap[] Snaps;
    public PostDirectArguments() {}
    
    public PostDirectArguments(PostDirectArguments arguments)
    {
        Users = arguments.Users;
        RandomUsers = arguments.RandomUsers;
        RandomTargetAmount = arguments.RandomTargetAmount;
        FriendsOnly = arguments.FriendsOnly;
        CountryFilter = arguments.CountryFilter;
        GenderFilter = arguments.GenderFilter;
        RaceFilter = arguments.RaceFilter;
        Snaps = arguments.Snaps;
    }

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();

            if (Snaps.Length == 0) throw new ArgumentException("You need to set at least 1 snap to send");

            if (RandomUsers && FriendsOnly) throw new ArgumentException("Can not post to FriendsOnly and RandomTargets in one job.");

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

    public static implicit operator string(PostDirectArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator PostDirectArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<PostDirectArguments>(arguments)!;
    }
}