using System.ComponentModel.DataAnnotations;

namespace SnapWebModels;

// NEVER CHANGE THE NUMBERS OF THIS ENUM. DOING SO MEANS USAGES OF THEM IN THE DB WILL BE BROKEN
public enum SnapWebModuleId
{
    SendMessage = 1, // Free
    PostDirect = 2, // Free
    AddFriend = 3, // Free
    Subscribe = 4, // Not-public
    ReportUserPublicProfileRandom = 5, // Not-public
    ReportUserRandom = 6, // Not-public
    ViewBusinessPublicStory = 7, // Not-public
    Test = 8, // Not-public
    AndroidOs = 9, // Paid
    ExtraAccounts100 = 10, // Paid
    SnapwebAccess = 11, // Paid
    ExportAccount = 12,
    PostStory = 13, // Paid
    Ios = 14, // Free
    ExtraStorage1 = 15, // Paid
    ExtraStorage2 = 16, // Paid
    ExtraStorage3 = 17, // Paid
    FindUsersViaSearch = 18, // Paid
    ExtraConcurrencySlot = 19, // Paid
    EmailScraper = 20,
    PhoneScraper = 21,
    ExtraWorkSlot = 22, // Paid
    AcceptFriend = 23,
    RefreshFriends = 24,
    QuickAdd = 25,
    ProxyScraper = 26,
    Analytics = 27,
    FriendCleaner = 28,
    ViewPublicStory = 29, // Not-public
    ReportUserStoryRandom = 30, // Not-public
    MacroManager = 31,
    ProxyChecking = 32,
    RelogAccounts = 33,
    ExportFriends = 34,
    BitmojiCreator = 35
}

// Used for special category-specific functionality in UI
public enum SnapWebModuleCategory
{
    Access = 0, // These are modules that modify some setting in the user's settings like more accounts, access, storage and such
    Functionality = 1, // These are modules that unlock some functionality on the website
    Action = 2 // Modules that unlock a specific Snapchatlib command/api
}

public class SnapWebModule
{
    public static SnapWebModule SendMessage = new() {Id = SnapWebModuleId.SendMessage, Enabled = false, Purchaseable = true, Price = 100, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule PostDirect = new() {Id = SnapWebModuleId.PostDirect, Enabled = true, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule AddFriend = new() {Id = SnapWebModuleId.AddFriend, Enabled = true, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule Subscribe = new() {Id = SnapWebModuleId.Subscribe, Enabled = false, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule ReportUserPublicProfileRandom = new() {Id = SnapWebModuleId.ReportUserPublicProfileRandom, Enabled = false, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule ReportUserRandom = new() {Id = SnapWebModuleId.ReportUserRandom, Enabled = false, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule ViewBusinessPublicStory = new() {Id = SnapWebModuleId.ViewBusinessPublicStory, Enabled = false, Purchaseable = false, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule Test = new() {Id = SnapWebModuleId.Test, Enabled = false, Purchaseable = false, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule AndroidOs = new() {Id = SnapWebModuleId.AndroidOs, Enabled = true, Purchaseable = true, Price = 750, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ExtraAccounts100 = new() {Id = SnapWebModuleId.ExtraAccounts100, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule SnapwebAccess = new() {Id = SnapWebModuleId.SnapwebAccess, Enabled = true, Purchaseable = true, Price = 1000, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule ExportAccount = new() {Id = SnapWebModuleId.ExportAccount, Enabled = true, Purchaseable = false, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule PostStory = new() {Id = SnapWebModuleId.PostStory, Enabled = false, Purchaseable = true, Category = SnapWebModuleCategory.Action };
    public static SnapWebModule Ios = new() {Id = SnapWebModuleId.Ios, Enabled = false, Purchaseable = true, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ExtraStorage1 = new() {Id = SnapWebModuleId.ExtraStorage1, Enabled = false, Purchaseable = true, Price = 3, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule ExtraStorage2 = new() {Id = SnapWebModuleId.ExtraStorage2, Enabled = false, Purchaseable = false, Price = 100000, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule ExtraStorage3 = new() {Id = SnapWebModuleId.ExtraStorage3, Enabled = false, Purchaseable = false, Price = 100000, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule FindUsersViaSearch = new() {Id = SnapWebModuleId.FindUsersViaSearch, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ExtraConcurrencySlot = new() {Id = SnapWebModuleId.ExtraConcurrencySlot, Enabled = false, Purchaseable = true, Price = 100000, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule EmailScraper = new() {Id = SnapWebModuleId.EmailScraper, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule PhoneScraper = new() {Id = SnapWebModuleId.PhoneScraper, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ExtraWorkSlot = new() {Id = SnapWebModuleId.ExtraWorkSlot, Enabled = false, Purchaseable = true, Price = 5, Category = SnapWebModuleCategory.Access };
    public static SnapWebModule AcceptFriend = new() {Id = SnapWebModuleId.AcceptFriend, Enabled = true, Purchaseable = false, Price = 5, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule RefreshFriends = new() {Id = SnapWebModuleId.RefreshFriends, Enabled = true, Purchaseable = false, Price = 5, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule RelogAccounts = new() {Id = SnapWebModuleId.RelogAccounts, Enabled = true, Purchaseable = false, Price = 5, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule QuickAdd = new() {Id = SnapWebModuleId.QuickAdd, Enabled = false, Purchaseable = false, Price = 5, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ProxyScraper = new() {Id = SnapWebModuleId.ProxyScraper, Enabled = true, Purchaseable = false, Price = 5, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule Analytics = new() {Id = SnapWebModuleId.Analytics, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule FriendCleaner = new() {Id = SnapWebModuleId.FriendCleaner, Enabled = true, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ViewPublicStory = new() {Id = SnapWebModuleId.ViewPublicStory, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ReportUserStoryRandom = new() {Id = SnapWebModuleId.ReportUserStoryRandom, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule MacroManager = new() {Id = SnapWebModuleId.MacroManager, Enabled = false, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ProxyChecking = new() {Id = SnapWebModuleId.ProxyChecking, Enabled = true, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule ExportFriends = new() {Id = SnapWebModuleId.ExportFriends, Enabled = true, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };
    public static SnapWebModule BitmojiCreator = new() {Id = SnapWebModuleId.BitmojiCreator, Enabled = true, Purchaseable = true, Price = 50, Category = SnapWebModuleCategory.Functionality };

    
    public static List<SnapWebModule> DefaultModules = new()
    {
        SendMessage,
        PostDirect,
        AddFriend,
        Subscribe,
        ReportUserPublicProfileRandom,
        ReportUserRandom,
        ViewBusinessPublicStory,
        Test,
        AndroidOs,
        ExportAccount,
        SnapwebAccess,
        ExtraAccounts100,
        PostStory,
        Ios,
        ExtraStorage1,
        ExtraStorage2,
        ExtraStorage3,
        ExtraConcurrencySlot,
        FindUsersViaSearch,
        EmailScraper,
        PhoneScraper,
        ExtraWorkSlot,
        AcceptFriend,
        RefreshFriends,
        QuickAdd,
        ProxyScraper,
        Analytics,
        FriendCleaner,
        ViewPublicStory,
        ReportUserStoryRandom,
        MacroManager,
        ProxyChecking,
        RelogAccounts,
        ExportFriends,
        BitmojiCreator
    };

    [Key]
    public long DatabaseId { get; set; }
    public SnapWebModuleId Id { get; set; }
    public bool Enabled { get; set; }
    public bool Purchaseable { get; set; }
    public double Price { get; set; }
    public SnapWebModuleCategory Category { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SnapWebIconClass { get; set; }
}