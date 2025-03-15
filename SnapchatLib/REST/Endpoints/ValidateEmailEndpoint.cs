using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

public interface IValidateEmailEndpoint
{
    Task<string> ValidateEmail(string email);
    Task<bool> IsValidEmail(string email);
}

internal class ValidateEmailEndpoint : EndpointAccessor, IValidateEmailEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new () { Url = "/bq/validate_email", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken };

    public ValidateEmailEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ValidateEmail(string email)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "updateEmail"},
            {"email", email}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> IsValidEmail(string email)
    {
        var response = await ValidateEmail(email);
        var parsedData = m_Utilities.JsonDeserializeObject<ValidateEmailResponse>(response);
        return parsedData.is_valid;
    }
}