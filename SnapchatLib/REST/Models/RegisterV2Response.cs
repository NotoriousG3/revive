using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class CofResponse
{
    public string base64_encoded_response { get; set; }
}

public class fidelius_client_init
{
    public int new_fidelius_version { get; set; }
    public List<string> hashed_out_betas { get; set; }
    public string new_hashed_out_beta { get; set; }
    public string new_iwek { get; set; }
    public string new_out_beta { get; set; }
}

public class VerificationNeeded
{
    public string type { get; set; }
    public string prompt { get; set; }
}

public class UserSessionInit
{
    public string user_id { get; set; }
    public string snapads_id { get; set; }
    public string laguna_id { get; set; }
    public string auth_token { get; set; }
}

public class BootstrapData
{
    public string CoreData { get; set; }
    public string UserScore { get; set; }
    public string SUP { get; set; }
    public string NotificationData { get; set; }
}

public class GatewayAuthToken
{
    public string payload { get; set; }
    public string mac { get; set; }
}

public class MessagingGatewayInfo
{
    public GatewayAuthToken gateway_auth_token { get; set; }
    public string gateway_server { get; set; }
}

public class RegisterV2Response
{
    public string hashed_out_beta { get; set; }
    public UpdatesJson UpdatesJson { get; set; }
    public CofResponse cof_response { get; set; }
    public IdentityCheckJson IdentityCheckJson { get; set; }
    public VerificationNeeded verification_needed { get; set; }
    public MessagingGatewayInfo messaging_gateway_info { get; set; }
    public string dtoken1i { get; set; }
    public string dtoken1v { get; set; }
    public string preferred_verification_method { get; set; }
    public UserSessionInit user_session_init { get; set; }
    public BootstrapData bootstrap_data { get; set; }
}