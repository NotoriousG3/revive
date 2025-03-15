namespace SnapchatLib.REST.Models
{
    public class Analytics
    {
        public string client_id { get; set; }
        public double event_sampling_rate { get; set; }
        public int page_view_id { get; set; }
        public string experiment_id { get; set; }
        public bool app_travel_mode { get; set; }
        public double user_sampling_rate { get; set; }
        public string locale { get; set; }
        public string app_build { get; set; }
        public string study_name { get; set; }
        public string app_variant { get; set; }
        public string device_model { get; set; }
        public int log_queue_sequence_id { get; set; }
        public string app_startup_type { get; set; }
        public double client_ts { get; set; }
        public string event_name { get; set; }
        public string session_id { get; set; }
        public string log_queue_name { get; set; }
        public string os_type { get; set; }
        public string page_tab_type { get; set; }
        public string app_version { get; set; }
        public int connection_download_bandwidth_bps { get; set; }
        public string device_connectivity { get; set; }
        public string os_version { get; set; }
        public string os_minor_version { get; set; }
        public string long_client_id { get; set; }
        public bool google_play_instant { get; set; }
        public int referrer_click_timestamp_seconds { get; set; }
        public string advertising_id { get; set; }
        public string install_referral_url { get; set; }
        public int install_begin_timestamp_seconds { get; set; }
        public string channel_id { get; set; }
        public long install_timestamp { get; set; }
        public bool enable_ad_tracking { get; set; }
        public int referrer_click_timestamp_server_seconds { get; set; }
        public int install_begin_timestamp_server_seconds { get; set; }
        public string http_user_agent { get; set; }
        public string apps_scope_id { get; set; }
        public bool has_logged_in_before { get; set; }
        public string additional_info { get; set; }
        public string registration_version { get; set; }
        public string registration_session_id { get; set; }
        public string page { get; set; }
        public string page_from { get; set; }
        public string from_page { get; set; }
        public string to_page { get; set; }
        public string authentication_session_id { get; set; }
        public string @event { get; set; }
        public string to_state { get; set; }
        public string from_state { get; set; }
        public int transition_time_ms { get; set; }
        public string trigger { get; set; }
        public string user_guid { get; set; }
        public bool accepted { get; set; }
        public string permission_prompt_type { get; set; }
        public string permision_prompt_action_type { get; set; }
        public string setting_field_value_previous { get; set; }
        public string setting_field_value { get; set; }
        public string setting_field_name { get; set; }
    }
}
