using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.AutoSkip;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace PlexAutoplay.Infrastructure.Sync;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly IPlexApiClient _plexClient;
    private readonly IMarkerDetector _markerDetector;
    private readonly IMarkerRepository _markerRepository;
    private readonly ICustomJsonWriter _customJsonWriter;
    private readonly string _mediaRoot;
    private int _intervalMinutes;
    private static readonly Meter SyncMeter = new("PlexAutoplay.Sync");
    private static readonly Counter<int> ScanCounter = SyncMeter.CreateCounter<int>("plex_scans_total");
    private static readonly Histogram<double> ScanDurationHistogram = SyncMeter.CreateHistogram<double>("plex_scan_duration_ms");

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IPlexApiClient plexClient,
        IMarkerDetector markerDetector,
        IMarkerRepository markerRepository,
        ICustomJsonWriter customJsonWriter,
        IConfiguration configuration)
    {
        _logger = logger;
        _plexClient = plexClient;
        _markerDetector = markerDetector;
        _markerRepository = markerRepository;
        _customJsonWriter = customJsonWriter;
        _mediaRoot = configuration["MEDIA_ROOT"] ?? "/media";
        _intervalMinutes = configuration.GetValue("SYNC_INTERVAL_MINUTES", 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync service starting with {IntervalMinutes} minute interval", _intervalMinutes);

        // Initial delay before first scan
        await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting scheduled sync scan");
            var scanStart = DateTime.UtcNow;
            try
            {
                await RunScanAsync();
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

            // Wait for next interval
            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Sync service stopping");
    }

    private async Task RunScanAsync()
    {
        var libraries = await _plexClient.GetLibrariesAsync();
        var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();

        int processed = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var lib in seriesLibraries)
        {
            try
            {
                var episodes = await _plexClient.GetEpisodesAsync(lib.Key);

                foreach (var episode in episodes)
                {
                    // Skip if already has both intro and credits markers
                    var hasIntro = await _markerRepository.ExistsAsync(episode.RatingKey, "intro");
                    var hasCredits = await _markerRepository.ExistsAsync(episode.RatingKey, "credits");
                    if (hasIntro && hasCredits)
                    {
                        skipped++;
                        continue;
                    }

                    // Resolve local media path
                    var localPath = MediaPathResolver.Resolve(_mediaRoot, episode.FilePath);
                    if (string.IsNullOrEmpty(localPath) || !System.IO.File.Exists(localPath))
                    {
                        skipped++;
                        continue;
                    }

                    // Detect markers via ffprobe
                    var detectedMarkers = _markerDetector.Detect(localPath);

                    var markerEntries = new List<ICustomJsonWriter.MarkerEntry>();
                    foreach (var marker in detectedMarkers)
                    {
                        var exists = await _markerRepository.ExistsAsync(episode.RatingKey, marker.Kind);
                        if (!exists)
                        {
                            await _markerRepository.AddAsync(new PlexAutoplay.Domain.Entities.Marker
                            {
                                RatingKey = episode.RatingKey,
                                Kind = marker.Kind,
                                StartMs = marker.StartMs,
                                EndMs = marker.EndMs
                            });
                            markerEntries.Add(new ICustomJsonWriter.MarkerEntry(marker.Kind, marker.StartMs, marker.EndMs));
                        }
                    }

                    if (markerEntries.Count > 0)
                    {
                        _customJsonWriter.UpsertMarkers(episode.RatingKey, markerEntries);
                    }

                    processed++;
                }
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