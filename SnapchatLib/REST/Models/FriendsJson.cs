using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class FriendsJson
{
    public List<AddedFriend> added_friends { get; set; }
    public string added_friends_sync_token { get; set; }
    public string added_friends_sync_type { get; set; }
    public List<object> bests { get; set; }
    public List<object> bests_user_ids { get; set; }
    public List<Friend> friends { get; set; }
    public string friends_sync_token { get; set; }
    public string friends_sync_type { get; set; }
    public bool is_response_with_partial_columns { get; set; }
}