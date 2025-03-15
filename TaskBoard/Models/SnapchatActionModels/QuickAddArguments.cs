using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class QuickAddArguments : ActionArguments
{
    private SnapchatAccountModel account;
    private bool UseAllAccounts { get; set; }
    public int MaxAdds { get; set; }
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

            if(MaxAdds > 1000 || MaxAdds < 1)
            {
                throw new ArgumentException("Allowed adds per account is 1-1000 friends.");
            }
            
            if (AddDelay < 5 || AddDelay > 300)
            {
                throw new ArgumentException("Allowed accept delay is 5-300 second(s).");
            }
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(QuickAddArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator QuickAddArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<QuickAddArguments>(arguments)!;
    }
}