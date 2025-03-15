using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class ValidationResult
{
    public Exception? Exception;

    public ValidationResult(Exception e)
    {
        Exception = e;
    }

    public ValidationResult()
    {
    }
}

public class ActionArguments
{
    private static readonly string[] UsersBlacklist =
    {
        "rum"
    };

    public int AccountsToUse { get; set; }
    public long AccountGroupToUse { get; set; }
    public long ProxyGroup { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public long? PreviousWorkRequestId { get; set; } = null;
    public long? ChainDelayMs { get; set; } = null;

    public virtual ValidationResult Validate()
    {
        if (AccountGroupToUse == 0 && AccountsToUse < 1) throw new ArgumentException("AccountsToUse must be higher than 0");
        if (AccountGroupToUse > 0 && AccountsToUse < 1) AccountsToUse = 1;

        if (ProxyGroup == 0) throw new ArgumentException("Please select a proxy group");

        // Only check ChainDelayMs when PreviousWorkRequestId is not null
        if (PreviousWorkRequestId != null && ChainDelayMs is < 1) throw new ArgumentException("ChainDelayMs must be higher than 0");
        return new ValidationResult();
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    protected void CheckUsername(string username, bool overRide = false)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username must not be null nor whitespace");
        if (!overRide && UsersBlacklist.Contains(username)) throw new ArgumentException($"User {username} cannot be used for this.");
    }

    protected void CheckUserList(List<string> users)
    {
        switch (users.Count)
        {
            case 0:
                throw new ArgumentException("Users must be more than 0");
            case 1 when UsersBlacklist.Intersect(users).Any():
                throw new ArgumentException($"User {users[0]} cannot be used for this.");
            default:
                users.RemoveAll(u => UsersBlacklist.Contains(u));
                break;
        }
    }
    protected void CheckKeywords(string keyword)
    {
        switch (keyword.Length)
        {
            case 0:
                throw new ArgumentException("Keywords can not be blank.");
            default:
                break;
        }
    }
    
    protected void CheckEmail(string email)
    {
        switch (email.Length)
        {
            case 0:
                throw new ArgumentException("E-Mail addresses can not be blank.");
            default:
                break;
        }
    }
    
    protected void CheckPhoneNumber(string phone, string country_code)
    {
        switch (phone.Length)
        {
            case 0:
                throw new ArgumentException("Phone numbers can not be blank.");
            default:
                break;
        }
        switch (country_code.Length)
        {
            case 0:
                throw new ArgumentException("Phone numbers country code can not be blank.");
            default:
                break;
        }
    }
}