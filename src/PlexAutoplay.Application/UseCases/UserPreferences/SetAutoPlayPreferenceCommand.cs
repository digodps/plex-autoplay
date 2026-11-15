namespace PlexAutoplay.Application.UseCases.UserPreferences;

using PlexAutoplay.Application;

public class SetAutoPlayPreferenceCommand
{
    private readonly IAppSettingsStore _settingsStore;

    public SetAutoPlayPreferenceCommand(IAppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public async Task ExecuteAsync(bool enabled)
    {
        var settings = await _settingsStore.GetAsync();
        await _settingsStore.SaveAsync(settings with { AutoPlayEnabled = enabled });
    }
}