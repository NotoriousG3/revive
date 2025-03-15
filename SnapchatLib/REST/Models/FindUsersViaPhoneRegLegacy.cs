using System.Collections.Generic;

namespace SnapchatLib.REST.Models;
public class FindUsersViaPhoneResultsLegacy
{
    public string content_suggestion_page_position { get; set; }
    public bool is_trimmed { get; set; }
    public int last_address_book_updated_date { get; set; }
    public List<object> registration_suggested_friend_ordering { get; set; }
    public List<object> results { get; set; }
    public List<object> suggested_friend_results_v2 { get; set; }
}

public class FindUsersViaPhoneLegacy
{
    public List<object> number { get; set; }
    public string display_name { get; set; }
    public long last_updated_timestamp { get; set; }
    public bool hasStarred { get; set; }
    public bool hasPhoto { get; set; }
    public bool hasSavedDate { get; set; }
    public List<string> email_address { get; set; }
}


