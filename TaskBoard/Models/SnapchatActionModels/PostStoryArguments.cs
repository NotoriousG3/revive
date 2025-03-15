using Newtonsoft.Json;

namespace TaskBoard.Models.SnapchatActionModels;

public class PostStoryArguments : ActionArguments
{
    // These need get; set; for it to be deserialized properly from requests
    public int MediaFileId { get; set; }
    public string? SwipeUpUrl { get; set; } = null;
    public List<string>? Mentioned { get; set; } = null;

    public override ValidationResult Validate()
    {
        try
        {
            base.Validate();

            if (MediaFileId == 0) throw new ArgumentException("MediaFileId not provided");

            if (!string.IsNullOrWhiteSpace(SwipeUpUrl))
            {
                var url = new Uri(SwipeUpUrl);
                if (!Uri.IsWellFormedUriString(url.ToString(), UriKind.Absolute) || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ArgumentException("Swipe Up URL must be a valid URL, including http or https scheme");
                }
            }
            if (Mentioned != null && Mentioned.Count > 0) CheckUserList(Mentioned);
            return new ValidationResult();
        }
        catch (Exception e)
        {
            return new ValidationResult(e);
        }
    }

    public static implicit operator string(PostStoryArguments arguments)
    {
        return arguments.ToString();
    }

    public static implicit operator PostStoryArguments(string arguments)
    {
        return JsonConvert.DeserializeObject<PostStoryArguments>(arguments)!;
    }
}