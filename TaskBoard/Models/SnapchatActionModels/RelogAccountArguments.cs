using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class RelogAccountArguments : ActionArguments
{
    private SnapchatAccountModel account;
    private bool UseAllAccounts { get; set; }

    public new ValidationResult Validate()
    {
        try
        {
            base.Validate();

            if (!UseAllAccounts && account == null && AccountsToUse == 0)
            {
                throw new ArgumentException("You must have at least one account assigned to this job.");
            }
            
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(RelogAccountArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator RelogAccountArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<RelogAccountArguments>(arguments)!;
    }
}