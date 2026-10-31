namespace PlexAutoplay.Infrastructure.Sync;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Serviço em segundo plano para sincronização incremental do catálogo do Plex (bibliotecas e séries).</summary>
public class CatalogSyncBackgroundService : BackgroundService
{
    private readonly ILogger<CatalogSyncBackgroundService> _logger;
    private readonly IPlexApiClient _plexClient;
    private readonly ILibraryRepository _libraryRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly int _intervalMinutes;
    private static readonly Meter SyncMeter = new("PlexAutoplay.CatalogSync");
    private static readonly Counter<int> SyncCounter = SyncMeter.CreateCounter<int>("plex_catalog_sync_total");
    private static readonly Histogram<double> SyncDurationHistogram = SyncMeter.CreateHistogram<double>("plex_catalog_sync_duration_ms");

    public CatalogSyncBackgroundService(
        ILogger<CatalogSyncBackgroundService> logger,
        IPlexApiClient plexClient,
        ILibraryRepository libraryRepository,
        ISeriesRepository seriesRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _plexClient = plexClient;
        _libraryRepository = libraryRepository;
        _seriesRepository = seriesRepository;
        _intervalMinutes = configuration.GetValue("SYNC_CATALOG_INTERVAL_MINUTES", 30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Catalog sync service starting with {IntervalMinutes} minute interval", _intervalMinutes);

        // Initial delay before first sync
        await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting scheduled catalog sync");
            var syncStart = DateTime.UtcNow;
            try
            {
                await RunSyncAsync(stoppingToken);
                SyncCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Catalog sync failed");
                SyncCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            }
            finally
            {
                var syncDurationMs = (DateTime.UtcNow - syncStart).TotalMilliseconds;
                SyncDurationHistogram.Record(syncDurationMs);
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Catalog sync service stopping");
    }

    private async Task RunSyncAsync(CancellationToken stoppingToken)
    {
        var librariesResult = await _plexClient.GetLibrariesAsync();
        if (librariesResult.Failed)
        {
            _logger.LogWarning("Failed to fetch libraries from Plex: {ErrorMessage}", librariesResult.ErrorMessage);
            return;
        }

        var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();
        _logger.LogInformation("Found {Count} series libraries in Plex", seriesLibraries.Count);

        // Step 1: Sync libraries - upsert and remove orphans
        var plexLibraryKeys = new HashSet<string>(seriesLibraries.Select(l => l.Key));
        var localLibraries = await _libraryRepository.GetAllAsync();
        var localLibraryKeys = new HashSet<string>(localLibraries.Select(l => l.RatingKey));

        int librariesInserted = 0;
        int librariesUpdated = 0;
        int librariesRemoved = 0;

        // Upsert libraries from Plex
        foreach (var plexLib in seriesLibraries)
        {
            stoppingToken.ThrowIfCancellationRequested();
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

        // Remove libraries not in Plex anymore
        var orphanLibraries = localLibraryKeys.Except(plexLibraryKeys).ToList();
        foreach (var orphanKey in orphanLibraries)
        {
            stoppingToken.ThrowIfCancellationRequested();
            await _libraryRepository.DeleteAsync(orphanKey);
            librariesRemoved++;
        }

        // Step 2: Sync series for each library
        int seriesInserted = 0;
        int seriesUpdated = 0;
        int seriesRemoved = 0;

        foreach (var lib in seriesLibraries)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var seriesResult = await _plexClient.GetSeriesAsync(lib.Key, 0, int.MaxValue);
            if (seriesResult.Failed)
            {
                _logger.LogWarning("Failed to fetch series for library {LibKey}: {ErrorMessage}", lib.Key, seriesResult.ErrorMessage);
                continue;
            }

            var plexSeriesKeys = new HashSet<string>(seriesResult.Items.Select(s => s.RatingKey));
            var (localSeries, _) = await _seriesRepository.GetByLibraryAsync(lib.Key, 0, int.MaxValue, null);
            var localSeriesKeys = new HashSet<string>(localSeries.Select(s => s.RatingKey));

            // Upsert series from Plex
            foreach (var plexSeries in seriesResult.Items)
            {
                stoppingToken.ThrowIfCancellationRequested();
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

            // Remove series not in Plex anymore for this library
            var orphanSeries = localSeriesKeys.Except(plexSeriesKeys).ToList();
            foreach (var orphanKey in orphanSeries)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await _seriesRepository.DeleteAsync(orphanKey);
                seriesRemoved++;
            }
        }

        _logger.LogInformation(
            "Catalog sync completed: libraries {Inserted} inserted, {Updated} updated, {Removed} removed; series {SeriesInserted} inserted, {SeriesUpdated} updated, {SeriesRemoved} removed",
            librariesInserted, librariesUpdated, librariesRemoved,
            seriesInserted, seriesUpdated, seriesRemoved);
    }
}