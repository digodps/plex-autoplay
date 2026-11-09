using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Persistence.Repositories;
using Xunit;

namespace PlexAutoplay.Tests.Unit.Infrastructure;

public class SqliteScanJobLogStoreTests : IAsyncLifetime
{
    private readonly string _connectionString = "Data Source=:memory:;Cache=Shared";
    private SqliteConnection _conn;
    private SqliteScanJobLogStore _store;

    public async Task InitializeAsync()
    {
        _conn = new SqliteConnection(_connectionString);
        await _conn.OpenAsync();
        await _conn.ExecuteAsync("PRAGMA journal_mode=WAL");
        await _conn.ExecuteAsync("PRAGMA busy_timeout=5000");
        await _conn.ExecuteAsync("PRAGMA foreign_keys=ON");

        // Create tables (simplified from migration v8/9/10)
        await _conn.ExecuteAsync("""
            CREATE TABLE ScanJob (
                JobId TEXT PRIMARY KEY,
                Status TEXT NOT NULL,
                Processed INTEGER NOT NULL DEFAULT 0,
                Skipped INTEGER NOT NULL DEFAULT 0,
                Errors INTEGER NOT NULL DEFAULT 0,
                ErrorMessage TEXT,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            )
        """);
        await _conn.ExecuteAsync("""
            CREATE TABLE ScanJobLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId TEXT NOT NULL,
                Timestamp TEXT NOT NULL DEFAULT (datetime('now')),
                Level TEXT NOT NULL COLLATE NOCASE,
                Message TEXT NOT NULL COLLATE NOCASE,
                Suggestion TEXT,
                FOREIGN KEY (JobId) REFERENCES ScanJob(JobId) ON DELETE CASCADE
            )
        """);

        // Insert a test job for FK
        await _conn.ExecuteAsync("INSERT INTO ScanJob (JobId, Status) VALUES ('job-1', 'completed')");
        await _conn.ExecuteAsync("INSERT INTO ScanJob (JobId, Status) VALUES ('job-2', 'failed')");

        _store = new SqliteScanJobLogStore(_connectionString);
    }

    public async Task DisposeAsync()
    {
        await _conn.CloseAsync();
        _conn.Dispose();
    }

    [Fact]
    public async Task AddLogAsync_PersistsLogWithTimestampAndLevel()
    {
        // Act
        await _store.AddLogAsync("job-1", "info", "Test message", "Test suggestion");

        // Assert
        var logs = await _store.GetLogsAsync("job-1", null, null, null, null, 1, 100);
        Assert.Single(logs);
        Assert.Equal("info", logs[0].Level);
        Assert.Equal("Test message", logs[0].Message);
        Assert.Equal("Test suggestion", logs[0].Suggestion);
        Assert.Equal("job-1", logs[0].JobId);
        Assert.Equal("completed", logs[0].JobStatus);
    }

    [Fact]
    public async Task GetLogsAsync_FiltersByLevel()
    {
        await _store.AddLogAsync("job-1", "error", "Error message", null);
        await _store.AddLogAsync("job-1", "info", "Info message", null);

        var errors = await _store.GetLogsAsync("job-1", null, null, "error", null, 1, 100);
        Assert.Single(errors);
        Assert.Equal("error", errors[0].Level);

        var infos = await _store.GetLogsAsync("job-1", null, null, "info", null, 1, 100);
        Assert.Single(infos);
        Assert.Equal("info", infos[0].Level);
    }

    [Fact]
    public async Task GetLogsAsync_FiltersByTextCaseInsensitive()
    {
        await _store.AddLogAsync("job-1", "info", "Falha na conexão", null);
        await _store.AddLogAsync("job-1", "info", "Sucesso total", null);

        var results = await _store.GetLogsAsync("job-1", null, null, null, "falha", 1, 100);
        Assert.Single(results);
        Assert.Contains("Falha", results[0].Message);
    }

    [Fact]
    public async Task GetLogsAsync_FiltersByDateRange()
    {
        var past = DateTime.UtcNow.AddDays(-2);
        var future = DateTime.UtcNow.AddDays(2);

        // Add directly to bypass timestamp generation for test precision
        await _conn.ExecuteAsync(
            "INSERT INTO ScanJobLog (JobId, Timestamp, Level, Message) VALUES ('job-1', @ts, 'info', 'old')",
            new { ts = past.ToString("o") });
        await _conn.ExecuteAsync(
            "INSERT INTO ScanJobLog (JobId, Timestamp, Level, Message) VALUES ('job-1', @ts, 'info', 'new')",
            new { ts = future.ToString("o") });

        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow.AddDays(1);
        var results = await _store.GetLogsAsync("job-1", from, to, null, null, 1, 100);
        Assert.Single(results);
        Assert.Equal("new", results[0].Message);
    }

    [Fact]
    public async Task ClearLogsAsync_RemovesAllLogs()
    {
        await _store.AddLogAsync("job-1", "info", "To clear", null);
        var before = await _store.GetLogsAsync(null, null, null, null, null, 1, 100);
        Assert.NotEmpty(before);

        var cleared = await _store.ClearLogsAsync();
        Assert.True(cleared > 0);

        var after = await _store.GetLogsAsync(null, null, null, null, null, 1, 100);
        Assert.Empty(after);
    }

    [Fact]
    public async Task CleanupOldLogsAsync_RemovesOldLogsAndOrphanJobs()
    {
        // Add old log for job-1
        var oldTime = DateTime.UtcNow.AddDays(-40).ToString("o");
        await _conn.ExecuteAsync(
            "INSERT INTO ScanJobLog (JobId, Timestamp, Level, Message) VALUES ('job-1', @ts, 'info', 'old')",
            new { ts = oldTime });

        // Add old log for job-3 (no job in ScanJob table)
        var oldTime3 = DateTime.UtcNow.AddDays(-40).ToString("o");
        await _conn.ExecuteAsync(
            "INSERT INTO ScanJobLog (JobId, Timestamp, Level, Message) VALUES ('job-3', @ts, 'info', 'orphan')",
            new { ts = oldTime3 });

        // Cleanup with 30 day retention
        var cleaned = await _store.CleanupOldLogsAsync(30);
        Assert.True(cleaned > 0);

        var remaining = await _store.GetLogsAsync(null, null, null, null, null, 1, 100);
        Assert.All(remaining, log =>
        {
            Assert.True(log.Timestamp > DateTime.UtcNow.AddDays(-30));
        });
    }

    [Fact]
    public async Task GetLogsAsync_OrdersByTimestampDesc()
    {
        await _store.AddLogAsync("job-1", "info", "First", null);
        await Task.Delay(10); // ensure different timestamp
        await _store.AddLogAsync("job-1", "info", "Second", null);

        var logs = await _store.GetLogsAsync("job-1", null, null, null, null, 1, 100);
        Assert.Equal(2, logs.Count);
        Assert.Equal("Second", logs[0].Message);
        Assert.Equal("First", logs[1].Message);
    }
}