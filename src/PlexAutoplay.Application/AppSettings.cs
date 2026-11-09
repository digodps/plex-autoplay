namespace PlexAutoplay.Application;

/// <summary>Chaves persistidas na tabela AppSetting.</summary>
public static class AppSettingKeys
{
    public const string PlexUrl = "PLEX_URL";
    public const string PlexToken = "PLEX_TOKEN";

    /// <summary>Chave legada (uma raiz). Lida apenas para migração.</summary>
    public const string MediaRoot = "MEDIA_ROOT";

    public const string MediaRoots = "MEDIA_ROOTS";
    public const string SyncIntervalMinutes = "SYNC_INTERVAL_MINUTES";
    public const string SyncCatalogIntervalMinutes = "SYNC_CATALOG_INTERVAL_MINUTES";
    public const string AutoSkipAuthorized = "AUTO_SKIP_AUTHORIZED";

    public static readonly string[] All =
    [
        PlexUrl,
        PlexToken,
        MediaRoots,
        SyncIntervalMinutes,
        SyncCatalogIntervalMinutes,
        AutoSkipAuthorized
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
    public const bool AutoSkipAuthorized = false;

    public static IReadOnlyList<string> MediaRoots => [MediaRoot];
}

/// <summary>Configurações operacionais tipadas, lidas do SQLite.</summary>
public sealed record AppSettingsSnapshot(
    string PlexUrl,
    string PlexToken,
    IReadOnlyList<string> MediaRoots,
    int SyncIntervalMinutes,
    int SyncCatalogIntervalMinutes,
    bool AutoSkipAuthorized = AppSettingDefaults.AutoSkipAuthorized)
{
    public bool PlexTokenSet => !string.IsNullOrEmpty(PlexToken);

    public static AppSettingsSnapshot Defaults => new(
        AppSettingDefaults.PlexUrl,
        AppSettingDefaults.PlexToken,
        AppSettingDefaults.MediaRoots,
        AppSettingDefaults.SyncIntervalMinutes,
        AppSettingDefaults.SyncCatalogIntervalMinutes,
        AppSettingDefaults.AutoSkipAuthorized);
}

/// <summary>Porta de leitura/gravação das configurações operacionais no SQLite.</summary>
public interface IAppSettingsStore
{
    Task<AppSettingsSnapshot> GetAsync();
    Task SaveAsync(AppSettingsSnapshot settings);
}
