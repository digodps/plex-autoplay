namespace PlexAutoplay.UnitTests;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using PlexAutoplay.Domain.Ports;
using Xunit;

/// <summary>
/// Verifica que `GET /api/status` retorna HTTP 200 com dados do serviço,
/// último job de sincronização e métricas de processo, sem expor 500.
/// </summary>
public class StatusEndpointsTests : IClassFixture<StatusEndpointsFixture>
{
    private readonly StatusEndpointsFixture _fixture;

    public StatusEndpointsTests(StatusEndpointsFixture fixture)
    {
        _fixture = fixture;
    }

    private static JsonDocument ParseBody(HttpResponseMessage response)
    {
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonDocument.Parse(json);
    }

    [Fact]
    public async Task Get_Returns200_WhenNoJobs()
    {
        var response = await _fixture.Client.GetAsync("/api/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = ParseBody(response);
        var root = json.RootElement;

        Assert.Equal("plex-autoplay", root.GetProperty("service").GetString()!);
        Assert.True(root.TryGetProperty("version", out var version) && version.GetString().Length > 0);
        Assert.True(root.GetProperty("lastJob").ValueKind == JsonValueKind.Null);
        AssertProcessMetrics(root.GetProperty("process"));
    }

    [Fact]
    public async Task Get_ReturnsLastJob_WhenJobExists()
    {
        _fixture.ScanJobs.Add(new ScanJob
        {
            JobId = "job-1",
            Status = "completed",
            Processed = 10,
            Skipped = 2,
            Errors = 0,
            ErrorMessage = null,
            UpdatedAt = DateTime.UtcNow
        });

        var response = await _fixture.Client.GetAsync("/api/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = ParseBody(response);
        var root = json.RootElement;

        Assert.False(root.GetProperty("lastJob").ValueKind == JsonValueKind.Null);
        var lastJob = root.GetProperty("lastJob");
        Assert.Equal("job-1", lastJob.GetProperty("job_id").GetString());
        Assert.Equal("completed", lastJob.GetProperty("status").GetString());
        Assert.Equal(10, lastJob.GetProperty("processed").GetInt32());
        Assert.Equal(2, lastJob.GetProperty("skipped").GetInt32());
        Assert.Equal(0, lastJob.GetProperty("errors").GetInt32());
        Assert.True(lastJob.TryGetProperty("updated_at", out _));
        AssertProcessMetrics(root.GetProperty("process"));
    }

    private static void AssertProcessMetrics(JsonElement process)
    {
        Assert.True(process.TryGetProperty("cpu_total_ms", out var cpu) && cpu.GetDouble() >= 0);
        Assert.True(process.TryGetProperty("memory_private_bytes", out var mem) && mem.GetInt64() >= 0);
        Assert.True(process.TryGetProperty("memory_working_set_bytes", out var ws) && ws.GetInt64() >= 0);
        Assert.True(process.TryGetProperty("uptime_ms", out var up) && up.GetDouble() >= 0);
    }
}
