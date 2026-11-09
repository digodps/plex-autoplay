using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de acompanhamento de jobs de varredura: status, log em streaming (SSE) e cancelamento.
/// </summary>
public static class SyncEndpoints
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sync/status/{jobId}", async (string jobId, [FromServices] IScanJobRepository scanJobs) =>
        {
            var job = await scanJobs.GetAsync(jobId);
            if (job != null)
            {
                return Results.Ok(new
                {
                    job_id = job.JobId,
                    status = job.Status,
                    processed = job.Processed,
                    skipped = job.Skipped,
                    errors = job.Errors,
                    error = job.ErrorMessage,
                    updated_at = job.UpdatedAt
                });
            }
            return Results.NotFound();
        });

        app.MapGet("/api/sync/log/stream/{jobId}", async (string jobId, HttpContext context, [FromServices] IScanJobRepository scanJobs, [FromServices] IScanJobLogStore scanJobLogs) =>
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            while (!context.RequestAborted.IsCancellationRequested)
            {
                var job = await scanJobs.GetAsync(jobId);
                var payload = new Dictionary<string, object>
                {
                    ["status"] = job?.Status ?? "notfound",
                    ["processed"] = job?.Processed ?? 0,
                    ["skipped"] = job?.Skipped ?? 0,
                    ["errors"] = job?.Errors ?? 0,
                    ["logs"] = job != null ? await scanJobLogs.GetLogsAsync(jobId, null, null, null, null, 1, 100) : Array.Empty<ScanLogEntry>()
                };
                await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(payload, System.Text.Json.JsonSerializerOptions.Web)}\n\n");
                await context.Response.Body.FlushAsync();

                if (job != null && (job.Status == "completed" || job.Status == "failed" || job.Status == "cancelled"))
                    break;

                await Task.Delay(1000);
            }
        });

        app.MapPost("/api/sync/cancel/{jobId}", async (string jobId, [FromServices] IScanJobRepository scanJobs) =>
        {
            var job = await scanJobs.GetAsync(jobId);
            if (job != null)
            {
                job.Status = "cancelled";
                job.UpdatedAt = DateTime.UtcNow;
                await scanJobs.UpdateAsync(job);
                return Results.Ok(new { cancelled = true });
            }
            return Results.NotFound();
        });

        // New endpoints for history and logs
        app.MapGet("/api/sync/history", async ([FromServices] IScanJobRepository scanJobs, int page = 1, int pageSize = 100) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 500) pageSize = 100;
            var jobs = await scanJobs.GetHistoryAsync(page, pageSize);
            return Results.Ok(jobs.Select(j => new
            {
                job_id = j.JobId,
                status = j.Status,
                processed = j.Processed,
                skipped = j.Skipped,
                errors = j.Errors,
                error = j.ErrorMessage,
                created_at = j.CreatedAt,
                updated_at = j.UpdatedAt
            }));
        });

        app.MapGet("/api/sync/logs", async (
            [FromServices] IScanJobLogStore scanJobLogs,
            string? jobId,
            DateTime? from,
            DateTime? to,
            string? level,
            string? text,
            int page = 1,
            int pageSize = 100) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 500) pageSize = 100;
            var logs = await scanJobLogs.GetLogsAsync(jobId, from, to, level, text, page, pageSize);
            return Results.Ok(logs.Select(l => new
            {
                timestamp = l.Timestamp,
                level = l.Level,
                message = l.Message,
                suggestion = l.Suggestion,
                jobId = l.JobId,
                jobStatus = l.JobStatus
            }));
        });

        app.MapPost("/api/sync/logs/clear", async ([FromServices] IScanJobLogStore scanJobLogs) =>
        {
            var count = await scanJobLogs.ClearLogsAsync();
            return Results.Ok(new { cleared = count });
        });

        app.MapPost("/api/sync/clear", async ([FromServices] IScanJobRepository scanJobs, [FromServices] IScanJobLogStore scanJobLogs) =>
        {
            var clearedJobs = await scanJobs.ClearJobsAsync();
            var clearedLogs = await scanJobLogs.ClearLogsAsync();
            return Results.Ok(new { cleared_jobs = clearedJobs, cleared_logs = clearedLogs });
        });

        app.MapPost("/api/sync/logs/cleanup", async ([FromServices] IScanJobLogStore scanJobLogs, int retentionDays = 30) =>
        {
            if (retentionDays < 1) retentionDays = 30;
            var count = await scanJobLogs.CleanupOldLogsAsync(retentionDays);
            return Results.Ok(new { cleaned = count });
        });
    }
}