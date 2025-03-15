using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class SyncResponse
{
    public class AddedFriend
    {
        public string add_source { get; set; }
        public string add_source_type { get; set; }
        public int cameos_sharing_policy { get; set; }
        public string direction { get; set; }
        public string display { get; set; }
        public int expiration { get; set; }
        public bool is_incoming_friend_request_viewed { get; set; }
        public string mutable_username { get; set; }
        public string name { get; set; }
        public int reverse_ts { get; set; }
        public string snapshot_metadata { get; set; }
        public object ts { get; set; }
        public int type { get; set; }
        public string user_id { get; set; }
        public string bitmoji_avatar_id { get; set; }
        public string bitmoji_selfie_id { get; set; }
        public string snap_pro_id { get; set; }
        public bool? ignored_link { get; set; }
    }

    public class Device
    {
        public string out_beta { get; set; }
        public int version { get; set; }
    }

    public class ExtraFriendmojiReadOnlyDict
    {
    }

    public class FideliusInfo
    {
        public List<Device> devices { get; set; }
    }

    public class Friend
    {
        public string birthday { get; set; }
        public string bitmoji_avatar_id { get; set; }
        public string bitmoji_background_id { get; set; }
        public string bitmoji_scene_id { get; set; }
        public string bitmoji_selfie_id { get; set; }
        public int cameos_sharing_policy { get; set; }
        public bool can_see_custom_stories { get; set; }
        public string direction { get; set; }
        public string display { get; set; }
        public int expiration { get; set; }
        public FideliusInfo fidelius_info { get; set; }
        public string friendmoji_string { get; set; }
        public List<Friendmoji> friendmojis { get; set; }
        public bool is_bitmoji_friendmoji_sharing_supported { get; set; }
        public bool is_cameos_sharing_supported { get; set; }
        public bool is_popular { get; set; }
        public bool is_story_muted { get; set; }
        public string mutable_username { get; set; }
        public string name { get; set; }
        public int plus_badge_visibility { get; set; }
        public object reverse_ts { get; set; }
        public int snap_streak_count { get; set; }
        public object ts { get; set; }
        public int type { get; set; }
        public string user_id { get; set; }
        public string snap_pro_id { get; set; }
        public string post_view_emoji { get; set; }
    }

    public class Friendmoji
    {
        public string category_name { get; set; }
    }

    public class ami_friends
    {
        public List<AddedFriend> added_friends { get; set; }
        public string added_friends_sync_token { get; set; }
        public string added_friends_sync_type { get; set; }
        public List<object> bests { get; set; }
        public List<object> bests_user_ids { get; set; }
        public ExtraFriendmojiMutableDict extra_friendmoji_mutable_dict { get; set; }
        public ExtraFriendmojiReadOnlyDict extra_friendmoji_read_only_dict { get; set; }
        public List<Friend> friends { get; set; }
        public string friends_sync_token { get; set; }
        public string friends_sync_type { get; set; }
        public List<object> invited_users { get; set; }
        public bool is_response_with_partial_columns { get; set; }
    }

}
