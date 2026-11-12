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

        var mediaRoots = MediaRoots.Parse(Get(values, AppSettingKeys.MediaRoots, ""));
        if (mediaRoots.Count == 0)
        {
            var legacy = Get(values, AppSettingKeys.MediaRoot, defaults.MediaRoots[0]);
            mediaRoots = [legacy];
        }

        return new AppSettingsSnapshot(
            Get(values, AppSettingKeys.PlexUrl, defaults.PlexUrl),
            Get(values, AppSettingKeys.PlexToken, defaults.PlexToken),
            mediaRoots,
            GetInt(values, AppSettingKeys.SyncIntervalMinutes, defaults.SyncIntervalMinutes));
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
            [AppSettingKeys.MediaRoots] = MediaRoots.Serialize(settings.MediaRoots),
            [AppSettingKeys.SyncIntervalMinutes] = settings.SyncIntervalMinutes.ToString()
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
/// start (tabela vazia) e migra bancos antigos que só têm MEDIA_ROOT.
/// </summary>
public static class AppSettingsSeeder
{
    public static async Task SeedAsync(string connectionString)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM AppSetting");
        if (count == 0)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var key in AppSettingKeys.All)
            {
                var fallback = key switch
                {
                    AppSettingKeys.PlexUrl => AppSettingDefaults.PlexUrl,
                    AppSettingKeys.PlexToken => AppSettingDefaults.PlexToken,
                    AppSettingKeys.MediaRoots => MediaRoots.Serialize(MediaRootsFromEnvironment()),
                    AppSettingKeys.SyncIntervalMinutes => AppSettingDefaults.SyncIntervalMinutes.ToString(),
                    _ => ""
                };

                var value = Environment.GetEnvironmentVariable(key) ?? fallback;
                await connection.ExecuteAsync(
                    "INSERT INTO AppSetting (Key, Value) VALUES (@Key, @Value)",
                    new { Key = key, Value = value },
                    transaction);
            }

            await transaction.CommitAsync();
            return;
        }

        await MigrateMediaRootsAsync(connection);
    }

    /// <summary>Grava MEDIA_ROOTS derivado de env/MEDIA_ROOT quando a chave não existe.</summary>
    private static async Task MigrateMediaRootsAsync(SqliteConnection connection)
    {
        var existing = await connection.ExecuteScalarAsync<string>(
            "SELECT Value FROM AppSetting WHERE Key = @Key",
            new { Key = AppSettingKeys.MediaRoots });

        if (!string.IsNullOrWhiteSpace(existing))
        {
            return;
        }

        var legacy = await connection.ExecuteScalarAsync<string>(
            "SELECT Value FROM AppSetting WHERE Key = @Key",
            new { Key = AppSettingKeys.MediaRoot });

        var roots = MediaRootsFromEnvironment();
        if (roots.Count == 0 && !string.IsNullOrWhiteSpace(legacy))
        {
            roots = [legacy];
        }

        if (roots.Count == 0)
        {
            roots = AppSettingDefaults.MediaRoots;
        }

        await connection.ExecuteAsync(
            """
            INSERT INTO AppSetting (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO NOTHING
            """,
            new { Key = AppSettingKeys.MediaRoots, Value = MediaRoots.Serialize(roots) });
    }

    private static IReadOnlyList<string> MediaRootsFromEnvironment()
    {
        var roots = MediaRoots.ParseEnvironment(Environment.GetEnvironmentVariable(AppSettingKeys.MediaRoots));
        if (roots.Count > 0)
        {
            return roots;
        }

        var legacy = Environment.GetEnvironmentVariable(AppSettingKeys.MediaRoot);
        return string.IsNullOrWhiteSpace(legacy) ? [] : [legacy.Trim()];
    }
}
