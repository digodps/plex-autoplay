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
        app.MapGet("/api/sync/status/{jobId}", (string jobId, [FromServices] IScanJobStore scanJobs) =>
        {
            var job = scanJobs.Get(jobId);
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
                    logs = job.Logs.ToArray()
                });
            }
            return Results.NotFound();
        });

        app.MapGet("/api/sync/log/stream/{jobId}", async (string jobId, HttpContext context, [FromServices] IScanJobStore scanJobs) =>
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            while (!context.RequestAborted.IsCancellationRequested)
            {
                var job = scanJobs.Get(jobId);
                if (job != null)
                {
                    await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { status = job.Status, processed = job.Processed, skipped = job.Skipped, errors = job.Errors, logs = job.Logs.ToArray() })}\n\n");
                    await context.Response.Body.FlushAsync();

                    if (job.Status == "completed" || job.Status == "failed" || job.Status == "cancelled")
                        break;
                }

                await Task.Delay(1000);
            }
        });

        app.MapPost("/api/sync/cancel/{jobId}", (string jobId, [FromServices] IScanJobStore scanJobs) =>
        {
            var job = scanJobs.Get(jobId);
            if (job != null)
            {
                job.Status = "cancelled";
                job.UpdatedAt = DateTime.UtcNow;
                return Results.Ok(new { cancelled = true });
            }
            return Results.NotFound();
        });
    }
}