namespace PlexAutoplay.Application.UseCases.UserPreferences;

using PlexAutoplay.Application;

public class GetAutoPlayPreferenceQuery
{
    private readonly IAppSettingsStore _settingsStore;

    public GetAutoPlayPreferenceQuery(IAppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public async Task<bool> GetAsync()
    {
        var settings = await _settingsStore.GetAsync();
        return settings.AutoPlayEnabled;
    }
}