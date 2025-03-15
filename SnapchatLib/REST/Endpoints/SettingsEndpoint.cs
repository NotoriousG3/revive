using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Com.Snapchat.Deltaforce;

namespace SnapchatLib.REST.Endpoints;

public interface ISettingsEndpoint
{
    Task<string> ResendVerifyEmail();
    Task<string> DisableQuickAdd();
    Task<string> EnableQuickAdd();
    Task<string> DisableQuickAddAndroid();
    Task<string> EnableQuickAddAndroid();
    Task<string> DisableQuickAddIOS();
    Task<string> EnableQuickAddIOS();
    Task<string> MakeStoryPublic();
    Task<string> MakeStoryPublicIOS();
    Task<string> MakeStoryPublicAndroid();
    Task<string> MakeStoryFriendsOnly();
    Task<string> MakeStoryFriendsOnlyAndroid();
    Task<string> MakeStoryFriendsOnlyIOS();
    Task MakeSnapPublic();
    Task MakeSnapPrivate();
}

internal class SettingsEndpoint : EndpointAccessor, ISettingsEndpoint
{
    internal static readonly EndpointInfo AndroidEndpointInfo = new() { Url = "/ph/settings", Requirements = EndpointRequirements.Username | EndpointRequirements.RequestToken | EndpointRequirements.XSnapchatUUID };
    internal static readonly EndpointInfo IOSEndpointInfo = new() { Url = "/bq/settings", Requirements = EndpointRequirements.Username | EndpointRequirements.DSIG | EndpointRequirements.RequestToken | EndpointRequirements.XSnapchatUUID };

    public SettingsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ResendVerifyEmail()
    {
        var targetEndpoint = Config.OS == OS.android ? AndroidEndpointInfo : IOSEndpointInfo;

        var parameters = new Dictionary<string, string>
        {
            {"snapchat_user_id", Config.user_id},
            {"action", "verifyEmail"}
        };

        var response = await Send(targetEndpoint, parameters);
        return await response.Content.ReadAsStringAsync();
    }
    private async Task<string> ChangeQuickAdd(string privacySetting)
    {
        var targetEndpoint = Config.OS == OS.android ? AndroidEndpointInfo : IOSEndpointInfo;

        var parameters = new Dictionary<string, string>
        {
            {"snapchat_user_id", Config.user_id},
            {"action", "updateQuickAddPrivacy"},
            {"privacySetting", privacySetting}
        };
        var response = await Send(targetEndpoint, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public Task<string> DisableQuickAdd()
    {
        return ChangeQuickAdd("NO_ONE");
    }

    public Task<string> EnableQuickAdd()
    {
        return ChangeQuickAdd("EVERYONE");
    }

    [Obsolete("Please use DisableQuickAdd which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> DisableQuickAddAndroid()
    {
        return DisableQuickAdd();
    }

    [Obsolete("Please use EnableQuickAdd which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> EnableQuickAddAndroid()
    {
        return EnableQuickAdd();
    }

    [Obsolete("Please use DisableQuickAdd which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> DisableQuickAddIOS()
    {
        return DisableQuickAdd();
    }

    [Obsolete("Please use EnableQuickAdd which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> EnableQuickAddIOS()
    {
        return EnableQuickAdd();
    }


    private async Task<string> ChangeStoryPrivacySetting(string privacySetting)
    {
        var targetEndpoint = Config.OS == OS.android ? AndroidEndpointInfo : IOSEndpointInfo;

        var parameters = new Dictionary<string, string>
        {
            {"snapchat_user_id", Config.user_id},
            {"action", "updateStoryPrivacy"},
            {"privacySetting", privacySetting},
            {"storyFriendsIdsToBlock", "[]"}
        };
        var response = await Send(targetEndpoint, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public Task<string> MakeStoryPublic()
    {
        return ChangeStoryPrivacySetting("EVERYONE");
    }

    [Obsolete("Please use MakeStoryPublic which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> MakeStoryPublicIOS()
    {
        return MakeStoryPublic();
    }

    [Obsolete("Please use MakeStoryPublic which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> MakeStoryPublicAndroid()
    {
        return MakeStoryPublic();
    }

    public Task<string> MakeStoryFriendsOnly()
    {
        return ChangeStoryPrivacySetting("FRIENDS");
    }

    [Obsolete("Please use MakeStoryFriendsOnly which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> MakeStoryFriendsOnlyAndroid()
    {
        return MakeStoryFriendsOnly();
    }

    [Obsolete("Please use MakeStoryFriendsOnly which handles Android/IOS specific behavior based on the value of SnapchatConfig.Android")]
    public Task<string> MakeStoryFriendsOnlyIOS()
    {
        return MakeStoryFriendsOnly();
    }

    public async Task MakeSnapPublic()
    {
        var _properties = new Dictionary<string, SCDeltaforceValue>
        {
            { "23", new SCDeltaforceValue { BoolP = true } },
            { "24", new SCDeltaforceValue { BoolP = true } },
            { "9", new SCDeltaforceValue { LongP = 2 } }
        };
        var _Request = new ConditionalPutRequest
        {
            Item = new SCDeltaforceItem
            {
                Key = new SCDeltaforceItemKey
                {
                    Group = new SCDeltaforceGroupKey
                    {
                        Kind = "SnapPrivacy",
                        Name = "2c9272d2-2064-4a95-a9ff-f88fde02bfef"
                    }
                },
                Property = { _properties }
            },
            ReturnGroupState = true
        };

        await SnapchatGrpcClient.ConditionalPutAsync(_Request);
    }

    public async Task MakeSnapPrivate()
    {
        var _properties = new Dictionary<string, SCDeltaforceValue>
        {
            { "23", new SCDeltaforceValue { BoolP = true } },
            { "24", new SCDeltaforceValue { BoolP = false } },
            { "9", new SCDeltaforceValue { LongP = 1 } }
        };
        var _Request = new ConditionalPutRequest
        {
            Item = new SCDeltaforceItem
            {
                Key = new SCDeltaforceItemKey
                {
                    Group = new SCDeltaforceGroupKey
                    {
                        Kind = "SnapPrivacy",
                        Name = "2c9272d2-2064-4a95-a9ff-f88fde02bfef"
                    }
                },
                Property = { _properties }
            },
            ReturnGroupState = true
        };

        await SnapchatGrpcClient.ConditionalPutAsync(_Request);
    }
}