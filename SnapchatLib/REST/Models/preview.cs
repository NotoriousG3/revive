using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

// TODO: Check Unused
public class SnapConnectAttributes
{
    public string source_app_oauth_client_id { get; set; }
    public string source_app_display_name { get; set; }
}

// TODO: Check Unused
public class Story2
{
    public object timestamp { get; set; }
    public bool is_public { get; set; }
    public string id { get; set; }
    public string client_id { get; set; }
    public string username { get; set; }
    public int media_type { get; set; }
    public string media_id { get; set; }
    public string media_url { get; set; }
    public bool zipped { get; set; }
    public string thumbnail_url { get; set; }
    public bool needs_auth { get; set; }
    public string thumbnail_iv { get; set; }
    public string media_key { get; set; }
    public string media_iv { get; set; }
    public bool is_official_story { get; set; }
    public string context_hint { get; set; }
    public bool is_infinite_duration { get; set; }
    public int brand_friendliness { get; set; }
    public int capture_timestamp { get; set; }
    public SnapConnectAttributes snap_connect_attributes { get; set; }
    public bool mature_content { get; set; }
    public string caption_text_display { get; set; }
    public string large_thumbnail_url { get; set; }
    public int time_left { get; set; }
    public double time { get; set; }
    public bool is_shared { get; set; }
}

public class stories
{
    public stories story { get; set; }
    public bool viewed { get; set; }
    public string flushable_story_id { get; set; }
}

public class PublicStories
{
    public bool mature_content { get; set; }
    public int new_story_count { get; set; }
    public List<stories> stories { get; set; }
    public string user_id { get; set; }
    public string type { get; set; }
    public string username { get; set; }
}

public class StoryJson
{
    public PublicStories public_stories { get; set; }
}