using System;

namespace SnapchatLib.REST.Models;

public class IdentityCheckJson
{
    public Int64 display_name_pop_up_count { get; set; }
    public bool force_addressbook_full_sync { get; set; }
    public bool is_add_nearby_enabled { get; set; }
    public bool is_contact_sync_enabled { get; set; }
    public bool is_email_verified { get; set; }
    public bool is_high_accuracy_required_for_nearby { get; set; }
    public bool is_snapchat_contact_permission_ios_v2_enabled { get; set; }
    public bool is_user_searchability_prompt_enabled { get; set; }
    public Int64 last_checked_trophies_timestamp { get; set; }
    public Int64 max_suggestions_in_full_page { get; set; }
    public Int64 max_suggestions_in_quick_add { get; set; }
    public Int64 max_suggestions_in_search_result { get; set; }
    public Int64 red_gear_duration_millis { get; set; }
    public bool require_phone_password_confirmed { get; set; }
    public bool should_show_quick_add_unit { get; set; }
    public Int64 suggested_friend_fetch_threshold_hours { get; set; }
    public bool was_snapchat_contact_permission_never_granted { get; set; }
}