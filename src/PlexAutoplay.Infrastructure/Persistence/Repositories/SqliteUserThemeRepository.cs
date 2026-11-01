namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Data.Common;

public class SqliteUserThemeRepository : IUserThemeRepository
{
    private readonly string _connectionString;

    public SqliteUserThemeRepository(string connectionString)
    {
        _connectionString = connectionString;
        // Garante que o diretório do banco existe (evita SQLite Error 14 em Docker/ZimaOS)
        var dir = Path.GetDirectoryName(_connectionString);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var connection = CreateConnection();
        connection.Execute(@"CREATE TABLE IF NOT EXISTS UserThemePreferences (
            UserId TEXT NOT NULL PRIMARY KEY,
            AccentColor TEXT NOT NULL,
            Mode TEXT NOT NULL
        )");
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<UserThemePreference?> GetAsync(string userId)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserThemePreference>(
            "SELECT UserId, AccentColor, Mode FROM UserThemePreferences WHERE UserId = @UserId",
            new { UserId = userId });
    }

    public async Task UpsertAsync(UserThemePreference preference)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync(@"INSERT INTO UserThemePreferences (UserId, AccentColor, Mode)
            VALUES (@UserId, @AccentColor, @Mode)
            ON CONFLICT(UserId) DO UPDATE SET AccentColor = excluded.AccentColor, Mode = excluded.Mode", preference);
    }
}
