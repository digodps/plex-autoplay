using System.Diagnostics;
using System.Text.Json;
using PlexAutoplay.Application;
using Serilog;

namespace PlexAutoplay.Web.Services;

/// <summary>
/// Partial do <see cref="PlexApiClient"/> com endpoints de séries
/// (catálogo paginado e episódios de uma série).
/// </summary>
public partial class PlexApiClient
{
    public async Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetSeries");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/sections/{sectionKey}/all");

        var startTime = DateTime.UtcNow;

        try
        {
            var url = $"/library/sections/{sectionKey}/all?type=show";
            if (!string.IsNullOrWhiteSpace(titleQuery))
            {
                url += $"&title.contains={Uri.EscapeDataString(titleQuery)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            // Paginação delegada ao Plex via cabeçalhos de container
            request.Headers.Add("X-Plex-Container-Start", containerStart.ToString());
            request.Headers.Add("X-Plex-Container-Size", containerSize.ToString());

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetSeries", (int)response.StatusCode);
                return new PlexSeriesPage([], 0, Failed: true);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var series = new List<PlexSeriesDto>();
            var container = doc.RootElement.GetProperty("MediaContainer");
            var totalSize = container.TryGetProperty("totalSize", out var totalEl) ? totalEl.GetInt32()
                            : container.TryGetProperty("TotalSize", out var totalEl2) ? totalEl2.GetInt32()
                            : 0;

            if (container.TryGetProperty("Metadata", out var metadata))
            {
                foreach (var item in metadata.EnumerateArray())
                {
                    // Filtro defensivo: apenas séries (por segurança contra versões que ignoram type=show)
                    if (item.TryGetProperty("type", out var typeEl) && typeEl.GetString() != "show")
                        continue;

                    series.Add(new PlexSeriesDto(
                        item.GetProperty("ratingKey").GetString() ?? "",
                        item.GetProperty("title").GetString() ?? "",
                        item.TryGetProperty("summary", out var summaryEl) ? summaryEl.GetString() ?? "" : "",
                        item.TryGetProperty("thumb", out var thumbEl) ? thumbEl.GetString() ?? "" : ""
                    ));
                }
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetSeries", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, section_key = sectionKey, count = series.Count, total_size = totalSize });

            return new PlexSeriesPage(series, totalSize);
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetSeries", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, section_key = sectionKey });
            return new PlexSeriesPage([], 0, Failed: true);
        }
    }

    public async Task<List<PlexEpisodeDto>> GetSeriesEpisodesAsync(string seriesKey)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetSeriesEpisodes");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/metadata/{seriesKey}/allLeaves");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync($"/library/metadata/{seriesKey}/allLeaves");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetSeriesEpisodes", (int)response.StatusCode);
                return new List<PlexEpisodeDto>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var episodes = new List<PlexEpisodeDto>();
            var metadata = doc.RootElement.GetProperty("MediaContainer").GetProperty("Metadata");

            foreach (var item in metadata.EnumerateArray())
            {
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
            Log.Information("Plex API call: GetSeriesEpisodes", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, series_key = seriesKey, count = episodes.Count });

            return episodes;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetSeriesEpisodes", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, series_key = seriesKey });
            return new List<PlexEpisodeDto>();
        }
    }
}
