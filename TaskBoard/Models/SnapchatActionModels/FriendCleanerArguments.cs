using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class FriendCleanerArguments : ActionArguments
{
    private SnapchatAccountModel account;
    private bool UseAllAccounts { get; set; }
    public int AddDelay { get; set; }

    public new ValidationResult Validate()
    {
        try
        {
            base.Validate();

            if (!UseAllAccounts && account == null && AccountsToUse == 0)
            {
                throw new ArgumentException("You must have at least one account assigned to this job.");
            }

            if (AddDelay < 5 || AddDelay > 300)
            {
                throw new ArgumentException("Allowed removal delay is 5-300 second(s).");
            }
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(FriendCleanerArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator FriendCleanerArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<FriendCleanerArguments>(arguments)!;
    }
}