
namespace SnapchatLib.REST.Models;

public class FriendRequestType
{
    public FriendRequestType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static FriendRequestType ByPhone => new("ADDED_BY_PHONE");
    public static FriendRequestType ByUsername => new("ADDED_BY_USERNAME");
    public static FriendRequestType ByDisplayName => new("ADDED_BY_DISPLAY_NAME");
    public static FriendRequestType ByQRCode => new("ADDED_BY_QR_CODE");
    public static FriendRequestType ByAddedMeBack => new("ADDED_BY_ADDED_ME_BACK");
    public static FriendRequestType ByNearby => new("ADDED_BY_NEARBY");
    public static FriendRequestType BySuggested => new("ADDED_BY_SUGGESTED");
    public static FriendRequestType ByStoryChrome => new("ADDED_BY_STORY_CHROME");
    public static FriendRequestType BySharedUsername => new("ADDED_BY_SHARED_USERNAME");
    public static FriendRequestType BySharedStory => new("ADDED_BY_SHARED_STORY");
    public static FriendRequestType ByGroupChat => new("ADDED_BY_GROUP_CHAT");
    public static FriendRequestType ByMention => new("ADDED_BY_MENTION_STICKER");
    public static FriendRequestType BySnapcode => new("ADDED_BY_SNAPCODE_STICKER");
    public static FriendRequestType FromPublicProfile => new("ADDED_FROM_PUBLIC_PROFILE");
}