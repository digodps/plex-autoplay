using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Infrastructure.Telemetry;

namespace PlexAutoplay.Infrastructure.PlexProxy;

/// <summary>
/// Cliente HTTP para a API do Plex, com telemetria (traces, métricas e logs).
/// Orquestra as chamadas HTTP e delega o parsing de resposta a
/// <see cref="PlexResponseParser"/>. Dividido em partials por domínio:
/// núcleo (bibliotecas/episódios) em <see cref="PlexApiClient"/> e séries em
/// <c>PlexApiClient.Series</c>.
/// </summary>
public partial class PlexApiClient : IPlexApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlexApiClient> _logger;
    private static readonly ActivitySource ActivitySource = OpenTelemetryService.Tracer;
    private static readonly Meter PlexMeter = new("PlexAutoplay.PlexApi");
    private static readonly Counter<int> PlexErrorCounter = PlexMeter.CreateCounter<int>("plex_api_errors_total");

    public PlexApiClient(HttpClient httpClient, ILogger<PlexApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
                var errorMessage = $"Plex API retornou status {{statusCode}} ao buscar bibliotecas. Verifique a URL do servidor, o token e a conectividade.";
                _logger.LogWarning("Plex API returned {{StatusCode}} for GetLibraries", statusCode);
                return PlexLibrariesResult.FailedResult(errorMessage);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var libraries = PlexResponseParser.ParseLibraries(json);

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Plex API call: GetLibraries {{@Data}}", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, count = libraries.Count });

            return new PlexLibrariesResult(libraries);
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            var errorMessage = $"Falha ao contatar a API do Plex: {{ex.Message}}. Verifique se o servidor Plex está acessível e se o token está configurado corretamente.";
            _logger.LogError(ex, "Plex API call failed: GetLibraries {{@Data}}", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return PlexLibrariesResult.FailedResult(errorMessage);
        }
    }

    public async Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetEpisodes");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/sections/{{sectionKey}}/all?type=4");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync($"/library/sections/{{sectionKey}}/all?type=4");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                var statusCode = (int)response.StatusCode;
                var errorMessage = $"Plex API retornou status {{statusCode}} ao buscar episódios da seção {{sectionKey}}. Verifique a conexão com o servidor Plex.";
                _logger.LogWarning("Plex API returned {{StatusCode}} for GetEpisodes", statusCode);
                return PlexEpisodesResult.FailedResult(errorMessage);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var episodes = PlexResponseParser.ParseEpisodes(json);

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Plex API call: GetEpisodes {{@Data}}", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, section_key = sectionKey, count = episodes.Count });

            return new PlexEpisodesResult(episodes);
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            var errorMessage = $"Falha ao contatar a API do Plex ao buscar episódios da seção {{sectionKey}}: {{ex.Message}}. Verifique se o servidor Plex está acessível e se o token está configurado corretamente.";
            _logger.LogError(ex, "Plex API call failed: GetEpisodes {{@Data}}", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, section_key = sectionKey });
            return PlexEpisodesResult.FailedResult(errorMessage);
        }
    }

    public async Task<PlexIdentityResult> GetIdentityAsync(string plexUrl, string plexToken)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetIdentity");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", "/identity");

        var startTime = DateTime.UtcNow;

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Plex-Token", plexToken);

            var url = plexUrl.TrimEnd('/') + "/identity";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                var statusCode = (int)response.StatusCode;
                var errorMessage = $"Plex API retornou status {{statusCode}} ao validar identidade. Verifique a URL do servidor e o token.";
                _logger.LogWarning("Plex API returned {{StatusCode}} for GetIdentity", statusCode);
                return PlexIdentityResult.FailedResult(errorMessage);
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var xml = await response.Content.ReadAsStringAsync();
            var identity = PlexResponseParser.ParseIdentity(xml);

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Plex API call: GetIdentity {{@Data}}", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode });

            return identity;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            var errorMessage = $"Falha ao contatar a API do Plex para validar identidade: {{ex.Message}}.";
            _logger.LogError(ex, "Plex API call failed: GetIdentity {{@Data}}", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return PlexIdentityResult.FailedResult(errorMessage);
        }
    }
}