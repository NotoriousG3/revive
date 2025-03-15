using System.Collections.Generic;

namespace SnapchatLib.REST.Models;
public class ClientProperties
{
    public string snapcash_new_tos_accepted { get; set; }
    public string snapcash_tos_v2_accepted { get; set; }
    public string square_tos_accepted { get; set; }
    public string tos_version_10_accepted { get; set; }
    public string tos_version_11_accepted { get; set; }
    public string tos_version_3_accepted { get; set; }
    public string tos_version_4_accepted { get; set; }
    public string tos_version_5_accepted { get; set; }
    public string tos_version_6_accepted { get; set; }
    public string tos_version_7_accepted { get; set; }
    public string tos_version_8_accepted { get; set; }
    public string tos_version_9_accepted { get; set; }
    public string tos_version_9_and_10_accepted { get; set; }
    public string tos_version_9_and_11_accepted { get; set; }
    public string tou_9_14_accepted { get; set; }
}
public class ExtraFriendmojiMutableDict
{
}

public class ExtraFriendmojiReadOnlyDict
{
}

public class FeatureSettingsResponse
{
    public long latest_version { get; set; }
    public string setting_name { get; set; }
    public string setting_value { get; set; }
}

public class FeedResponseInfo
{
}

public class Friend
{
    public int cameos_sharing_policy { get; set; }
    public bool can_see_custom_stories { get; set; }
    public string direction { get; set; }
    public string display { get; set; }
    public int expiration { get; set; }
    public string friendmoji_string { get; set; }
    public List<object> friendmojis { get; set; }
    public bool is_popular { get; set; }
    public bool is_story_muted { get; set; }
    public string mutable_username { get; set; }
    public string name { get; set; }
    public object reverse_ts { get; set; }
    public int snap_streak_count { get; set; }
    public object ts { get; set; }
    public int type { get; set; }
    public string user_id { get; set; }
    public bool? ignored_link { get; set; }
    public FideliusInfo fidelius_info { get; set; }
    public bool is_bitmoji_friendmoji_sharing_supported { get; set; }
    public bool is_cameos_sharing_supported { get; set; }
    public string birthday { get; set; }
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_background_id { get; set; }
    public string bitmoji_scene_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public string snap_pro_id { get; set; }
    public string story_privacy { get; set; }
}

public class FriendsResponse
{
    public List<object> added_friends { get; set; }
    public List<string> bests { get; set; }
    public List<string> bests_user_ids { get; set; }
    public ExtraFriendmojiMutableDict extra_friendmoji_mutable_dict { get; set; }
    public ExtraFriendmojiReadOnlyDict extra_friendmoji_read_only_dict { get; set; }
    public List<Friend> friends { get; set; }
    public string friends_sync_token { get; set; }
    public string friends_sync_type { get; set; }
    public bool is_response_with_partial_columns { get; set; }
}

public class IdentityCheckResponse
{
    public int display_name_pop_up_count { get; set; }
    public bool force_addressbook_full_sync { get; set; }
    public bool is_add_nearby_enabled { get; set; }
    public bool is_contact_sync_enabled { get; set; }
    public bool is_email_verified { get; set; }
    public bool is_high_accuracy_required_for_nearby { get; set; }
    public bool is_snapchat_contact_permission_ios_v2_enabled { get; set; }
    public bool is_user_searchability_prompt_enabled { get; set; }
    public int last_checked_trophies_timestamp { get; set; }
    public int max_suggestions_in_full_page { get; set; }
    public int max_suggestions_in_quick_add { get; set; }
    public int max_suggestions_in_search_result { get; set; }
    public int red_gear_duration_millis { get; set; }
    public bool require_phone_password_confirmed { get; set; }
    public bool should_show_quick_add_unit { get; set; }
    public int suggested_friend_fetch_threshold_hours { get; set; }
    public bool was_snapchat_contact_permission_never_granted { get; set; }
}

public class InAppWarning
{
}

public class LoginJson
{
    public string message { get; set; }
    public string odlv_pre_auth_token { get; set; }
    public BootstrapData bootstrap_data { get; set; }
    public CofResponse cof_response { get; set; }
    public FeedResponseInfo feed_response_info { get; set; }
    public FriendsResponse friends_response { get; set; }
    public IdentityCheckResponse identity_check_response { get; set; }
    public InAppWarning in_app_warning { get; set; }
    public bool is_otp_two_fa_enabled { get; set; }
    public bool is_sms_two_fa_enabled { get; set; }
    public long last_active_time { get; set; }
    public bool quick_login_response { get; set; }
    public SecInfo sec_info { get; set; }
    public List<object> two_fa_verified_devices { get; set; }
    public UpdatesResponse updates_response { get; set; }
    public UserSessionInit user_session_init { get; set; }
}

public class SecInfo
{
}

public class StudySettings
{
    public string ADSERVER_INSTALL_PREDICTION { get; set; }
    public string ADSERVER_SKIP_PREDICTION { get; set; }
    public string ADSERVER_SWIPE_PREDICTION { get; set; }
    public string ADSERVER_WEB_VIEW_PREDICTIVE_PREFETCH { get; set; }
    public string ADS_HOLDOUT_01 { get; set; }
    public string APP_DELEGATE_AUTH_MIGRATION { get; set; }
    public string ARROYO_1ON1_IOS { get; set; }
    public string ARROYO_EXPERIENCE_IOS { get; set; }
    public string ARROYO_IOS_BWC_ONE_ON_ONE_ENGAGEMENT_TEST { get; set; }
    public string AUDIO_NOTE_ON_MAIN_THREAD { get; set; }
    public string AWS_SHIM_TEST { get; set; }
    public string AdManager { get; set; }
    public string BLIZZARD_IOS_JSON_FRAMES { get; set; }
    public string BLIZZARD_IOS_PROTO_FRAMES { get; set; }
    public string BLOOPS_IN_LENSES_BEAUTIFICATION_IOS { get; set; }
    public string BLOOPS_LENSES_BEAUTIFICATION_IOS { get; set; }
    public string BLOOPS_ONBOARDING_UI_CONTAINER_IOS { get; set; }
    public string BLOOPS_POST_TO_STORIES_IOS { get; set; }
    public string BLOOPS_PREPROCESSING_BEAUTIFICATION_IOS { get; set; }
    public string BLOOPS_TWO_PERSON_IN_PREVIEW { get; set; }
    public string CAMERA_ANDROID_TIMELINE_CONTEXT_CARD { get; set; }
    public string CAMERA_IOS_14_G2S_IMPROVEMENTS { get; set; }
    public string CAMERA_IOS_ADD_TO_YOUR_SNAP { get; set; }
    public string CAMERA_IOS_G2S_TACTICAL_IMPROVEMENT { get; set; }
    public string CAMERA_IOS_MATCHA_FEATURE_FRAMEWORK { get; set; }
    public string CAMERA_IOS_METAL_LAYER_LAZY_SETUP { get; set; }
    public string CAMERA_IOS_SMART_GAMMA_CORRECTION { get; set; }
    public string CAMERA_IOS_VIDEO_STREAM_MONITORING { get; set; }
    public string CAMPLAT_FLUSH_OPENGL_CACHE_ON_CLEANUP_REQUEST { get; set; }
    public string CAMPLAT_MEMMAPPED_NSDATA_V0 { get; set; }
    public string CAPTURE_IMU_WITH_VIDEO { get; set; }
    public string COGNAC_AD_SKIPPABLE_FLAG_IOS { get; set; }
    public string COGNAC_SNAPCODE_PLUGIN_IOS { get; set; }
    public string COMPOSER_ADD_FRIENDS_PAGE_PUBLIC_STORY_iOS { get; set; }
    public string COMPOSER_ADD_FRIENDS_POST_TYPE_SEARCH_BACKEND_IOS { get; set; }
    public string CONTENT_MANAGER_IMAGES_EXPERIMENT { get; set; }
    public string CONTEXT_AB_SERVICE_IOS { get; set; }
    public string CONTEXT_IOS_COMBINED_MENU_MEMORIES { get; set; }
    public string CONTEXT_QUICK_REPLY_IOS { get; set; }
    public string CONTEXT_SPOTLIGHT_SHARE_WATCH_CTA_IOS { get; set; }
    public string CRASH_REPORT_IOS_NEW { get; set; }
    public string CRNET_IOS_V5 { get; set; }
    public string CT_CAPTION_STICKER_SUGGESTION { get; set; }
    public string CT_QUICK_SEARCH_BAR_IN_CHAT_IOS { get; set; }
    public string DFP_USER_SEGMENT_TARGETING { get; set; }
    public string DFP_USER_SEGMENT_TARGETING_20 { get; set; }
    public string DF_AST_ANNIHILATION { get; set; }
    public string DF_BIB_STORY_IMPRESSION_CAP { get; set; }
    public string DF_BITMOJI_TV_PUBLISHER_ROLLOUT { get; set; }
    public string DF_BOOST_SPOTLIGHT { get; set; }
    public string DF_CHANNEL_PIVOT { get; set; }
    public string DF_DM_DEMOTION { get; set; }
    public string DF_EMPLOYEE_GATING { get; set; }
    public string DF_MAX_ENGAGEMENT_STATS { get; set; }
    public string DF_MKTPLC_EXPLORATION { get; set; }
    public string DF_MKTPLC_FY_LAUNCH_1 { get; set; }
    public string DF_MKTPLC_HERO_TILE_HOLDOUT { get; set; }
    public string DF_MKTPLC_INTERNAL_CURATED { get; set; }
    public string DF_MKTPLC_SPT_3 { get; set; }
    public string DF_MKTPLC_SUBS_LAUNCH { get; set; }
    public string DF_NEWS_TIERING { get; set; }
    public string DF_NEW_USERS_COLD_START_QUALITY_1 { get; set; }
    public string DF_PROFILE_AB_FETCH { get; set; }
    public string DF_PUBLISHER_TILE_BOLT_URL { get; set; }
    public string DF_PUBLISHER_TILE_SUGGESTIVENESS { get; set; }
    public string DF_PU_QUALITY_MODERATION_DEMOTION { get; set; }
    public string DF_RANDOM_BUCKET { get; set; }
    public string DF_RANKING_STORY_INTERACTION_HISTORY_IOS { get; set; }
    public string DF_SHOWS_REPO_LWS { get; set; }
    public string DF_SPOTLIGHT_SWPIE_MAIN_THREAD_OPTIMIZATION { get; set; }
    public string DF_SPOTLIGHT_VIDEO_LENGTH_FILTER_IOS { get; set; }
    public string DF_SPOTLIGHT_VIEW_COUNT_NOTIF_IOS { get; set; }
    public string DF_STORIES_PAGE_SIZE_V6_IOS { get; set; }
    public string DF_STRICT_LOCALE_MATCH_ABLATE_V1 { get; set; }
    public string DISCOVER_RANKING_ML_STUDY_REMOVE_DDL { get; set; }
    public string DISCOVER_TOPIC_STICKERS { get; set; }
    public string DISCOVER_TOPIC_STICKER_VENDING_SERVICE { get; set; }
    public string DUM_IOS { get; set; }
    public string DUM_SERVER { get; set; }
    public string EXPOSURE_CACHE_FILE_TTL_INCREASE_IOS { get; set; }
    public string FF_RANKING_POSTER_SIGNALS_ROLLOUT { get; set; }
    public string FRIENDS_OF_FRIENDS_SUGGESTION { get; set; }
    public string GAME_SDK_INITIALIZE_NOT_DISMISS_LOADING_SCREEN_IOS { get; set; }
    public string GROWTH_NOTIFICATION_CONTACT_SYNC_EXISTING_USER_IOS { get; set; }
    public string GROWTH_NOTIFICATION_CONTACT_SYNC_NEW_USER_IOS { get; set; }
    public string GROWTH_NOTIFICATION_LENS_BADGE_IOS { get; set; }
    public string GROWTH_NOTIFICATION_NON_FRIEND_STORIES { get; set; }
    public string HIDE_FEATURED_LIVE_FOR_SELECT_COUNTRIES { get; set; }
    public string IOS_15_CAPTURE_SESSION_PHOTO_QUALITY_PRIORITIZATION { get; set; }
    public string IOS_BACKGROUND_PREFETCH { get; set; }
    public string IOS_BLIZZARD_MOVE_OFF_MAIN { get; set; }
    public string IOS_GROUP_CHAT_CREATION_CARD { get; set; }
    public string IOS_MATCHA_SENDTO { get; set; }
    public string IOS_METRIC_KIT_ROLL_OUT { get; set; }
    public string IOS_PAYOUTS_HUB_EARNERS { get; set; }
    public string IOS_PAYOUTS_HUB_GLOBAL { get; set; }
    public string IOS_PERMISSION_SETTINGS_REPORT_JOB { get; set; }
    public string IOS_PLATFORM_COMPONENTS_QUEUE_PRIORITY { get; set; }
    public string IOS_PRIORITY_CONCURRENCY_CONTROL { get; set; }
    public string IOS_SENDTO_LOGIN_PAGINATION { get; set; }
    public string IOS_SNAP_AD_BACKFILL { get; set; }
    public string Image_Renderer_Migration { get; set; }
    public string LENSCORE_FASTDNN_NNAPI_EXP_PIXEL_6 { get; set; }
    public string LENSCORE_INVITE_FLOW_DEVELOPER_TESTING { get; set; }
    public string LENSCORE_PRECISE_TRACKING_ON_PHOTO { get; set; }
    public string LENSES_IOS_SCHEDULE_RESTORATION_OFF_STARTUP { get; set; }
    public string LENS_IOS_DOWNLOAD_CONCURRENCY_RESOLVING { get; set; }
    public string LE_DEEPLINKING_IOS { get; set; }
    public string LE_ERROR_STATE_IOS { get; set; }
    public string LE_LENS_CREATOR_CATEGORY_IOS { get; set; }
    public string LE_LENS_CTA_IOS { get; set; }
    public string LE_RANKING_TUNING { get; set; }
    public string LE_TILE_COLLECTION_IOS { get; set; }
    public string LOOKSERY_SPONSORED_GEO_BACK { get; set; }
    public string MATCHA_NOTIFICATION_ACTION_HANDLING { get; set; }
    public string MATCHA_TASK_FRIEND_FEED { get; set; }
    public string MDP_CONTENT_MANAGER_LENS_DATA_FETCHING { get; set; }
    public string MDP_CRONET_BANDWIDTH_IOS { get; set; }
    public string MDP_INFRA_PROD_SAUDI_ARABIA_CF_VS_GCDN { get; set; }
    public string MDP_NATIVE_RANKER_IOS { get; set; }
    public string MDP_OPERA_SPOTLIGHT_5TH_TAB_INLINE_PREFETCH { get; set; }
    public string MDP_TUNE_NATIVE_RETRY_REQUEST_TYPE { get; set; }
    public string MEMORIES_IOS_2021_YEAR_END_STORY { get; set; }
    public string MEMORIES_IOS_COMPOSER_SAVE_DIALOG { get; set; }
    public string MEMORIES_IOS_SINGLE_STORY_SAVE_WITH_SNAPDOC { get; set; }
    public string MEMORIES_IOS_STITCH_MULTISNAP_SEND { get; set; }
    public string MQ_DFMEDIA_UNIFY_480P_IOS { get; set; }
    public string MQ_HEVC_720_POPULAR_STORIES_IOS { get; set; }
    public string MQ_HEVC_NVENC_720_FRIEND_MEDIA_US_IOS { get; set; }
    public string MVM_TASKS_TRIAL { get; set; }
    public string MVM_TRIAL { get; set; }
    public string NOTIFICATION_BADGE_AND_REVOKE_IOS { get; set; }
    public string NV_iOS { get; set; }
    public string PU_DF_USE_BOLT_FOR_PUBLIC_USER_STORY_ANDROID { get; set; }
    public string PU_PF_IOS_INT_1 { get; set; }
    public string PU_SPOTLIGHT_SNAP_STATUS_IOS { get; set; }
    public string PU_TOPICS_IN_CONTEXT_CARDS_IOS { get; set; }
    public string RANKING_LOGGING_CONTROL_IOS { get; set; }
    public string SEARCH_BITMOJI_TV_PUBLISHER_ROLLOUT { get; set; }
    public string SEARCH_COF_SYNC_IOS { get; set; }
    public string SEARCH_GLOBAL_DISPLAY_NAME_V1 { get; set; }
    public string SEARCH_GLOBAL_DISPLAY_NAME_V1_5 { get; set; }
    public string SIG_FULLSCREEN_TRANSITION { get; set; }
    public string SNAP3D_ASSETS_PREFETCH_EXPERIMENT { get; set; }
    public string SNAPADS_DISCOVER_FEED_SESSION_AD { get; set; }
    public string SNAPADS_IOS_CI_STORY_AD { get; set; }
    public string SNAPADS_IOS_CLIENT_ADLOAD_AUTOADVANCE { get; set; }
    public string SNAPADS_IOS_CLIENT_ADLOAD_CONTENTINTERSTITIAL { get; set; }
    public string SNAPADS_IOS_DPA_NATIVE_CTA { get; set; }
    public string SNAPADS_IOS_DYNAMIC_PUBLISHER_INSERTION { get; set; }
    public string SNAPADS_IOS_ENABLE_BRAND_NAME_PROFILE { get; set; }
    public string SNAPADS_IOS_ENABLE_SHOWCASE { get; set; }
    public string SNAPADS_IOS_FUS_STORY_AD { get; set; }
    public string SNAPADS_IOS_SHOWCASE_RANKING { get; set; }
    public string SNAPADS_MUSHROOM_CLIENT_ADLOAD_AUTOADVANCE { get; set; }
    public string SNAPADS_MUSHROOM_CLIENT_ADLOAD_CONTENTINTERSTITIAL { get; set; }
    public string SNAPADS_SHOW_DYNAMIC_INSERTION { get; set; }
    public string SNAPADS_USER_STORIES_AD_UNSKIPPABLE_IOS { get; set; }
    public string SNAP_3D_IOS_DEPTH_FILTERING { get; set; }
    public string SNAP_3D_LIVE_CAMERA_IOS { get; set; }
    public string SNAP_3D_VIEWING { get; set; }
    public string SNAP_ADS_DISCOVER_ADS_iOS { get; set; }
    public string SNAP_ADS_OURSTORY_ADS_iOS { get; set; }
    public string SNAP_ADS_USERSTORY_ADS_iOS { get; set; }
    public string SNAP_ADS_iOS_GATING { get; set; }
    public string SONIC_IOS_CAPTURE { get; set; }
    public string SPECS_BUY_BUTTON_SETTINGS { get; set; }
    public string SPECTACLES_IOS_DEVELOPER_MODE { get; set; }
    public string SPECTRUM_IOS { get; set; }
    public string SPOTLIGHT_MANAGEMENT_IOS { get; set; }
    public string ST_MEMORIES_LENS_LINK_SHARING_IOS { get; set; }
    public string ST_MY_PROFILE_STORY_LINK_SHARING_IOS { get; set; }
    public string ST_SPOTLIGHT_SHARE_SHEET_IOS { get; set; }
    public string TALK_NO_LENSES_IOS { get; set; }
    public string UNLOCKABLES_DOGOOD { get; set; }
    public string UP_POLAROID_IOS { get; set; }
    public string USER_AND_FRIENDS_ON_AWS { get; set; }
    public string USE_CALLING_SERVICE_SEND_TALK_PUSH_NOTIFICATION { get; set; }
    public string caption_v2_locale_support { get; set; }
    public string carnaval_2018 { get; set; }
    public string carnaval_italy_2018 { get; set; }
    public string dynamic_caption_android_nonlatin { get; set; }
    public string iOS_NON_FATAL_ENABLED { get; set; }
    public string mothers_day_norway_2018 { get; set; }
}

public class UpdatesResponse
{
    public bool amr_codec_enabled_android { get; set; }
    public bool android_media_recorder_surface_recording_enabled { get; set; }
    public bool audio_note_enabled_android { get; set; }
    public string birthday { get; set; }
    public bool bitmoji_hide_download_prompt { get; set; }
    public string blizzard_token { get; set; }
    public bool blur_after_downscale_enabled_android { get; set; }
    public bool camera1_take_photo_api_blacklisted_android { get; set; }
    public bool camera1_take_photo_api_whitelisted_android { get; set; }
    public bool camera2_enabled_android { get; set; }
    public bool camera2_limited_level_high_resolution_photo_enabled_android { get; set; }
    public bool camera2_take_photo_api_android { get; set; }
    public bool chat_video_enabled_android { get; set; }
    public ClientProperties client_properties { get; set; }
    public long created { get; set; }
    public int credits { get; set; }
    public bool d2s_media_download_enabled { get; set; }
    public string device_token { get; set; }
    public bool dirty_video_rendering_enabled_android { get; set; }
    public bool discover_content_disabled { get; set; }
    public string display_name { get; set; }
    public string email { get; set; }
    public bool enable_separate_lens_list_for_back_camera { get; set; }
    public bool enable_world_lens_grid { get; set; }
    public bool enabled_push_notifications { get; set; }
    public List<FeatureSettingsResponse> feature_settings_response { get; set; }
    public bool front_camera_zoom_enabled_android { get; set; }
    public bool gles3_allowed_android { get; set; }
    public bool has_used_laguna { get; set; }
    public List<object> industries { get; set; }
    public bool is_affected_by_gdpr { get; set; }
    public string iso3166_alpha2_country_code { get; set; }
    public string laguna_id { get; set; }
    public int last_address_book_updated_date { get; set; }
    public bool logged { get; set; }
    public string mobile { get; set; }
    public double nft_hi_timeout { get; set; }
    public double nft_lo_timeout { get; set; }
    public int notification_privacy { get; set; }
    public string notification_sound_setting { get; set; }
    public int number_of_best_friends { get; set; }
    public string phone_number_country_code { get; set; }
    public bool pinnable_stickers_enabled_android { get; set; }
    public string quick_add_privacy { get; set; }
    public bool raw_thumbnail_upload_enabled { get; set; }
    public int received { get; set; }
    public string registration_country_code { get; set; }
    public bool require_refreshing_profile_media { get; set; }
    public bool reverse_filter_enabled_android { get; set; }
    public string ringing_sound { get; set; }
    public string said { get; set; }
    public bool samsung_aac_enc_enabled_android { get; set; }
    public bool sc_media_recorder_enabled_android { get; set; }
    public bool sc_media_recorder_recommended_android { get; set; }
    public int score { get; set; }
    public int sent { get; set; }
    public bool should_call_to_verify_number { get; set; }
    public bool smoothing_filter_enabled_android { get; set; }
    public int snap_media_upload_so_timeout { get; set; }
    public int snap_p { get; set; }
    public string snapchat_phone_number { get; set; }
    public bool speed_filters_enabled_android { get; set; }
    public bool sticker_visual_recommendation_enabled_android { get; set; }
    public int story_count { get; set; }
    public string story_privacy { get; set; }
    public StudySettings study_settings { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
    public bool video_decoder_texcoord_transformation_enabled_android { get; set; }
    public bool video_note_api_fallback_android { get; set; }
    public bool video_note_enabled_android { get; set; }
    public bool video_thumbnail_enabled_android { get; set; }
    public string voip_device_token { get; set; }
}