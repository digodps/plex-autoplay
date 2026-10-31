namespace PlexAutoplay.UnitTests;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PlexAutoplay.Domain.Ports;
using Xunit;

/// <summary>
/// Verifica origem (manual/detected) e atualização (PUT) nos endpoints de marcadores.
/// </summary>
public class MarkerEndpointsTests : IClassFixture<MarkerEndpointsFixture>
{
    private readonly MarkerEndpointsFixture _fixture;

    public MarkerEndpointsTests(MarkerEndpointsFixture fixture)
    {
        _fixture = fixture;
    }

    private static HttpContent JsonContent(object body) =>
        new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

    [Fact]
    public async Task Post_ReturnsManualOrigin()
    {
        var response = await _fixture.Client.PostAsync("/api/markers", JsonContent(new
        {
            ratingKey = "4001",
            kind = "intro",
            startMs = 1000,
            endMs = 90000
        }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("manual", json.RootElement.GetProperty("origin").GetString());
    }

    [Fact]
    public async Task Get_ListIncludesOrigin()
    {
        var response = await _fixture.Client.GetAsync("/api/markers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetArrayLength() >= 1);
        Assert.All(json.RootElement.EnumerateArray(), item =>
            Assert.True(item.TryGetProperty("origin", out _) || item.TryGetProperty("Origin", out _)));
    }

    [Fact]
    public async Task Post_Duplicate_Returns409()
    {
        var response = await _fixture.Client.PostAsync("/api/markers", JsonContent(new
        {
            ratingKey = "4001",
            kind = "intro",
            startMs = 0,
            endMs = 1
        }));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_ViaQueryString_KeepsCompatWithPlaywrightE2E()
    {
        // Contrato que o E2E Playwright usa: POST com query string, body vazio
        var response = await _fixture.Client.PostAsync(
            "/api/markers?RatingKey=4002&Kind=credits&StartMs=90000&EndMs=150000", null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("manual", json.RootElement.GetProperty("origin").GetString());
    }

    [Fact]
    public async Task Put_ExistingMarker_Returns200AndUpdatesSqlite()
    {
        // Cria marcador manual
        await _fixture.Client.PostAsync("/api/markers", JsonContent(new
        {
            ratingKey = "5001",
            kind = "intro",
            startMs = 1000,
            endMs = 90000
        }));

        var put = await _fixture.Client.PutAsync("/api/markers/5001/intro", JsonContent(new
        {
            startMs = 2000,
            endMs = 95000
        }));

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        // SQLite atualizado
        var get = await _fixture.Client.GetAsync("/api/markers/5001");
        var markers = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        var marker = markers.RootElement.EnumerateArray().Single();
        Assert.Equal(2000, marker.GetProperty("startMs").GetInt64());
        Assert.Equal(95000, marker.GetProperty("endMs").GetInt64());
        Assert.Equal("manual", marker.GetProperty("origin").GetString());
    }

    [Fact]
    public async Task Put_WhenMissing_Returns404()
    {
        var put = await _fixture.Client.PutAsync("/api/markers/9999/intro", JsonContent(new
        {
            startMs = 1,
            endMs = 2
        }));
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }
}
