using System.Collections.Generic;

namespace SnapchatLib.Models
{
    public class Device
    {
        public string out_beta { get; set; }
        public int version { get; set; }
    }

    public class ExtraFriendmojiMutableDict
    {
    }

    public class ExtraFriendmojiReadOnlyDict
    {
    }

    public class FideliusInfo
    {
        public List<Device> devices { get; set; }
    }

    public class FriendRequestObject
    {
        public string birthday { get; set; }
        public int cameos_sharing_policy { get; set; }
        public bool can_see_custom_stories { get; set; }
        public string direction { get; set; }
        public string display { get; set; }
        public int expiration { get; set; }
        public FideliusInfo fidelius_info { get; set; }
        public string friendmoji_string { get; set; }
        public List<object> friendmojis { get; set; }
        public bool is_cameos_sharing_supported { get; set; }
        public bool is_popular { get; set; }
        public bool is_story_muted { get; set; }
        public string mutable_username { get; set; }
        public string name { get; set; }
        public int plus_badge_visibility { get; set; }
        public int snap_streak_count { get; set; }
        public int type { get; set; }
        public string user_id { get; set; }
    }

    public class FriendRequestJson
    {
        public ExtraFriendmojiMutableDict extra_friendmoji_mutable_dict { get; set; }
        public ExtraFriendmojiReadOnlyDict extra_friendmoji_read_only_dict { get; set; }
        public bool logged { get; set; }
        public string message { get; set; }
        public FriendRequestObject FriendRequestObject { get; set; }
    }

}
