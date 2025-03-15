using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class Snapchatter
{
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_background_id { get; set; }
    public string bitmoji_scene_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public string display_name { get; set; }
    public string mutable_username { get; set; }
    public string snap_pro_id { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
}

public class SnapchatterPublicInfoResponse
{
    public List<Snapchatter> snapchatters { get; set; }
}