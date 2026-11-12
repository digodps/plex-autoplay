using Microsoft.Extensions.Logging;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Application.UseCases;

/// <summary>
/// Orquestra o processo único de sincronização em duas etapas ordenadas por ciclo:
/// 1. Etapa 1 — catálogo (bibliotecas e séries do Plex → banco).
/// 2. Etapa 2 — marcadores (episódios das bibliotecas de séries → detecção → banco).
///
/// O mesmo processo é usado pelo agendamento periódico e pelo comando de
/// sincronização forçada (POST /api/sync/run).
/// </summary>
public sealed class SynchronizationUseCase
{
    private readonly CatalogSyncUseCase _catalogSync;
    private readonly EpisodeMarkerSyncUseCase _markerSync;
    private readonly IScanJobRepository _scanJobs;
    private readonly IScanJobLogStore _scanJobLogs;
    private readonly ILogger<SynchronizationUseCase> _logger;

    public SynchronizationUseCase(
        CatalogSyncUseCase catalogSync,
        EpisodeMarkerSyncUseCase markerSync,
        IScanJobRepository scanJobs,
        IScanJobLogStore scanJobLogs,
        ILogger<SynchronizationUseCase> logger)
    {
        _catalogSync = catalogSync;
        _markerSync = markerSync;
        _scanJobs = scanJobs;
        _scanJobLogs = scanJobLogs;
        _logger = logger;
    }

    /// <summary>
    /// Executa as duas etapas ordenadas para o <paramref name="jobId"/> informado.
    /// Falha na etapa 1 cancela a etapa 2 e deixa o job em failed.
    /// </summary>
    public async Task RunAsync(string jobId, CancellationToken ct)
    {
        var job = await _scanJobs.GetAsync(jobId);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job {jobId} não encontrado.");
        }

        async Task LogAsync(string level, string message, string? suggestion = null)
        {
            await _scanJobLogs.AddLogAsync(jobId, level, message, suggestion);
            _logger.LogInformation("{Level} [{JobId}]: {Message}", level, jobId, message);
        }

        try
        {
            // Etapa 1 — catálogo
            job.Status = "running";
            await LogAsync("info", "Etapa 1 (catálogo): iniciada");
            await _scanJobs.UpdateAsync(job);

            var catalogResult = await _catalogSync.RunAsync(ct);
            if (!catalogResult.Success)
            {
                job.Status = "failed";
                job.ErrorMessage = catalogResult.ErrorMessage ?? "Falha na etapa 1 (catálogo)";
                job.UpdatedAt = DateTime.UtcNow;
                await _scanJobs.UpdateAsync(job);
                await LogAsync("error", $"Etapa 1 (catálogo) falhou: {job.ErrorMessage}");
                await LogAsync("info", "Etapa 2 (marcadores) não iniciada devido à falha na etapa 1");
                return;
            }

            job.Processed = catalogResult.LibrariesInserted + catalogResult.LibrariesUpdated;
            job.Skipped = catalogResult.LibrariesRemoved;
            await LogAsync("info", $"Etapa 1 (catálogo) concluída: bibliotecas inseridas={catalogResult.LibrariesInserted} atualizadas={catalogResult.LibrariesUpdated} removidas={catalogResult.LibrariesRemoved}");
            await _scanJobs.UpdateAsync(job);

            // Etapa 2 — marcadores
            await LogAsync("info", "Etapa 2 (marcadores): iniciada");
            job.Status = "running";
            await _scanJobs.UpdateAsync(job);

            var markerResult = await _markerSync.RunAsync(ct);
            job.Processed += markerResult.Processed;
            job.Skipped += markerResult.Skipped;
            job.Errors += markerResult.Errors;

            job.Status = markerResult.Success ? "completed" : "failed";
            job.ErrorMessage = markerResult.ErrorMessage;
            job.UpdatedAt = DateTime.UtcNow;
            await _scanJobs.UpdateAsync(job);

            await LogAsync("info", $"Etapa 2 (marcadores) concluída: processados={markerResult.Processed} pulados={markerResult.Skipped} erros={markerResult.Errors}. Job final={job.Status}");
        }
        catch (OperationCanceledException)
        {
            job.Status = "cancelled";
            job.UpdatedAt = DateTime.UtcNow;
            await _scanJobs.UpdateAsync(job);
            await LogAsync("warn", "Job cancelado");
            throw;
        }
        catch (Exception ex)
        {
            job.Status = "failed";
            job.ErrorMessage = ex.Message;
            job.UpdatedAt = DateTime.UtcNow;
            await _scanJobs.UpdateAsync(job);
            await LogAsync("error", $"Exceção não tratada: {ex.Message}");
            throw;
        }
    }
}

/// <summary>Resultado agregado do ciclo de sincronização.</summary>
public sealed record SyncResult(
    bool Success,
    int Stage1LibrariesInserted,
    int Stage1LibrariesUpdated,
    int Stage1LibrariesRemoved,
    int Stage1SeriesInserted,
    int Stage1SeriesUpdated,
    int Stage1SeriesRemoved,
    int Stage2Processed,
    int Stage2Skipped,
    int Stage2Errors,
    string? ErrorMessage)
{
    public static SyncResult Ok(
        int s1Li, int s1Lu, int s1Lr,
        int s1Si, int s1Su, int s1Sr,
        int s2P, int s2Sk, int s2E) =>
        new(true, s1Li, s1Lu, s1Lr, s1Si, s1Su, s1Sr, s2P, s2Sk, s2E, null);

    public static SyncResult Fail(string error) =>
        new(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, error);
}
