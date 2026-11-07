namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Application;
using System.Data.Common;

/// <summary>
/// Credencial administrativa única (UserId = admin) persistida no SQLite.
/// </summary>
public class SqliteAuthCredentialStore : IAuthCredentialStore
{
    private const string UserId = "admin";
    private readonly string _connectionString;

    public SqliteAuthCredentialStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<bool> IsConfiguredAsync()
    {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<long>(
            "SELECT EXISTS(SELECT 1 FROM AuthCredential WHERE UserId = @UserId)",
            new { UserId }) == 1;
    }

    public async Task<string?> GetHashAsync()
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT PasswordHash FROM AuthCredential WHERE UserId = @UserId",
            new { UserId });
    }

    public async Task<bool> CreateAsync(string passwordHash)
    {
        using var connection = CreateConnection();
        var rows = await connection.ExecuteAsync(
            "INSERT OR IGNORE INTO AuthCredential (UserId, PasswordHash, UpdatedAt) VALUES (@UserId, @PasswordHash, datetime('now'))",
            new { UserId, PasswordHash = passwordHash });
        return rows == 1;
    }

    public async Task<bool> UpdateAsync(string passwordHash)
    {
        using var connection = CreateConnection();
        var rows = await connection.ExecuteAsync(
            "UPDATE AuthCredential SET PasswordHash = @PasswordHash, UpdatedAt = datetime('now') WHERE UserId = @UserId",
            new { UserId, PasswordHash = passwordHash });
        return rows == 1;
    }
}
