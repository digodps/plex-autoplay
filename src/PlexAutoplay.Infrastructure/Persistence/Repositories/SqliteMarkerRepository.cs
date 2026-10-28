namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

public class SqliteMarkerRepository : IMarkerRepository
{
    private readonly string _connectionString;

    public SqliteMarkerRepository(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var conn = CreateConnection();
        conn.ExecuteAsync(
            @"CREATE TABLE IF NOT EXISTS Markers (
                RatingKey TEXT NOT NULL,
                Kind TEXT NOT NULL,
                StartMs INTEGER NOT NULL,
                EndMs INTEGER NOT NULL,
                Origin TEXT NOT NULL DEFAULT 'manual',
                PRIMARY KEY (RatingKey, Kind)
            )").Wait();

        // Migração aditiva: bancos criados antes da coluna Origin ganham a coluna com default 'manual'.
        try
        {
            conn.ExecuteAsync(
                "ALTER TABLE Markers ADD COLUMN Origin TEXT NOT NULL DEFAULT 'manual'").Wait();
        }
        catch
        {
            // Coluna já existe (ex.: "duplicate column name: Origin") — nada a fazer.
        }
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<IEnumerable<Marker>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Marker>(
            "SELECT RatingKey, Kind, StartMs, EndMs, Origin FROM Markers");
    }

    public async Task<IEnumerable<Marker>> GetByRatingKeyAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Marker>(
            "SELECT RatingKey, Kind, StartMs, EndMs, Origin FROM Markers WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }

    public async Task AddAsync(Marker marker)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO Markers (RatingKey, Kind, StartMs, EndMs, Origin) VALUES (@RatingKey, @Kind, @StartMs, @EndMs, @Origin)",
            marker);
    }

    public async Task UpdateAsync(Marker marker)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Markers SET StartMs = @StartMs, EndMs = @EndMs WHERE RatingKey = @RatingKey AND Kind = @Kind",
            marker);
    }

    public async Task DeleteAsync(string ratingKey, string kind)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM Markers WHERE RatingKey = @RatingKey AND Kind = @Kind",
            new { RatingKey = ratingKey, Kind = kind });
    }

    public async Task<bool> ExistsAsync(string ratingKey, string kind)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Markers WHERE RatingKey = @RatingKey AND Kind = @Kind",
            new { RatingKey = ratingKey, Kind = kind });
        return count > 0;
    }
}
