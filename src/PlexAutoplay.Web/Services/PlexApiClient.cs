using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using PlexAutoplay.Application;
using Serilog;

namespace PlexAutoplay.Web.Services;

/// <summary>
/// Cliente HTTP para a API do Plex, com telemetria (traces, métricas e logs).
/// Dividido em partials por domínio: núcleo (bibliotecas/episódios) em
/// <see cref="PlexApiClient"/> e séries em <c>PlexApiClient.Series</c>.
/// </summary>
public partial class PlexApiClient : IPlexApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly ActivitySource ActivitySource = PlexAutoplay.Infrastructure.Telemetry.OpenTelemetryService.Tracer;
    private static readonly Meter PlexMeter = new("PlexAutoplay.PlexApi");
    private static readonly Counter<int> PlexErrorCounter = PlexMeter.CreateCounter<int>("plex_api_errors_total");

    public PlexApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Extrai o ID numérico da seção de uma chave do Plex.
    /// O Plex pode devolver o key como "/library/sections/1" (caminho completo)
    /// ou apenas "1" (ID puro). Retorna apenas o segmento numérico final.
    /// </summary>
    private static string ExtractSectionId(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return rawKey;

        // Remove barras finais para evitar segmentos vazios
        var trimmed = rawKey.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }

    public async Task<PlexLibrariesResult> GetLibrariesAsync()
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
                var statusCode = (int)response.StatusCode;
                var errorMessage = $"Plex API retornou status {statusCode} ao buscar bibliotecas. Verifique a URL do servidor, o token e a conectividade.";
                Log.Warning("Plex API returned {StatusCode} for GetLibraries", statusCode);
                return PlexLibrariesResult.FailedResult(errorMessage);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var libraries = new List<PlexLibraryDto>();
            // Envelope padrão do Plex aninha Directory em MediaContainer;
            // alguns proxies/mock devolvem Directory na raiz.
            var root = doc.RootElement;
            var directory = root.TryGetProperty("Directory", out var directDir)
                ? directDir
                : root.TryGetProperty("MediaContainer", out var container) && container.TryGetProperty("Directory", out var nestedDir)
                    ? nestedDir
                    : default;

            if (directory.ValueKind == JsonValueKind.Array)
                foreach (var item in directory.EnumerateArray())
                {
                    // O Plex pode devolver o key como "/library/sections/1" (caminho completo)
                    // ou apenas "1" (ID). Extraímos apenas o segmento numérico final,
                    // pois os endpoints de catálogo/episódios esperam o ID puro da seção.
                    var rawKey = item.GetProperty("key").GetString() ?? "";
                    var sectionId = ExtractSectionId(rawKey);

                    libraries.Add(new PlexLibraryDto(
                        sectionId,
                        item.GetProperty("title").GetString() ?? "",
                        item.GetProperty("type").GetString() ?? "",
                        item.GetProperty("agent").GetString() ?? ""
                    ));
                }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetLibraries", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, count = libraries.Count });

            return new PlexLibrariesResult(libraries);
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            var errorMessage = $"Falha ao contatar a API do Plex: {ex.Message}. Verifique se o servidor Plex está acessível e se o token está configurado corretamente.";
            Log.Error(ex, "Plex API call failed: GetLibraries", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return PlexLibrariesResult.FailedResult(errorMessage);
        }
    }

    public async Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetEpisodes");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/sections/{sectionKey}/all?type=4");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync($"/library/sections/{sectionKey}/all?type=4");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                var statusCode = (int)response.StatusCode;
                var errorMessage = $"Plex API retornou status {statusCode} ao buscar episódios da seção {sectionKey}. Verifique a conexão com o servidor Plex.";
                Log.Warning("Plex API returned {StatusCode} for GetEpisodes", statusCode);
                return PlexEpisodesResult.FailedResult(errorMessage);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var episodes = new List<PlexEpisodeDto>();
            if (doc.RootElement.GetProperty("MediaContainer").TryGetProperty("Metadata", out var metadata) && metadata.ValueKind == JsonValueKind.Array)
            {
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
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, section_key = sectionKey, count = episodes.Count });

            return new PlexEpisodesResult(episodes);
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            var errorMessage = $"Falha ao contatar a API do Plex ao buscar episódios da seção {sectionKey}: {ex.Message}. Verifique se o servidor Plex está acessível e se o token está configurado corretamente.";
            Log.Error(ex, "Plex API call failed: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, section_key = sectionKey });
            return PlexEpisodesResult.FailedResult(errorMessage);
        }
    }
}
