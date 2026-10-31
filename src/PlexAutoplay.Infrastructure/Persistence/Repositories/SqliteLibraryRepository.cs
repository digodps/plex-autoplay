namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Data.Common;
using System.Threading.Tasks;

/// <summary>Repositório SQLite para bibliotecas.</summary>
public class SqliteLibraryRepository : ILibraryRepository
{
    private readonly string _connectionString;

    public SqliteLibraryRepository(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var conn = CreateConnection();
        conn.ExecuteAsync(
            @"CREATE TABLE IF NOT EXISTS Library (
                RatingKey TEXT NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                Type TEXT NOT NULL,
                Agent TEXT NOT NULL,
                SyncedAt TEXT NOT NULL DEFAULT (datetime('now'))
            )").Wait();
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<IEnumerable<Library>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Library>(
            "SELECT RatingKey, Title, Type, Agent FROM Library");
    }

    public async Task<Library?> GetByRatingKeyAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Library>(
            "SELECT RatingKey, Title, Type, Agent FROM Library WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }

    public async Task UpsertAsync(Library library)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO Library (RatingKey, Title, Type, Agent, SyncedAt)
              VALUES (@RatingKey, @Title, @Type, @Agent, datetime('now'))
              ON CONFLICT(RatingKey) DO UPDATE SET
                Title = excluded.Title,
                Type = excluded.Type,
                Agent = excluded.Agent,
                SyncedAt = datetime('now')",
            library);
    }

    public async Task DeleteAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM Library WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }
}