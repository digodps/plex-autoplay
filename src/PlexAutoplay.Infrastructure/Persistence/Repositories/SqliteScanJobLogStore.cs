using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

/// <summary>Repositório SQLite para logs de varredura.</summary>
public sealed class SqliteScanJobLogStore : IScanJobLogStore
{
    private readonly string _connectionString;

    public SqliteScanJobLogStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddLogAsync(string jobId, string level, string message, string? suggestion = null)
    {
        const string sql = """
            INSERT INTO ScanJobLog (JobId, Timestamp, Level, Message, Suggestion)
            VALUES (@JobId, @Timestamp, @Level, @Message, @Suggestion)
            """;

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(sql, new
        {
            JobId = jobId,
            Timestamp = DateTime.UtcNow.ToString("o"),
            Level = level,
            Message = message,
            Suggestion = suggestion
        });
    }

    public async Task<List<ScanLogEntry>> GetLogsAsync(string? jobId, DateTime? from, DateTime? to, string? level, string? textFilter, int page = 1, int pageSize = 100)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(jobId))
        {
            where.Add("l.JobId = @JobId");
            parameters.Add("@JobId", jobId);
        }

        if (from.HasValue)
        {
            where.Add("l.Timestamp >= @From");
            parameters.Add("@From", from.Value.ToUniversalTime().ToString("o"));
        }

        if (to.HasValue)
        {
            where.Add("l.Timestamp <= @To");
            parameters.Add("@To", to.Value.ToUniversalTime().ToString("o"));
        }

        if (!string.IsNullOrEmpty(level))
        {
            where.Add("l.Level = @Level");
            parameters.Add("@Level", level);
        }

        if (!string.IsNullOrEmpty(textFilter))
        {
            where.Add("l.Message LIKE @TextFilter COLLATE NOCASE");
            parameters.Add("@TextFilter", $"%{textFilter}%");
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (page - 1) * pageSize;

        var sql = $"""
            SELECT l.Timestamp, l.Level, l.Message, l.Suggestion, l.JobId, j.Status AS JobStatus
            FROM ScanJobLog l
            LEFT JOIN ScanJob j ON j.JobId = l.JobId
            {whereClause}
            ORDER BY l.Timestamp DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        parameters.Add("@PageSize", pageSize);
        parameters.Add("@Offset", offset);

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync(sql, parameters);
        return rows.Select(r => new ScanLogEntry(
            DateTime.Parse((string)r.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind),
            (string)r.Message,
            (string)r.Level,
            r.Suggestion as string,
            r.JobId as string,
            r.JobStatus as string
        )).ToList();
    }

    public async Task<int> ClearLogsAsync()
    {
        const string sql = "DELETE FROM ScanJobLog";

        await using var conn = CreateConnection();
        await conn.OpenAsync();
        return await conn.ExecuteAsync(sql);
    }

    public async Task<int> CleanupOldLogsAsync(int retentionDays = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays).ToString("o");

        const string deleteLogsSql = "DELETE FROM ScanJobLog WHERE Timestamp < @Cutoff";
        const string deleteOrphanJobsSql = "DELETE FROM ScanJob WHERE JobId NOT IN (SELECT DISTINCT JobId FROM ScanJobLog)";

        await using var conn = CreateConnection();
        await conn.OpenAsync();

        var deletedLogs = await conn.ExecuteAsync(deleteLogsSql, new { Cutoff = cutoff });
        var deletedJobs = await conn.ExecuteAsync(deleteOrphanJobsSql);

        return deletedLogs + deletedJobs;
    }
}