using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class FriendStories
{
    public bool mature_content { get; set; }
    public int new_story_count { get; set; }
    public List<object> stories { get; set; }
    public string type { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
    public string deep_link_url { get; set; }
}