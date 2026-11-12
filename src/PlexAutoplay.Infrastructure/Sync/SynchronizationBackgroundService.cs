using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace PlexAutoplay.Infrastructure.Sync;

/// <summary>
/// Serviço em segundo plano para sincronização periódica unificada (catálogo + marcadores)
/// governada por um único intervalo (sync_interval_minutes) lido do banco a cada ciclo.
/// </summary>
public class SynchronizationBackgroundService : BackgroundService
{
    private readonly ILogger<SynchronizationBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly Meter SyncMeter = new("PlexAutoplay.Synchronization");
    private static readonly Counter<int> SyncCounter = SyncMeter.CreateCounter<int>("plex_sync_total");
    private static readonly Histogram<double> SyncDurationHistogram = SyncMeter.CreateHistogram<double>("plex_sync_duration_ms");

    public SynchronizationBackgroundService(
        ILogger<SynchronizationBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SynchronizationBackgroundService iniciado");

        // Initial delay before first sync cycle
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            int intervalMinutes = 5;
            using (var scope = _serviceProvider.CreateScope())
            {
                var settingsStore = scope.ServiceProvider.GetRequiredService<IAppSettingsStore>();
                var settings = await settingsStore.GetAsync();
                intervalMinutes = settings.SyncIntervalMinutes;
            }

            _logger.LogInformation("Aguardando {IntervalMinutes} min até próximo ciclo de sincronização", intervalMinutes);
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            var syncStart = DateTime.UtcNow;
            var jobId = Guid.NewGuid().ToString("N");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scanJobs = scope.ServiceProvider.GetRequiredService<IScanJobRepository>();

                // Cria job
                var job = new ScanJob
                {
                    JobId = jobId,
                    Status = "queued",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await scanJobs.AddAsync(job);

                var syncUseCase = scope.ServiceProvider.GetRequiredService<SynchronizationUseCase>();
                await syncUseCase.RunAsync(jobId, stoppingToken);

                SyncCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ciclo de sincronização falhou");
                SyncCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            }
            finally
            {
                var syncDurationMs = (DateTime.UtcNow - syncStart).TotalMilliseconds;
                SyncDurationHistogram.Record(syncDurationMs);
            }
        }

        _logger.LogInformation("SynchronizationBackgroundService parando");
    }
}