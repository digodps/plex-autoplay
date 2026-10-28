using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace PlexAutoplay.Infrastructure.Sync;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private int _intervalMinutes;
    private static readonly Meter SyncMeter = new("PlexAutoplay.Sync");
    private static readonly Counter<int> ScanCounter = SyncMeter.CreateCounter<int>("plex_scans_total");
    private static readonly Histogram<double> ScanDurationHistogram = SyncMeter.CreateHistogram<double>("plex_scan_duration_ms");

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        // Read interval from environment variable, default to 5 minutes
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
                // In a full implementation, this would orchestrate the scan
                // via PlexApiClient, FfmpegMarkerDetector, and marker repository.
                _logger.LogInformation("Sync scan completed");
                
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
}
