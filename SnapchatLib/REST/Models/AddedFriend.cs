using System;

namespace SnapchatLib.REST.Models;

public class AddedFriend
{
    public string add_source { get; set; }
    public string add_source_type { get; set; }
    public Int64 cameos_sharing_policy { get; set; }
    public string direction { get; set; }
    public string display { get; set; }
    public Int64 expiration { get; set; }
    public bool is_incoming_friend_request_viewed { get; set; }
    public string mutable_username { get; set; }
    public string name { get; set; }
    public Int64 reverse_ts { get; set; }
    public string snapshot_metadata { get; set; }
    public object ts { get; set; }
    public Int64 type { get; set; }
    public string user_id { get; set; }
    public string snap_pro_id { get; set; }
}