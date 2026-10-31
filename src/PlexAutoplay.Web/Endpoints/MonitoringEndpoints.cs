using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Ports;
using Serilog;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de monitoramento e observabilidade: status do serviço, métricas Prometheus e resumo de telemetria.
/// </summary>
public static class MonitoringEndpoints
{
    public static void MapMonitoringEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/status", ([FromServices] IScanJobStore scanJobs, [FromServices] IProcessInfoService processInfo) =>
        {
            try
            {
                var lastJob = scanJobs.GetLatest();
                var process = processInfo;

                return Results.Ok(new
                {
                    service = "plex-autoplay",
                    version = "1.0.0",
                    lastJob = lastJob != null ? new
                    {
                        job_id = lastJob.JobId,
                        status = lastJob.Status,
                        processed = lastJob.Processed,
                        skipped = lastJob.Skipped,
                        errors = lastJob.Errors,
                        error = lastJob.ErrorMessage,
                        updated_at = lastJob.UpdatedAt
                    } : null,
                    process = new
                    {
                        cpu_total_ms = process.TotalProcessorTime.TotalMilliseconds,
                        memory_private_bytes = process.PrivateMemorySize64,
                        memory_working_set_bytes = process.WorkingSet64,
                        uptime_ms = (DateTime.UtcNow - process.StartTimeUtc).TotalMilliseconds
                    }
                });
            }
            catch (Exception ex)
            {
                // Nunca expor 500 ao cliente: logar com correlation ID e retornar 200 com dados parciais.
                // CorrelationId já está enriquecido no LogContext pelo CorrelationIdMiddleware.
                Log.Error(ex, "Falha inesperada ao montar resposta de /api/status; retornando 200 com dados parciais");

                return Results.Ok(new
                {
                    service = "plex-autoplay",
                    version = "1.0.0",
                    lastJob = (object?)null,
                    process = new
                    {
                        cpu_total_ms = 0,
                        memory_private_bytes = 0,
                        memory_working_set_bytes = 0,
                        uptime_ms = 0
                    }
                });
            }
        });

        // Prometheus metrics endpoint (real metrics, not hardcoded)
        app.MapGet("/metrics", ([FromServices] IScanJobStore scanJobs, [FromServices] IProcessInfoService processInfo) =>
        {
            var lastJob = scanJobs.GetLatest();
            var process = processInfo;
            var lines = new List<string>
            {
                "# HELP plex_autoplay_info Plex Autoplay service info",
                "# TYPE plex_autoplay_info gauge",
                $"plex_autoplay_info{{version=\"1.0.0\",service=\"plex-autoplay\"}} 1",
                "",
                "# HELP plex_autoplay_process_memory_private_bytes Private memory bytes",
                "# TYPE plex_autoplay_process_memory_private_bytes gauge",
                $"plex_autoplay_process_memory_private_bytes {process.PrivateMemorySize64}",
                "",
                "# HELP plex_autoplay_process_memory_working_set_bytes Working set bytes",
                "# TYPE plex_autoplay_process_memory_working_set_bytes gauge",
                $"plex_autoplay_process_memory_working_set_bytes {process.WorkingSet64}",
                "",
                "# HELP plex_autoplay_process_cpu_total_ms Total CPU milliseconds",
                "# TYPE plex_autoplay_process_cpu_total_ms counter",
                $"plex_autoplay_process_cpu_total_ms {process.TotalProcessorTime.TotalMilliseconds}",
                "",
                "# HELP plex_autoplay_process_uptime_ms Process uptime milliseconds",
                "# TYPE plex_autoplay_process_uptime_ms counter",
                $"plex_autoplay_process_uptime_ms {(DateTime.UtcNow - process.StartTimeUtc).TotalMilliseconds}",
                "",
                "# HELP plex_autoplay_last_job_processed Last job processed count",
                "# TYPE plex_autoplay_last_job_processed gauge",
                $"plex_autoplay_last_job_processed {lastJob?.Processed ?? 0}",
                "",
                "# HELP plex_autoplay_last_job_skipped Last job skipped count",
                "# TYPE plex_autoplay_last_job_skipped gauge",
                $"plex_autoplay_last_job_skipped {lastJob?.Skipped ?? 0}",
                "",
                "# HELP plex_autoplay_last_job_errors Last job errors count",
                "# TYPE plex_autoplay_last_job_errors gauge",
                $"plex_autoplay_last_job_errors {lastJob?.Errors ?? 0}"
            };
            return Results.Text(string.Join("\n", lines) + "\n", "text/plain; version=0.0.4; charset=utf-8");
        });

        // Observability summary endpoint
        app.MapGet("/api/observability/summary", () =>
        {
            var summary = new
            {
                service = "plex-autoplay",
                version = "1.0.0",
                telemetry = new
                {
                    tracing_enabled = true,
                    metrics_enabled = true,
                    logging_enabled = true
                },
                metrics = new
                {
                    http_requests_total = "counter",
                    http_request_duration_ms = "histogram",
                    plex_api_errors_total = "counter",
                    plex_scans_total = "counter",
                    plex_scan_duration_ms = "histogram"
                },
                endpoints = new[]
                {
                    "/metrics",
                    "/api/observability/summary"
                }
            };
            return Results.Ok(summary);
        });
    }
}