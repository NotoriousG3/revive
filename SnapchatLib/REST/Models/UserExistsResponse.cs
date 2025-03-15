namespace SnapchatLib.REST.Models;

public class UserExistsResponse
{
    public bool exists { get; set; }
    public UserExistsFriend FriendJson { get; set; }
}

public class UserExistsFriend
{
    public string display { get; set; }
    public int expiration { get; set; }
    public bool is_popular { get; set; }
    public string name { get; set; }
    public int reverse_ts { get; set; }
    public string story_privacy { get; set; }
    public int ts { get; set; }
    public string user_id { get; set; }
}