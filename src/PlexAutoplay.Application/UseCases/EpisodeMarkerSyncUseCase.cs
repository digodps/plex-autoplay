using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Application.UseCases;

/// <summary>
/// Etapa 2: varredura de marcadores dos episódios das bibliotecas de séries.
/// </summary>
public sealed class EpisodeMarkerSyncUseCase
{
    private readonly IPlexApiClient _plexClient;
    private readonly LibraryScanUseCase _libraryScan;
    private readonly IAppSettingsStore _settings;
    private readonly ILogger<EpisodeMarkerSyncUseCase> _logger;

    public EpisodeMarkerSyncUseCase(
        IPlexApiClient plexClient,
        LibraryScanUseCase libraryScan,
        IAppSettingsStore settings,
        ILogger<EpisodeMarkerSyncUseCase> logger)
    {
        _plexClient = plexClient;
        _libraryScan = libraryScan;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Para cada biblioteca de séries, busca episódios e roda detecção de marcadores.
    /// </summary>
    /// <returns>Contagens: processed, skipped, errors.</returns>
    public async Task<MarkerSyncResult> RunAsync(CancellationToken ct)
    {
        var librariesResult = await _plexClient.GetLibrariesAsync();
        if (librariesResult.Failed)
        {
            _logger.LogWarning("Falha ao buscar bibliotecas do Plex (marcadores): {ErrorMessage}", librariesResult.ErrorMessage);
            return MarkerSyncResult.Fail(librariesResult.ErrorMessage ?? "Falha ao buscar bibliotecas do Plex");
        }

        var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();
        var settings = await _settings.GetAsync();

        int processed = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var lib in seriesLibraries)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var episodesResult = await _plexClient.GetEpisodesAsync(lib.Key);
                if (episodesResult.Failed)
                {
                    errors++;
                    _logger.LogWarning("Falha ao buscar episódios da biblioteca {LibKey}: {ErrorMessage}", lib.Key, episodesResult.ErrorMessage);
                    continue;
                }

                var seeds = episodesResult.Episodes.Select(e => new EpisodeSeed(e.RatingKey, e.FilePath));
                var summary = await _libraryScan.ScanLibraryAsync(seeds, settings.MediaRoots, origin: "detected");

                processed += summary.Processed;
                skipped += summary.Skipped;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao escanear biblioteca {LibKey}", lib.Key);
                errors++;
            }
        }

        _logger.LogInformation("Varredura de marcadores concluída: {Processed} processados, {Skipped} pulados, {Errors} erros", processed, skipped, errors);

        return MarkerSyncResult.Ok(processed, skipped, errors);
    }
}

/// <summary>Resultado da etapa 2 (marcadores).</summary>
public sealed record MarkerSyncResult(
    bool Success,
    int Processed,
    int Skipped,
    int Errors,
    string? ErrorMessage)
{
    public static MarkerSyncResult Ok(int processed, int skipped, int errors) =>
        new(true, processed, skipped, errors, null);

    public static MarkerSyncResult Fail(string error) =>
        new(false, 0, 0, 0, error);
}