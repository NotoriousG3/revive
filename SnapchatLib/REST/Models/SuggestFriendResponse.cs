using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class SuggestedFriendResultsV2
{
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_background_id { get; set; }
    public string bitmoji_scene_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public string display_name { get; set; }
    public string mutable_username { get; set; }
    public string story_privacy { get; set; }
    public string userId { get; set; }
    public string username { get; set; }
}


public class AddFriendsFooterOrdering
{
    public string suggestion_subtext { get; set; }
    public string suggestion_subtext_lowercase { get; set; }
    public string suggestion_token { get; set; }
    public string userId { get; set; }
}

public class FullPageOrdering
{
    public string suggestion_subtext { get; set; }
    public string suggestion_subtext_lowercase { get; set; }
    public string suggestion_token { get; set; }
    public string userId { get; set; }
}

public class suggest_friend_high_availability
{
    public List<AddFriendsFooterOrdering> add_friends_footer_ordering { get; set; }
    public int badging_end_index { get; set; }
    public int badging_start_index { get; set; }
    public int discover_carousel_client_impression { get; set; }
    public string discover_carousel_style { get; set; }
    public List<object> feed_page_ordering { get; set; }
    public List<object> friends_horizontal_page_ordering { get; set; }
    public List<object> friends_view_all_page_ordering { get; set; }
    public List<FullPageOrdering> full_page_ordering { get; set; }
    public List<SearchPageOrdering> search_page_ordering { get; set; }
    public List<SearchResultPageOrdering> search_result_page_ordering { get; set; }
    public List<object> send_to_page_ordering { get; set; }
    public List<object> stories_page_ordering { get; set; }
    public List<object> stories_view_all_page_ordering { get; set; }
    public List<SuggestedFriendResultsV2> suggested_friend_results_v2 { get; set; }
    public List<object> suggestions_with_active_story_ordering { get; set; }
}

public class SearchPageOrdering
{
    public string suggestion_subtext { get; set; }
    public string suggestion_subtext_lowercase { get; set; }
    public string suggestion_token { get; set; }
    public string userId { get; set; }
}

public class SearchResultPageOrdering
{
    public string suggestion_subtext { get; set; }
    public string suggestion_subtext_lowercase { get; set; }
    public string suggestion_token { get; set; }
    public string userId { get; set; }
}