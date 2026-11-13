namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

public class SqliteSkipHistoryRepository : ISkipHistoryRepository
{
    private readonly string _connectionString;

    public SqliteSkipHistoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddAsync(SkipHistoryEntry entry)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO SkipHistory (Timestamp, MachineIdentifier, Product, Platform, RatingKey, MarkerKind, SkippedFromMs, SkippedToMs)
              VALUES (@Timestamp, @MachineIdentifier, @Product, @Platform, @RatingKey, @MarkerKind, @SkippedFromMs, @SkippedToMs)",
            new
            {
                Timestamp = entry.Timestamp.ToString("o"),
                entry.MachineIdentifier,
                entry.Product,
                entry.Platform,
                entry.RatingKey,
                entry.MarkerKind,
                entry.SkippedFromMs,
                entry.SkippedToMs
            });
    }

    public async Task<IEnumerable<SkipHistoryEntry>> GetRecentAsync(int limit)
    {
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync<SkipHistoryRow>(
            @"SELECT Timestamp, MachineIdentifier, Product, Platform, RatingKey, MarkerKind, SkippedFromMs, SkippedToMs
              FROM SkipHistory
              ORDER BY Id DESC
              LIMIT @Limit",
            new { Limit = limit });

        var list = new List<SkipHistoryEntry>();
        foreach (var r in rows)
        {
            if (System.DateTime.TryParse(r.Timestamp, out var dt))
            {
                list.Add(new SkipHistoryEntry(dt, r.MachineIdentifier, r.Product, r.Platform, r.RatingKey, r.MarkerKind, r.SkippedFromMs, r.SkippedToMs));
            }
        }
        return list;
    }

    private sealed class SkipHistoryRow
    {
        public string Timestamp { get; set; } = string.Empty;
        public string MachineIdentifier { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int RatingKey { get; set; }
        public string MarkerKind { get; set; } = string.Empty;
        public long SkippedFromMs { get; set; }
        public long SkippedToMs { get; set; }
    }
}
