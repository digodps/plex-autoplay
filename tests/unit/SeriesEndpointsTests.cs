namespace PlexAutoplay.UnitTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using PlexAutoplay.Application;
using PlexAutoplay.Web.Endpoints;
using Xunit;

public class SeriesEndpointsTests : IDisposable
{
    private readonly IHost _host;
    private readonly HttpClient _client;

    public SeriesEndpointsTests()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton<IPlexApiClient>(new FakePlexClient());
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapSeriesEndpoints(mediaRoot: "/media"));
                });
            })
            .Start();
        _client = _host.GetTestClient();
    }

    public void Dispose() => _host.Dispose();

    /// <summary>IPlexApiClient fake com dados de catálogo e episódios.</summary>
    private class FakePlexClient : IPlexApiClient
    {
        public bool FailNextCall { get; set; }

        public Task<PlexLibrariesResult> GetLibrariesAsync() => Task.FromResult(PlexLibrariesResult.Empty);

        public Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey) => throw new NotImplementedException();

        public Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null)
        {
            if (FailNextCall)
                return Task.FromResult(new PlexSeriesPage([], 0, Failed: true));

            var items = new List<PlexSeriesDto>
            {
                new("101", "Arrow", "Um bilionário arma contra o crime.", "library/metadata/101/thumb/169"),
                new("102", "Mercy", "", "")
            };
            return Task.FromResult(new PlexSeriesPage(items, 87));
        }

        public Task<PlexEpisodesResult> GetSeriesEpisodesAsync(string seriesKey) => Task.FromResult(new PlexEpisodesResult(new List<PlexEpisodeDto>
        {
            new("5001", "Pilot", 1, 1, "/Volumes/Series/Arrow S01E01.mkv")
        }));
    }

    [Fact]
    public async Task GetSeries_ReturnsCatalogWithProxiedThumbsAndPagination()
    {
        var response = await _client.GetAsync("/api/series/1?page=2&pageSize=24");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(87, root.GetProperty("totalSize").GetInt32());
        Assert.Equal(2, root.GetProperty("page").GetInt32());

        var items = root.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());

        var first = items[0];
        Assert.Equal("Arrow", first.GetProperty("title").GetString());
        Assert.Equal("/api/images/library/metadata/101/thumb/169", first.GetProperty("thumb").GetString());

        // Série sem thumb: campo presente mas null
        var second = items[1];
        Assert.Equal(System.Text.Json.JsonValueKind.Null, second.GetProperty("thumb").ValueKind);
    }

    [Fact]
    public async Task GetSeries_WhenPlexFails_Returns502WithFriendlyMessage()
    {
        var fake = (FakePlexClient)_host.Services.GetRequiredService<IPlexApiClient>();
        fake.FailNextCall = true;

        var response = await _client.GetAsync("/api/series/1?page=1");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task GetSeries_ResolvesLocalPathsForEpisodes()
    {
        var response = await _client.GetAsync("/api/series/1/101/episodes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var episodes = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = Assert.Single(episodes.RootElement.EnumerateArray());
        Assert.Equal("5001", item.GetProperty("rating_key").GetString());
        Assert.Equal(1, item.GetProperty("season").GetInt32());
        Assert.Equal(1, item.GetProperty("episode").GetInt32());
        Assert.Equal("/media/Volumes/Series/Arrow S01E01.mkv", item.GetProperty("local_path").GetString());
    }
}
