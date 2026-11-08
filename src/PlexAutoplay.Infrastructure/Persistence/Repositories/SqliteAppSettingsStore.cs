namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Application;

/// <summary>Configurações operacionais persistidas no SQLite (tabela AppSetting).</summary>
public sealed class SqliteAppSettingsStore : IAppSettingsStore
{
    private readonly string _connectionString;

    public SqliteAppSettingsStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<AppSettingsSnapshot> GetAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        var rows = await connection.QueryAsync<(string Key, string Value)>(
            "SELECT Key, Value FROM AppSetting");

        var values = rows.ToDictionary(row => row.Key, row => row.Value);
        var defaults = AppSettingsSnapshot.Defaults;

        return new AppSettingsSnapshot(
            Get(values, AppSettingKeys.PlexUrl, defaults.PlexUrl),
            Get(values, AppSettingKeys.PlexToken, defaults.PlexToken),
            Get(values, AppSettingKeys.MediaRoot, defaults.MediaRoot),
            GetInt(values, AppSettingKeys.SyncIntervalMinutes, defaults.SyncIntervalMinutes),
            GetInt(values, AppSettingKeys.SyncCatalogIntervalMinutes, defaults.SyncCatalogIntervalMinutes));
    }

    public async Task SaveAsync(AppSettingsSnapshot settings)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var values = new Dictionary<string, string>
        {
            [AppSettingKeys.PlexUrl] = settings.PlexUrl,
            [AppSettingKeys.PlexToken] = settings.PlexToken,
            [AppSettingKeys.MediaRoot] = settings.MediaRoot,
            [AppSettingKeys.SyncIntervalMinutes] = settings.SyncIntervalMinutes.ToString(),
            [AppSettingKeys.SyncCatalogIntervalMinutes] = settings.SyncCatalogIntervalMinutes.ToString()
        };

        foreach (var (key, value) in values)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO AppSetting (Key, Value) VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value, UpdatedAt = datetime('now')
                """,
                new { Key = key, Value = value },
                transaction);
        }

        await transaction.CommitAsync();
    }

    private static string Get(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.TryGetValue(key, out var value) ? value : fallback;

    private static int GetInt(IReadOnlyDictionary<string, string> values, string key, int fallback) =>
        values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;
}

/// <summary>
/// Copia configurações do ambiente/padrões para o banco apenas no primeiro
/// start (tabela vazia). Não re-semeia valores removidos depois.
/// </summary>
public static class AppSettingsSeeder
{
    public static async Task SeedAsync(string connectionString)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM AppSetting");
        if (count > 0)
        {
            return;
        }

        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var key in AppSettingKeys.All)
        {
            var fallback = key switch
            {
                AppSettingKeys.PlexUrl => AppSettingDefaults.PlexUrl,
                AppSettingKeys.PlexToken => AppSettingDefaults.PlexToken,
                AppSettingKeys.MediaRoot => AppSettingDefaults.MediaRoot,
                AppSettingKeys.SyncIntervalMinutes => AppSettingDefaults.SyncIntervalMinutes.ToString(),
                AppSettingKeys.SyncCatalogIntervalMinutes => AppSettingDefaults.SyncCatalogIntervalMinutes.ToString(),
                _ => ""
            };

            var value = Environment.GetEnvironmentVariable(key) ?? fallback;
            await connection.ExecuteAsync(
                "INSERT INTO AppSetting (Key, Value) VALUES (@Key, @Value)",
                new { Key = key, Value = value },
                transaction);
        }

        await transaction.CommitAsync();
    }
}
