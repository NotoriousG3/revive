using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IPhoneVerifyEndpoint
{
    Task<string> ChangePhone(string number, string countrycode);
    Task<string> VerifyPhone(string code);
}

internal class PhoneVerifyEndpoint : EndpointAccessor, IPhoneVerifyEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new () {Url = "/bq/phone_verify", Requirements = EndpointRequirements.Username | EndpointRequirements.XSnapAccessToken | EndpointRequirements.RequestToken };

    public PhoneVerifyEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ChangePhone(string number, string countrycode)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "updatePhoneNumber"},
            {"client_id", Config.ClientID},
            {"countryCode", countrycode},
            {"method", "text"},
            {"phoneNumber", number},
            {"reset_password_in_app", "false"},
            {"snapchat_user_id", Config.user_id},
            {"type", "SETTINGS_PHONE_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> VerifyPhone(string code)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "verifyPhoneNumber"},
            {"client_id", Config.ClientID},
            {"code", code},
            {"reset_password_in_app", "false"},
            {"is_from_deep_link", "false"},
            {"snapchat_user_id", Config.user_id},
            {"type", "SETTINGS_PHONE_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }
}