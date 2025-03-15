using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class Result
{
    public string name { get; set; }
    public string user_id { get; set; }
    public int type { get; set; }
    public string display { get; set; }
    public int ts { get; set; }
    public int reverse_ts { get; set; }
    public int expiration { get; set; }
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public bool is_new_contact { get; set; }
    public bool is_recommended { get; set; }
    public int recommendation_score { get; set; }
}

public class FindFriendsJson
{
    public List<Result> results { get; set; }
    public long last_address_book_updated_date { get; set; }
    public bool is_trimmed { get; set; }
    public string content_suggestion_page_position { get; set; }
}