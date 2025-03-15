using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class AddFriendArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public List<string> Users { get; set; } = new ();
    public int FriendsPerAccount { get; set; }
    public int AddDelay { get; set; }
    public bool RandomUsers { get; set; }
    public string CountryFilter { get; set; }
    public string GenderFilter { get; set; }
    public string RaceFilter { get; set; }

    public new ValidationResult Validate()
    {
        try
        {
            base.Validate();

            if (RandomUsers && Users.Any())
            {
                throw new ArgumentException("If you're adding random users you must leave targets blank.");
            }
            
            if (RandomUsers && (FriendsPerAccount > 828 || FriendsPerAccount < 1))
            {
                throw new ArgumentException("You can only target between 1-828 targets.");
            }
            
            if (AddDelay > 300 || AddDelay < 5)
            {
                throw new ArgumentException("You can only add a delay between 5-300 seconds.");
            }
            
            if(!RandomUsers)
            {
                CheckUserList(Users);
            }
            
            if (FriendsPerAccount < 1) throw new ArgumentException("Friends per account must be higher than 0");

            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(AddFriendArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator AddFriendArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<AddFriendArguments>(arguments)!;
    }
}