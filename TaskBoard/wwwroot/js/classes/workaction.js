// Keep in sync with WorkAction in WorkRequest
class WorkAction {
    static PostDirect = 0;
    static ReportUserPublicProfileRandom = 1;
    static ReportUserRandom = 2;
    static SendMessage = 3;
    static Subscribe = 4;
    static CreateAccounts = 5;
    static ViewBusinessPublicStory = 6;
    static AddFriend = 7;
    static Test = 8;
    static PostStory = 9;
    static FindUsersViaSearch = 10;
    static PhoneScraper = 11;
    static EmailScraper = 12;
    static SendMention = 13;
    static AcceptFriend = 14;
    static RefreshFriend = 15;
    static QuickAdd = 16;
    static FriendCleaner = 17;
    static ViewPublicStory = 18;
    static ReportPublicStoryRandom = 19;
    static ChangeUsername = 20;
    static RelogAccounts = 21;
    static ExportFriends = 22;

    static TimeStatus(status){
        if(status === "31-12-1969 19:00" || status == null){
            return "N/A";
        }
        
        return status;
    }
    static ToString(actionId) {
        switch (actionId) {
            case this.PostDirect:
                return "Post Direct";
            case this.ReportUserPublicProfileRandom:
                return "Report User Public Profile Random";
            case this.ReportUserRandom:
                return "Report User Random";
            case this.SendMessage:
                return "Send Message";
            case this.Subscribe:
                return "Subscribe";
            case this.CreateAccounts:
                return "Create Accounts";
            case this.ViewBusinessPublicStory:
                return "View Business Public Story";
            case this.AddFriend:
                return "Add Friend";
            case this.PostStory:
                return "Post Story";
            case this.FindUsersViaSearch:
                return "Keyword Scrape";
            case this.PhoneScraper:
                return "Phone Scrape";
            case this.EmailScraper:
                return "Email Scrape";
            case this.SendMention:
                return "Send Mention";
            case this.Test:
                return "Test";
            case this.AcceptFriend:
                return "Accept Friends";
            case this.RefreshFriend:
                return "Refresh Friends";
            case this.QuickAdd:
                return "Quick Add";
            case this.FriendCleaner:
                return "Friend Removal";
            case this.ViewPublicStory:
                return "View Public Story";
            case this.ReportPublicStoryRandom:
                return "Report Public Random Story";
            case this.ChangeUsername:
                return "Change Username";
            case this.RelogAccounts:
                return "Relog Accounts";
            case this.ExportFriends:
                return "Export Friends";
            default:
                return "Not defined";
        }
    }
}
