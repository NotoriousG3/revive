using System.Text.RegularExpressions;
using _5sim.Objects;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ExportFriendsArguments : ActionArguments
{
    private SnapchatAccountModel account;
    private bool UseAllAccounts { get; set; }
    public string ExportEmail { get; set; }

    public new ValidationResult Validate()
    {
        try
        {
            base.Validate();
            
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(ExportEmail);
            
            if (ExportEmail.IsNullOrEmpty() && !match.Success)
            {
                throw new ArgumentException("You must provide a valid email!");
            }
            
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

    public static implicit operator string(ExportFriendsArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator ExportFriendsArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<ExportFriendsArguments>(arguments)!;
    }
}