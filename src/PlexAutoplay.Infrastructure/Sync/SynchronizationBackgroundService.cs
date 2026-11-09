namespace PlexAutoplay.Infrastructure.Sync;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Serviço único de sincronização em segundo plano: executa, por ciclo, a etapa
/// de catálogo e em seguida a etapa de marcadores.
/// </summary>
public class SynchronizationBackgroundService : BackgroundService
{
    private readonly ILogger<SynchronizationBackgroundService> _logger;
    private readonly SynchronizationOrchestrator _orchestrator;
    private readonly IAppSettingsStore _settings;
    private readonly IScanJobStore _scanJobs;

    public SynchronizationBackgroundService(
        ILogger<SynchronizationBackgroundService> logger,
        SynchronizationOrchestrator orchestrator,
        IAppSettingsStore settings,
        IScanJobStore scanJobs)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _settings = settings;
        _scanJobs = scanJobs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var initial = await _settings.GetAsync();
        _logger.LogInformation("Serviço de sincronização iniciado com intervalo de {IntervalMinutes} minuto(s)", initial.SyncIntervalMinutes);

        // Espera inicial antes do primeiro ciclo
        await Task.Delay(TimeSpan.FromMinutes(initial.SyncIntervalMinutes), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var jobId = Guid.NewGuid().ToString();
            _scanJobs.Add(new ScanJob { JobId = jobId, Status = "running" });

            try
            {
                await _orchestrator.RunAsync(jobId, stoppingToken);
                var job = _scanJobs.Get(jobId);
                if (job != null)
                {
                    job.Status = "completed";
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException)
            {
                var job = _scanJobs.Get(jobId);
                if (job != null)
                {
                    job.Status = "cancelled";
                    job.UpdatedAt = DateTime.UtcNow;
                }
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no ciclo de sincronização agendado");
                var job = _scanJobs.Get(jobId);
                if (job != null)
                {
                    job.Status = "failed";
                    job.ErrorMessage = ex.Message;
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Releitura para aplicar mudanças da UI no próximo ciclo
            var settings = await _settings.GetAsync();
            await Task.Delay(TimeSpan.FromMinutes(settings.SyncIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Serviço de sincronização parando");
    }
}