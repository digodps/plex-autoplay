using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace PlexAutoplay.Infrastructure.Sync;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly IPlexApiClient _plexClient;
    private readonly LibraryScanUseCase _libraryScan;
    private readonly IAppSettingsStore _settings;
    private static readonly Meter SyncMeter = new("PlexAutoplay.Sync");
    private static readonly Counter<int> ScanCounter = SyncMeter.CreateCounter<int>("plex_scans_total");
    private static readonly Histogram<double> ScanDurationHistogram = SyncMeter.CreateHistogram<double>("plex_scan_duration_ms");

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IPlexApiClient plexClient,
        LibraryScanUseCase libraryScan,
        IAppSettingsStore settings)
    {
        _logger = logger;
        _plexClient = plexClient;
        _libraryScan = libraryScan;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var initial = await _settings.GetAsync();
        _logger.LogInformation("Sync service starting with {IntervalMinutes} minute interval", initial.SyncIntervalMinutes);

        // Initial delay before first scan
        await Task.Delay(TimeSpan.FromMinutes(initial.SyncIntervalMinutes), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = await _settings.GetAsync();
            _logger.LogInformation("Starting scheduled sync scan");
            var scanStart = DateTime.UtcNow;
            try
            {
                await RunScanAsync(settings);
                ScanCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync scan failed");
                ScanCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            }
            finally
            {
                var scanDurationMs = (DateTime.UtcNow - scanStart).TotalMilliseconds;
                ScanDurationHistogram.Record(scanDurationMs);
            }

            // Wait for next interval, reread so UI changes apply next cycle
            settings = await _settings.GetAsync();
            await Task.Delay(TimeSpan.FromMinutes(settings.SyncIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Sync service stopping");
    }

    private async Task RunScanAsync(AppSettingsSnapshot settings)
    {
        var librariesResult = await _plexClient.GetLibrariesAsync();
        if (librariesResult.Failed)
        {
            _logger.LogWarning("Falha ao buscar bibliotecas do Plex: {ErrorMessage}", librariesResult.ErrorMessage);
            return;
        }
        var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();

        int processed = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var lib in seriesLibraries)
        {
            try
            {
                var episodesResult = await _plexClient.GetEpisodesAsync(lib.Key);
                if (episodesResult.Failed)
                {
                    _logger.LogWarning("Falha ao buscar episódios da biblioteca {LibKey}: {ErrorMessage}", lib.Key, episodesResult.ErrorMessage);
                    errors++;
                    continue;
                }

                var seeds = episodesResult.Episodes.Select(e => new EpisodeSeed(e.RatingKey, e.FilePath));
                var summary = await _libraryScan.ScanLibraryAsync(seeds, settings.MediaRoots);

                processed += summary.Processed;
                skipped += summary.Skipped;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scanning library {LibKey}", lib.Key);
                errors++;
            }
        }

        _logger.LogInformation("Sync scan completed: {Processed} processed, {Skipped} skipped, {Errors} errors", processed, skipped, errors);
    }
}
