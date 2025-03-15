namespace SnapchatLib.REST.Models;

public class IdentityProfilePage
{
    public IdentityProfilePage(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IdentityProfilePage Profile => new("profile");
    public static IdentityProfilePage ContextCard => new("context_card");
    public static IdentityProfilePage Group => new("group_profile");
    public static IdentityProfilePage QuickAddListOnFriendsFeed => new("quick_add_list_on_friends_feed");
    public static IdentityProfilePage QuickAddCarouselOnProfile => new("quick_add_carousel_on_profile");
    public static IdentityProfilePage Search => new("search");
    public static IdentityProfilePage Contacts => new("contacts");
    public static IdentityProfilePage AddFriendsTabNgs => new("add_friends_tab_ngs");
    public static IdentityProfilePage Snapcode => new("scan_snapcode");
    public static IdentityProfilePage SubscriptionUserStoryOnDiscoverFeed => new("subscription_user_story_on_discover_feed");
    public static IdentityProfilePage AddFriendsFromCanvasActionMenu => new("add_friends_from_canvas_action_menu");
    public static IdentityProfilePage AddFriendsCtaButtonOnFriendsFeed => new("add_friends_cta_button_on_friends_feed");
    public static IdentityProfilePage AddFriendsCtaButtonOnDiscoverFeed => new("add_friends_cta_button_on_discover_feed");
    public static IdentityProfilePage AddFriendsByUsernamePage => new("PROFILE_ADD_FRIENDS_BY_USERNAME_PAGE");
}