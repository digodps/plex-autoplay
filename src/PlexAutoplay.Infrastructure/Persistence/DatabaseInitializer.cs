using Dapper;
using Microsoft.Data.Sqlite;

namespace PlexAutoplay.Infrastructure.Persistence;

/// <summary>
/// Inicializa o schema SQLite de forma assíncrona, aplicando migrações
/// versionadas cobrindo os 4 repositórios (Library, Series, Markers,
/// UserThemePreferences). Chamado no composition root (Program.cs) antes do
/// app.Run(), em vez de cada repositório criar suas tabelas no construtor.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>Connection string padrão (mesma fonte usada pelos repositórios).</summary>
    public static string DefaultConnectionString =>
        Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING") ?? "Data Source=/data/app.db";

    /// <summary>
    /// Migrações ordenadas. IgnoreError=true para migrações não-idempotentes
    /// (ex.: ALTER TABLE ADD COLUMN que falha se a coluna já existir).
    /// </summary>
    private static readonly (int Version, string Sql, bool IgnoreError)[] Migrations =
    [
        (1, """
            CREATE TABLE IF NOT EXISTS Library (
                RatingKey TEXT NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                Type TEXT NOT NULL,
                Agent TEXT NOT NULL,
                SyncedAt TEXT NOT NULL DEFAULT (datetime('now'))
            )
            """, false),
        (2, """
            CREATE TABLE IF NOT EXISTS Series (
                RatingKey TEXT NOT NULL PRIMARY KEY,
                LibraryRatingKey TEXT NOT NULL,
                Title TEXT NOT NULL,
                Summary TEXT NOT NULL,
                Thumb TEXT NOT NULL,
                SyncedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (LibraryRatingKey) REFERENCES Library(RatingKey) ON DELETE CASCADE
            )
            """, false),
        (3, """
            CREATE TABLE IF NOT EXISTS Markers (
                RatingKey TEXT NOT NULL,
                Kind TEXT NOT NULL,
                StartMs INTEGER NOT NULL,
                EndMs INTEGER NOT NULL,
                Origin TEXT NOT NULL DEFAULT 'manual',
                PRIMARY KEY (RatingKey, Kind)
            )
            """, false),
        (4, """
            CREATE TABLE IF NOT EXISTS UserThemePreferences (
                UserId TEXT NOT NULL PRIMARY KEY,
                AccentColor TEXT NOT NULL,
                Mode TEXT NOT NULL
            )
            """, false),
        (5, "ALTER TABLE Markers ADD COLUMN Origin TEXT NOT NULL DEFAULT 'manual'", true),
        (6, """
            CREATE TABLE IF NOT EXISTS AuthCredential (
                UserId TEXT NOT NULL PRIMARY KEY,
                PasswordHash TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            )
            """, false),
        (7, """
            CREATE TABLE IF NOT EXISTS AppSetting (
                Key TEXT NOT NULL PRIMARY KEY,
                Value TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            )
            """, false),
    ];

    public static async Task InitializeAsync(string connectionString)
    {
        // Garante que o diretório do banco existe (evita SQLite Error 14 em Docker/ZimaOS)
        var dir = Path.GetDirectoryName(connectionString);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("PRAGMA journal_mode=WAL");
        await conn.ExecuteAsync("PRAGMA busy_timeout=5000");
        await conn.ExecuteAsync(
            "CREATE TABLE IF NOT EXISTS _Migrations (Version INTEGER NOT NULL PRIMARY KEY, AppliedAt TEXT NOT NULL DEFAULT (datetime('now')))");

        var applied = (await conn.QueryAsync<int>("SELECT Version FROM _Migrations")).ToHashSet();

        foreach (var (version, sql, ignoreError) in Migrations)
        {
            if (applied.Contains(version))
                continue;

            try
            {
                await conn.ExecuteAsync(sql);
            }
            catch when (ignoreError)
            {
                // Migração não-idempotente já aplicada (ex.: coluna já existe).
            }

            await conn.ExecuteAsync(
                "INSERT INTO _Migrations (Version) VALUES (@Version)",
                new { Version = version });
        }
    }
}
