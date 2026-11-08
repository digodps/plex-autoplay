namespace PlexAutoplay.Application;

/// <summary>Chaves persistidas na tabela AppSetting.</summary>
public static class AppSettingKeys
{
    public const string PlexUrl = "PLEX_URL";
    public const string PlexToken = "PLEX_TOKEN";
    public const string MediaRoot = "MEDIA_ROOT";
    public const string SyncIntervalMinutes = "SYNC_INTERVAL_MINUTES";
    public const string SyncCatalogIntervalMinutes = "SYNC_CATALOG_INTERVAL_MINUTES";

    public static readonly string[] All =
    [
        PlexUrl,
        PlexToken,
        MediaRoot,
        SyncIntervalMinutes,
        SyncCatalogIntervalMinutes
    ];
}

/// <summary>Valores usados quando a chave não existe no banco nem no ambiente.</summary>
public static class AppSettingDefaults
{
    public const string PlexUrl = "http://localhost:32400";
    public const string PlexToken = "";
    public const string MediaRoot = "/media";
    public const int SyncIntervalMinutes = 5;
    public const int SyncCatalogIntervalMinutes = 30;
}

/// <summary>Configurações operacionais tipadas, lidas do SQLite.</summary>
public sealed record AppSettingsSnapshot(
    string PlexUrl,
    string PlexToken,
    string MediaRoot,
    int SyncIntervalMinutes,
    int SyncCatalogIntervalMinutes)
{
    public bool PlexTokenSet => !string.IsNullOrEmpty(PlexToken);

    public static AppSettingsSnapshot Defaults => new(
        AppSettingDefaults.PlexUrl,
        AppSettingDefaults.PlexToken,
        AppSettingDefaults.MediaRoot,
        AppSettingDefaults.SyncIntervalMinutes,
        AppSettingDefaults.SyncCatalogIntervalMinutes);
}

/// <summary>Porta de leitura/gravação das configurações operacionais no SQLite.</summary>
public interface IAppSettingsStore
{
    Task<AppSettingsSnapshot> GetAsync();
    Task SaveAsync(AppSettingsSnapshot settings);
}
