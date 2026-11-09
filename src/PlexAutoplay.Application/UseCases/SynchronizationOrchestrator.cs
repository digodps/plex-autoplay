using Microsoft.Extensions.Logging;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
using System.Diagnostics.Metrics;

namespace PlexAutoplay.Application.UseCases;

/// <summary>
/// Orquestra as duas etapas da sincronização unificada (catálogo → marcadores).
/// </summary>
public sealed class SynchronizationOrchestrator
{
    private readonly CatalogSyncUseCase _catalogSync;
    private readonly EpisodeMarkerSyncUseCase _markerSync;
    private readonly IScanJobStore _scanJobs;
    private readonly ILogger<SynchronizationOrchestrator> _logger;
    private static readonly Meter SyncMeter = new("PlexAutoplay.Sync");
    private static readonly Counter<int> SyncCounter = SyncMeter.CreateCounter<int>("plex_catalog_sync_total");
    private static readonly Histogram<double> SyncDurationHistogram = SyncMeter.CreateHistogram<double>("plex_scan_duration_ms");

    public SynchronizationOrchestrator(
        CatalogSyncUseCase catalogSync,
        EpisodeMarkerSyncUseCase markerSync,
        IScanJobStore scanJobs,
        ILogger<SynchronizationOrchestrator> logger)
    {
        _catalogSync = catalogSync;
        _markerSync = markerSync;
        _scanJobs = scanJobs;
        _logger = logger;
    }

    /// <summary>
    /// Executa etapa 1 (catálogo) e, em caso de sucesso, etapa 2 (marcadores).
    /// Logs cumulativos vão para <see cref="IScanJobStore"/> do <paramref name="jobId"/>.
    /// </summary>
    public async Task<SyncResult> RunAsync(string jobId, CancellationToken ct)
    {
        var syncStart = DateTime.UtcNow;
        var scanJob = _scanJobs.Get(jobId);
        if (scanJob == null)
        {
            throw new InvalidOperationException($"Job {jobId} não encontrado no store");
        }

        // Etapa 1 — catálogo
        _scanJobs.AddLog(jobId, "info", "Iniciando etapa 1: sincronização de catálogo");
        var catalogResult = await _catalogSync.RunAsync(ct);

        if (!catalogResult.Success)
        {
            _scanJobs.AddLog(jobId, "error", catalogResult.ErrorMessage ?? "Falha na etapa 1");
            _logger.LogError("Etapa 1 (catálogo) falhou: {Error}", catalogResult.ErrorMessage);
            return SyncResult.Fail(catalogResult.ErrorMessage ?? "Falha na sincronização de catálogo");
        }

        _scanJobs.AddLog(jobId, "info",
            $"Etapa 1 concluída: {catalogResult.LibrariesInserted} bibliotecas inseridas, {catalogResult.LibrariesUpdated} atualizadas, {catalogResult.LibrariesRemoved} removidas; {catalogResult.SeriesInserted} séries inseridas, {catalogResult.SeriesUpdated} atualizadas, {catalogResult.SeriesRemoved} removidas");

        // Etapa 2 — marcadores
        _scanJobs.AddLog(jobId, "info", "Iniciando etapa 2: varredura de marcadores");
        var markerResult = await _markerSync.RunAsync(ct);

        if (!markerResult.Success)
        {
            _scanJobs.AddLog(jobId, "error", markerResult.ErrorMessage ?? "Falha na etapa 2");
            _logger.LogError("Etapa 2 (marcadores) falhou: {Error}", markerResult.ErrorMessage);
            return SyncResult.Fail(markerResult.ErrorMessage ?? "Falha na varredura de marcadores");
        }

        _scanJobs.AddLog(jobId, "info",
            $"Etapa 2 concluída: {markerResult.Processed} processados, {markerResult.Skipped} pulados, {markerResult.Errors} erros");

        var durationMs = (DateTime.UtcNow - syncStart).TotalMilliseconds;
        SyncDurationHistogram.Record(durationMs);
        SyncCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));

        return SyncResult.Ok(
            catalogResult.LibrariesInserted, catalogResult.LibrariesUpdated, catalogResult.LibrariesRemoved,
            catalogResult.SeriesInserted, catalogResult.SeriesUpdated, catalogResult.SeriesRemoved,
            markerResult.Processed, markerResult.Skipped, markerResult.Errors);
    }
}

/// <summary>Resultado completo da sincronização unificada.</summary>
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
        int lIns, int lUpd, int lRem,
        int sIns, int sUpd, int sRem,
        int processed, int skipped, int errors) =>
        new(true, lIns, lUpd, lRem, sIns, sUpd, sRem, processed, skipped, errors, null);

    public static SyncResult Fail(string error) =>
        new(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, error);
}