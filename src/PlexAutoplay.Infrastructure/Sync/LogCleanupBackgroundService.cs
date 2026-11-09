using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexAutoplay.Domain.Ports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlexAutoplay.Infrastructure.Sync;

/// <summary>
/// Serviço em segundo plano para limpeza periódica de logs de varredura antigos.
/// Executa no startup e depois a cada 24 horas.
/// </summary>
public class LogCleanupBackgroundService : BackgroundService
{
    private readonly ILogger<LogCleanupBackgroundService> _logger;
    private readonly IScanJobLogStore _scanJobLogStore;
    private readonly int _retentionDays;

    public LogCleanupBackgroundService(
        ILogger<LogCleanupBackgroundService> logger,
        IScanJobLogStore scanJobLogStore)
    {
        _logger = logger;
        _scanJobLogStore = scanJobLogStore;

        // Configuração via variável de ambiente LOG_RETENTION_DAYS (padrão 30)
        var envRetention = Environment.GetEnvironmentVariable("LOG_RETENTION_DAYS");
        _retentionDays = int.TryParse(envRetention, out var days) && days > 0 ? days : 30;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log cleanup service starting with {RetentionDays} day retention", _retentionDays);

        // Executa limpeza no startup
        await RunCleanupAsync(stoppingToken);

        // Agenda execuções periódicas (cada 24h)
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await RunCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during log cleanup cycle");
            }
        }

        _logger.LogInformation("Log cleanup service stopping");
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running scheduled log cleanup (retention: {Days} days)", _retentionDays);

        try
        {
            var cleaned = await _scanJobLogStore.CleanupOldLogsAsync(_retentionDays);
            _logger.LogInformation("Log cleanup completed: {Count} records removed (older than {Days} days)", cleaned, _retentionDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run log cleanup");
        }
    }
}