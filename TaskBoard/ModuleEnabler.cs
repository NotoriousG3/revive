using SnapWebModels;

namespace TaskBoard;

public class ModuleEnabler
{
    private static readonly SnapWebModuleId[] _otherModules = {SnapWebModuleId.Subscribe, SnapWebModuleId.ReportUserRandom, SnapWebModuleId.ViewBusinessPublicStory, SnapWebModuleId.ReportUserPublicProfileRandom, SnapWebModuleId.Test};
    private static readonly SnapWebModuleId[] _relationshipModules = {SnapWebModuleId.AddFriend, SnapWebModuleId.AcceptFriend};
    private readonly AppSettingsLoader _settingsLoader;

    public ModuleEnabler(AppSettingsLoader loader)
    {
        _settingsLoader = loader;
    }

    public async Task<bool> IsEnabled(SnapWebModuleId moduleId)
    {
        var settings = await _settingsLoader.Load();

        return settings.EnabledModules.Any(e => e.ModuleId == moduleId);
    }

    public async Task<bool> ShowPrivateActions()
    {
        var settings = await _settingsLoader.Load();

        return settings.EnabledModules.Any(e => _otherModules.Contains(e.ModuleId));
    }

    public async Task<bool> ShowRelationshipActions()
    {
        var settings = await _settingsLoader.Load();

        return settings.EnabledModules.Any(e => _relationshipModules.Contains(e.ModuleId));
    }
}