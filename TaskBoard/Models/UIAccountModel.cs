using SnapchatLib;

namespace TaskBoard.Models;

public class UIAccountModel
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string? Email { get; set; }
    public DateTime? CreationDate { get; set; }
    public OS OS { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public int FriendCount { get; set; }
    public int IncomingFriendCount { get; set; }
    public int OutgoingFriendCount { get; set; }
    public ValidationStatus PhoneValidated { get; set; }
    public ValidationStatus EmailValidated { get; set; }
    public int Groups { get; set; }

    public static IEnumerable<UIAccountModel> ToEnumerable(IEnumerable<SnapchatAccountModel> accounts, List<EmailModel> emails)
    {
        var uiAccounts = new List<UIAccountModel>();

        foreach (var account in accounts)
            uiAccounts.Add(new UIAccountModel
            {
                Id = account.Id,
                Username = account.Username,
                Email = emails.Find(e => e.AccountId == account.Id)?.Address,
                CreationDate = account.CreationDate,
                OS = account.OS,
                FriendCount = account.FriendCount,
                IncomingFriendCount = account.IncomingFriendCount,
                OutgoingFriendCount = account.OutgoingFriendCount,
                AccountStatus = account.AccountStatus,
                PhoneValidated = account.PhoneValidated,
                EmailValidated = account.EmailValidated,
                Groups = account.Groups.Count
            });

        return uiAccounts;
    }
}