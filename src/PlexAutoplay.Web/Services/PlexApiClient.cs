using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using PlexAutoplay.Application;
using Serilog;

namespace PlexAutoplay.Web.Services;

/// <summary>
/// Cliente HTTP para a API do Plex, com telemetria (traces, métricas e logs).
/// </summary>
public class PlexApiClient : IPlexApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly ActivitySource ActivitySource = PlexAutoplay.Infrastructure.Telemetry.OpenTelemetryService.Tracer;
    private static readonly Meter PlexMeter = new("PlexAutoplay.PlexApi");
    private static readonly Counter<int> PlexErrorCounter = PlexMeter.CreateCounter<int>("plex_api_errors_total");

    public PlexApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlexLibraryDto>> GetLibrariesAsync()
    {
        using var activity = ActivitySource.StartActivity("Plex.GetLibraries");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", "/library/sections");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync("/library/sections");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetLibraries", (int)response.StatusCode);
                return new List<PlexLibraryDto>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var libraries = new List<PlexLibraryDto>();
            var directory = doc.RootElement.GetProperty("Directory");

            foreach (var item in directory.EnumerateArray())
            {
                libraries.Add(new PlexLibraryDto(
                    item.GetProperty("key").GetString() ?? "",
                    item.GetProperty("title").GetString() ?? "",
                    item.GetProperty("type").GetString() ?? "",
                    item.GetProperty("agent").GetString() ?? ""
                ));
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetLibraries", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, count = libraries.Count });

            return libraries;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetLibraries", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return new List<PlexLibraryDto>();
        }
    }

    public async Task<List<PlexEpisodeDto>> GetEpisodesAsync(string sectionKey)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetEpisodes");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/sections/{sectionKey}/items");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync($"/library/sections/{sectionKey}/items");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetEpisodes", (int)response.StatusCode);
                return new List<PlexEpisodeDto>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var episodes = new List<PlexEpisodeDto>();
            var metadata = doc.RootElement.GetProperty("MediaContainer").GetProperty("Metadata");

            foreach (var item in metadata.EnumerateArray())
            {
                // Only process episodes (type "episode")
                if (item.TryGetProperty("type", out var typeEl) && typeEl.GetString() != "episode")
                    continue;

                string? filePath = null;
                if (item.TryGetProperty("Media", out var mediaEl))
                {
                    foreach (var media in mediaEl.EnumerateArray())
                    {
                        if (media.TryGetProperty("Part", out var partsEl))
                        {
                            foreach (var part in partsEl.EnumerateArray())
                            {
                                filePath = part.GetProperty("file").GetString();
                                break;
                            }
                        }
                        if (filePath != null) break;
                    }
                }

                episodes.Add(new PlexEpisodeDto(
                    item.GetProperty("ratingKey").GetString() ?? "",
                    item.GetProperty("title").GetString() ?? "",
                    item.GetProperty("parentIndex").GetInt32(),
                    item.GetProperty("index").GetInt32(),
                    filePath ?? ""
                ));
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, section_key = sectionKey, count = episodes.Count });

            return episodes;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, section_key = sectionKey });
            return new List<PlexEpisodeDto>();
        }
    }
}
