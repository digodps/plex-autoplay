using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Application.UseCases;

/// <summary>
/// Etapa 1: sincronização do catálogo (bibliotecas e séries do Plex → banco).
/// </summary>
public sealed class CatalogSyncUseCase
{
    private readonly IPlexApiClient _plexClient;
    private readonly ILibraryRepository _libraryRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly ILogger<CatalogSyncUseCase> _logger;

    public CatalogSyncUseCase(
        IPlexApiClient plexClient,
        ILibraryRepository libraryRepository,
        ISeriesRepository seriesRepository,
        ILogger<CatalogSyncUseCase> logger)
    {
        _plexClient = plexClient;
        _libraryRepository = libraryRepository;
        _seriesRepository = seriesRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executa upsert de bibliotecas e séries; hard delete do que sumiu no Plex.
    /// </summary>
    /// <returns>Contagens: inserted/updated/removed bibliotecas e séries.</returns>
    public async Task<CatalogSyncResult> RunAsync(CancellationToken ct)
    {
        var librariesResult = await _plexClient.GetLibrariesAsync();
        if (librariesResult.Failed)
        {
            _logger.LogWarning("Falha ao buscar bibliotecas do Plex: {ErrorMessage}", librariesResult.ErrorMessage);
            return CatalogSyncResult.Fail(librariesResult.ErrorMessage ?? "Falha ao buscar bibliotecas do Plex");
        }

        var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();
        _logger.LogInformation("Encontradas {Count} bibliotecas de séries no Plex", seriesLibraries.Count);

        // Bibliotecas
        var plexLibraryKeys = new HashSet<string>(seriesLibraries.Select(l => l.Key));
        var localLibraries = await _libraryRepository.GetAllAsync();
        var localLibraryKeys = new HashSet<string>(localLibraries.Select(l => l.RatingKey));

        int librariesInserted = 0;
        int librariesUpdated = 0;
        int librariesRemoved = 0;

        foreach (var plexLib in seriesLibraries)
        {
            ct.ThrowIfCancellationRequested();
            var existing = await _libraryRepository.GetByRatingKeyAsync(plexLib.Key);
            if (existing == null)
            {
                await _libraryRepository.UpsertAsync(new Library
                {
                    RatingKey = plexLib.Key,
                    Title = plexLib.Title,
                    Type = plexLib.Type,
                    Agent = plexLib.Agent
                });
                librariesInserted++;
            }
            else
            {
                await _libraryRepository.UpsertAsync(new Library
                {
                    RatingKey = plexLib.Key,
                    Title = plexLib.Title,
                    Type = plexLib.Type,
                    Agent = plexLib.Agent
                });
                librariesUpdated++;
            }
        }

        var orphanLibraries = localLibraryKeys.Except(plexLibraryKeys).ToList();
        foreach (var orphanKey in orphanLibraries)
        {
            ct.ThrowIfCancellationRequested();
            await _libraryRepository.DeleteAsync(orphanKey);
            librariesRemoved++;
        }

        // Séries
        int seriesInserted = 0;
        int seriesUpdated = 0;
        int seriesRemoved = 0;

        foreach (var lib in seriesLibraries)
        {
            ct.ThrowIfCancellationRequested();

            var seriesResult = await _plexClient.GetSeriesAsync(lib.Key, 0, int.MaxValue);
            if (seriesResult.Failed)
            {
                _logger.LogWarning("Falha ao buscar séries para a biblioteca {LibKey}", lib.Key);
                continue;
            }

            var plexSeriesKeys = new HashSet<string>(seriesResult.Items.Select(s => s.RatingKey));
            var (localSeries, _) = await _seriesRepository.GetByLibraryAsync(lib.Key, 0, int.MaxValue, null);
            var localSeriesKeys = new HashSet<string>(localSeries.Select(s => s.RatingKey));

            foreach (var plexSeries in seriesResult.Items)
            {
                ct.ThrowIfCancellationRequested();
                var existing = await _seriesRepository.GetByRatingKeyAsync(plexSeries.RatingKey);
                if (existing == null)
                {
                    await _seriesRepository.UpsertAsync(new Series
                    {
                        RatingKey = plexSeries.RatingKey,
                        LibraryRatingKey = lib.Key,
                        Title = plexSeries.Title,
                        Summary = plexSeries.Summary,
                        Thumb = plexSeries.Thumb
                    });
                    seriesInserted++;
                }
                else if (existing.Title != plexSeries.Title || existing.Summary != plexSeries.Summary || existing.Thumb != plexSeries.Thumb)
                {
                    await _seriesRepository.UpsertAsync(new Series
                    {
                        RatingKey = plexSeries.RatingKey,
                        LibraryRatingKey = lib.Key,
                        Title = plexSeries.Title,
                        Summary = plexSeries.Summary,
                        Thumb = plexSeries.Thumb
                    });
                    seriesUpdated++;
                }
            }

            var orphanSeries = localSeriesKeys.Except(plexSeriesKeys).ToList();
            foreach (var orphanKey in orphanSeries)
            {
                ct.ThrowIfCancellationRequested();
                await _seriesRepository.DeleteAsync(orphanKey);
                seriesRemoved++;
            }
        }

        _logger.LogInformation(
            "Sincronização de catálogo concluída: bibliotecas {Ins} inseridas, {Upd} atualizadas, {Rem} removidas; séries {SIns} inseridas, {SUpd} atualizadas, {SRem} removidas",
            librariesInserted, librariesUpdated, librariesRemoved,
            seriesInserted, seriesUpdated, seriesRemoved);

        return CatalogSyncResult.Ok(
            librariesInserted, librariesUpdated, librariesRemoved,
            seriesInserted, seriesUpdated, seriesRemoved);
    }
}

/// <summary>Resultado da etapa 1 (catálogo).</summary>
public sealed record CatalogSyncResult(
    bool Success,
    int LibrariesInserted,
    int LibrariesUpdated,
    int LibrariesRemoved,
    int SeriesInserted,
    int SeriesUpdated,
    int SeriesRemoved,
    string? ErrorMessage)
{
    public static CatalogSyncResult Ok(int li, int lu, int lr, int si, int su, int sr) =>
        new(true, li, lu, lr, si, su, sr, null);

    public static CatalogSyncResult Fail(string error) =>
        new(false, 0, 0, 0, 0, 0, 0, error);
}