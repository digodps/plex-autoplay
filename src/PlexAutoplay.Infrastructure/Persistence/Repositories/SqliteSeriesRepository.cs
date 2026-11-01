namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Data.Common;
using System.Threading.Tasks;

/// <summary>Repositório SQLite para séries.</summary>
public class SqliteSeriesRepository : ISeriesRepository
{
    private readonly string _connectionString;

    public SqliteSeriesRepository(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var conn = CreateConnection();
        conn.ExecuteAsync(
            @"CREATE TABLE IF NOT EXISTS Series (
                RatingKey TEXT NOT NULL PRIMARY KEY,
                LibraryRatingKey TEXT NOT NULL,
                Title TEXT NOT NULL,
                Summary TEXT NOT NULL,
                Thumb TEXT NOT NULL,
                SyncedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (LibraryRatingKey) REFERENCES Library(RatingKey) ON DELETE CASCADE
            )").Wait();
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<(IEnumerable<Series> Items, int Total)> GetByLibraryAsync(string libraryRatingKey, int offset, int limit, string? query)
    {
        using var conn = CreateConnection();
        var parameters = new Dictionary<string, object>
        {
            ["LibraryRatingKey"] = libraryRatingKey,
            ["Query"] = string.IsNullOrEmpty(query) ? "%" : $"%{query}%",
            ["Offset"] = offset,
            ["Limit"] = limit,
        };

        var items = await conn.QueryAsync<Series>(
            @"SELECT RatingKey, LibraryRatingKey, Title, Summary, Thumb 
              FROM Series 
              WHERE LibraryRatingKey = @LibraryRatingKey 
              AND Title LIKE @Query 
              ORDER BY Title 
              LIMIT @Limit OFFSET @Offset",
            parameters);

        var total = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM Series 
              WHERE LibraryRatingKey = @LibraryRatingKey 
              AND Title LIKE @Query",
            parameters);

        return (items, total);
    }

    public async Task<Series?> GetByRatingKeyAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Series>(
            "SELECT RatingKey, LibraryRatingKey, Title, Summary, Thumb FROM Series WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }

    public async Task UpsertAsync(Series series)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO Series (RatingKey, LibraryRatingKey, Title, Summary, Thumb, SyncedAt)
              VALUES (@RatingKey, @LibraryRatingKey, @Title, @Summary, @Thumb, datetime('now'))
              ON CONFLICT(RatingKey) DO UPDATE SET
                LibraryRatingKey = excluded.LibraryRatingKey,
                Title = excluded.Title,
                Summary = excluded.Summary,
                Thumb = excluded.Thumb,
                SyncedAt = datetime('now')",
            series);
    }

    public async Task DeleteAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM Series WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }

    public async Task DeleteByLibraryAsync(string libraryRatingKey)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM Series WHERE LibraryRatingKey = @LibraryRatingKey",
            new { LibraryRatingKey = libraryRatingKey });
    }
}