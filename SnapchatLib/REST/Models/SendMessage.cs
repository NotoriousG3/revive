using System.Collections.Generic;

namespace SnapchatLib.Sockets.Messaging.Extensions;

public class MediaCardAttribute
{
    public int start { get; set; }
    public int end { get; set; }
    public string type { get; set; }
    public string url { get; set; }
}

public class SendMessage
{
    public List<MediaCardAttribute> media_card_attributes { get; set; }
}