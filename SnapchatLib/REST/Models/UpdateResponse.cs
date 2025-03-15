using System;
using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class UpdatesJson
{
    public ClientProperties client_properties { get; set; }
    public List<FeatureSettingsJson> feature_settings_response { get; set; }
    public List<object> industries { get; set; }
    public List<string> seen_tooltips { get; set; }
    public bool amr_codec_enabled_android { get; set; }
    public bool android_media_recorder_surface_recording_enabled { get; set; }
    public bool audio_note_enabled_android { get; set; }
    public bool bitmoji_hide_download_prompt { get; set; }
    public bool blur_after_downscale_enabled_android { get; set; }
    public bool camera1_take_photo_api_blacklisted_android { get; set; }
    public bool camera1_take_photo_api_whitelisted_android { get; set; }
    public bool camera2_enabled_android { get; set; }
    public bool camera2_limited_level_high_resolution_photo_enabled_android { get; set; }
    public bool camera2_take_photo_api_android { get; set; }
    public bool chat_video_enabled_android { get; set; }
    public bool d2s_media_download_enabled { get; set; }
    public bool dirty_video_rendering_enabled_android { get; set; }
    public bool discover_content_disabled { get; set; }
    public bool enable_separate_lens_list_for_back_camera { get; set; }
    public bool enable_world_lens_grid { get; set; }
    public bool enabled_push_notifications { get; set; }
    public bool front_camera_zoom_enabled_android { get; set; }
    public bool gles3_allowed_android { get; set; }
    public bool has_used_laguna { get; set; }
    public bool is_affected_by_gdpr { get; set; }
    public bool logged { get; set; }
    public bool pinnable_stickers_enabled_android { get; set; }
    public bool raw_thumbnail_upload_enabled { get; set; }
    public bool require_refreshing_profile_media { get; set; }
    public bool reverse_filter_enabled_android { get; set; }
    public bool samsung_aac_enc_enabled_android { get; set; }
    public bool sc_media_recorder_enabled_android { get; set; }
    public bool sc_media_recorder_recommended_android { get; set; }
    public bool should_call_to_verify_number { get; set; }
    public bool smoothing_filter_enabled_android { get; set; }
    public bool speed_filters_enabled_android { get; set; }
    public bool sticker_visual_recommendation_enabled_android { get; set; }
    public bool video_decoder_texcoord_transformation_enabled_android { get; set; }
    public bool video_note_api_fallback_android { get; set; }
    public bool video_note_enabled_android { get; set; }
    public bool video_thumbnail_enabled_android { get; set; }
    public double nft_hi_timeout { get; set; }
    public double nft_lo_timeout { get; set; }
    public Int64 credits { get; set; }
    public Int64 notification_privacy { get; set; }
    public Int64 number_of_best_friends { get; set; }
    public Int64 received { get; set; }
    public Int64 score { get; set; }
    public Int64 sent { get; set; }
    public Int64 snap_media_upload_so_timeout { get; set; }
    public Int64 snap_p { get; set; }
    public Int64 story_count { get; set; }
    public long created { get; set; }
    public long last_address_book_updated_date { get; set; }
    public string auth_token { get; set; }
    public string birthday { get; set; }
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public string blizzard_token { get; set; }
    public string device_token { get; set; }
    public string display_name { get; set; }
    public string email { get; set; }
    public string iso3166_alpha2_country_code { get; set; }
    public string laguna_id { get; set; }
    public string mobile { get; set; }
    public string notification_sound_setting { get; set; }
    public string phone_number_country_code { get; set; }
    public string quick_add_privacy { get; set; }
    public string registration_country_code { get; set; }
    public string ringing_sound { get; set; }
    public string said { get; set; }
    public string snapchat_phone_number { get; set; }
    public string story_privacy { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
    public string voip_device_token { get; set; }
}

public class FeatureSettingsJson
{
    public long latest_version { get; set; }
    public string setting_name { get; set; }
    public string setting_value { get; set; }
}
