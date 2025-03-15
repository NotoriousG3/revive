using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class UploadUrl
{
    public string url { get; set; }
    public int expirySecs { get; set; }
    public string type { get; set; }
    public string region { get; set; }
}

public class UploadUrlsByType
{
    public string type { get; set; }
    public List<UploadUrl> upload_urls { get; set; }
}

public class GetUploadUrlsJson
{
    public List<UploadUrlsByType> upload_urls_by_type { get; set; }
}