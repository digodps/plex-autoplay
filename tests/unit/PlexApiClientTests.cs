namespace PlexAutoplay.UnitTests;

using System.Net;
using PlexAutoplay.Web.Services;
using Xunit;

public class PlexApiClientTests
{
    private static PlexApiClient MakeClient(string baseUrl) =>
        new(new HttpClient { BaseAddress = new Uri(baseUrl), DefaultRequestHeaders = { { "X-Plex-Token", "test-token" } } });

    public static readonly string SeriesCatalogJson = """
        {
          "MediaContainer": {
            "totalSize": 87,
            "Metadata": [
              {
                "type": "show",
                "ratingKey": "101",
                "title": "Arrow",
                "summary": "Um bilionário arma para combater o crime.",
                "thumb": "library/metadata/101/thumb/169"
              },
              {
                "type": "episode",
                "ratingKey": "999",
                "title": "Nao sou serie"
              }
            ]
          }
        }
        """;

    [Fact]
    public async Task GetSeriesAsync_ParsesSeriesAndSendsContainerHeaders()
    {
        using var server = new FakePlexServer(SeriesCatalogJson);
        var client = MakeClient(server.BaseUrl);

        var result = await client.GetSeriesAsync("1", containerStart: 24, containerSize: 24, titleQuery: "arrow");

        Assert.False(result.Failed);
        Assert.Equal(87, result.TotalSize);
        var series = Assert.Single(result.Items);
        Assert.Equal("101", series.RatingKey);
        Assert.Equal("Arrow", series.Title);
        Assert.Equal("library/metadata/101/thumb/169", series.Thumb);

        // Paginação delegada ao Plex via cabeçalhos de container + filtro por título
        // (type=show removido da URL porque o Plex 1.43.x retorna 500 com esse parâmetro;
        //  o filtro por tipo é feito do lado do cliente no parsing)
        Assert.Equal("24", server.LastHeaders["X-Plex-Container-Start"]);
        Assert.Equal("24", server.LastHeaders["X-Plex-Container-Size"]);
        Assert.Contains("title.contains=arrow", server.LastQuery);
        Assert.Contains("/library/sections/1/all", server.LastPath);
    }

    [Fact]
    public async Task GetSeriesAsync_WhenPlexFails_MarksFailed()
    {
        using var server = new FakePlexServer("""{"error":true}""", statusCode: 500);
        var client = MakeClient(server.BaseUrl);

        var result = await client.GetSeriesAsync("1", 0, 24);

        Assert.True(result.Failed);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetLibrariesAsync_ParsesNestedMediaContainerEnvelope()
    {
        using var server = new FakePlexServer("{}");

        var json = """
            {
              "MediaContainer": {
                "Directory": [
                  { "key": "1", "title": "Séries", "type": "show", "agent": "tv.plex.agents.series" },
                  { "key": "2", "title": "Filmes", "type": "movie", "agent": "com.plexapp.agents.imdb" }
                ]
              }
            }
            """;
        server.ResetResponse(json);

        var client = MakeClient(server.BaseUrl);
        var result = await client.GetLibrariesAsync();

        Assert.False(result.Failed);
        Assert.Equal(2, result.Libraries.Count);
        var show = result.Libraries.Single(l => l.Type == "show");
        Assert.Equal("1", show.Key);
        Assert.Equal("tv.plex.agents.series", show.Agent);
    }

    [Fact]
    public async Task GetLibrariesAsync_WhenPlexFails_MarksFailedWithMessage()
    {
        using var server = new FakePlexServer("""{"error":true}""", statusCode: 500);
        var client = MakeClient(server.BaseUrl);

        var result = await client.GetLibrariesAsync();

        Assert.True(result.Failed);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Libraries);
        Assert.Contains("Plex", result.ErrorMessage);
    }

    [Fact]
    public async Task GetLibrariesAsync_ExtractsSectionIdFromFullPathKey()
    {
        // O Plex real costuma devolver key como "/library/sections/1" (caminho completo)
        // e não apenas o ID "1". O backend deve extrair apenas o segmento numérico.
        using var server = new FakePlexServer("""
            {
              "MediaContainer": {
                "Directory": [
                  { "key": "/library/sections/1", "title": "Séries", "type": "show", "agent": "tv.plex.agents.series" },
                  { "key": "/library/sections/2", "title": "Filmes", "type": "movie", "agent": "com.plexapp.agents.imdb" }
                ]
              }
            }
            """);

        var client = MakeClient(server.BaseUrl);
        var result = await client.GetLibrariesAsync();

        Assert.False(result.Failed);
        Assert.Equal(2, result.Libraries.Count);
        Assert.Equal("1", result.Libraries[0].Key);
        Assert.Equal("2", result.Libraries[1].Key);
        Assert.Equal("Séries", result.Libraries[0].Title);
        Assert.Equal("Filmes", result.Libraries[1].Title);
    }

    public static readonly string SeriesEpisodesJson = """
        {
          "MediaContainer": {
            "Metadata": [
              {
                "type": "episode",
                "ratingKey": "5001",
                "title": "Pilot",
                "parentIndex": 1,
                "index": 1,
                "Media": [ { "Part": [ { "file": "/Volumes/Series/Arrow S01E01.mkv" } ] } ]
              },
              {
                "type": "clip",
                "ratingKey": "7001",
                "title": "Trailer"
              }
            ]
          }
        }
        """;

    [Fact]
    public async Task GetSeriesEpisodesAsync_ParsesEpisodesOnly()
    {
        using var server = new FakePlexServer(SeriesEpisodesJson);
        var client = MakeClient(server.BaseUrl);

        var result = await client.GetSeriesEpisodesAsync("101");

        Assert.False(result.Failed);
        var item = Assert.Single(result.Episodes);
        Assert.Equal("5001", item.RatingKey);
        Assert.Equal(1, item.ParentIndex);
        Assert.Equal(1, item.Index);
        Assert.Equal("/Volumes/Series/Arrow S01E01.mkv", item.FilePath);
    }

    [Fact]
    public async Task GetSeriesEpisodesAsync_WhenPlexFails_MarksFailedWithMessage()
    {
        using var server = new FakePlexServer("""{"nope":true}""", statusCode: 502);
        var client = MakeClient(server.BaseUrl);

        var result = await client.GetSeriesEpisodesAsync("101");

        Assert.True(result.Failed);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Episodes);
        Assert.Contains("Plex", result.ErrorMessage);
    }
}
