using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

/// <summary>Repositório SQLite para jobs de varredura.</summary>
public sealed class SqliteScanJobRepository : IScanJobRepository
{
    private readonly string _connectionString;

    public SqliteScanJobRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddAsync(ScanJob job)
    {
        const string sql = """
            INSERT INTO ScanJob (JobId, Status, Processed, Skipped, Errors, ErrorMessage, CreatedAt, UpdatedAt)
            VALUES (@JobId, @Status, @Processed, @Skipped, @Errors, @ErrorMessage, @CreatedAt, @UpdatedAt)
            """;

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(sql, new
        {
            job.JobId,
            job.Status,
            job.Processed,
            job.Skipped,
            job.Errors,
            job.ErrorMessage,
            CreatedAt = job.CreatedAt.ToString("o"),
            UpdatedAt = job.UpdatedAt.ToString("o")
        });
    }

    public async Task<ScanJob?> GetAsync(string jobId)
    {
        const string sql = "SELECT * FROM ScanJob WHERE JobId = @JobId";

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { JobId = jobId });
        return row != null ? MapJob(row) : null;
    }

    public async Task<ScanJob?> GetLatestAsync()
    {
        const string sql = "SELECT * FROM ScanJob ORDER BY UpdatedAt DESC LIMIT 1";

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        var row = await conn.QueryFirstOrDefaultAsync(sql);
        return row != null ? MapJob(row) : null;
    }

    public async Task<List<ScanJob>> GetHistoryAsync(int page = 1, int pageSize = 100)
    {
        var offset = (page - 1) * pageSize;
        const string sql = "SELECT * FROM ScanJob ORDER BY UpdatedAt DESC LIMIT @Limit OFFSET @Offset";

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync(sql, new { Limit = pageSize, Offset = offset });
        return rows.Select(MapJob).ToList();
    }

    public async Task UpdateAsync(ScanJob job)
    {
        const string sql = """
            UPDATE ScanJob
            SET Status = @Status, Processed = @Processed, Skipped = @Skipped, Errors = @Errors,
                ErrorMessage = @ErrorMessage, UpdatedAt = @UpdatedAt
            WHERE JobId = @JobId
            """;

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(sql, new
        {
            job.JobId,
            job.Status,
            job.Processed,
            job.Skipped,
            job.Errors,
            job.ErrorMessage,
            UpdatedAt = job.UpdatedAt.ToString("o")
        });
    }

    public async Task<int> ClearJobsAsync()
    {
        const string sql = "DELETE FROM ScanJob";

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        return await conn.ExecuteAsync(sql);
    }

    private static ScanJob MapJob(dynamic row) => new ScanJob
    {
        JobId = row.JobId,
        Status = row.Status,
        Processed = row.Processed,
        Skipped = row.Skipped,
        Errors = row.Errors,
        ErrorMessage = row.ErrorMessage,
        CreatedAt = DateTime.Parse(row.CreatedAt),
        UpdatedAt = DateTime.Parse(row.UpdatedAt)
    };
}