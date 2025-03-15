using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class TestArguments : MessagingArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public bool Pass { get; set; } = true;
    public int DelayMs { get; set; } = 1000;
    public int MediaFileId { get; set; }
    public IEnumerable<string>? AccountIds { get; set; } = null;

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();

            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(TestArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator TestArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<TestArguments>(arguments)!;
    }
}