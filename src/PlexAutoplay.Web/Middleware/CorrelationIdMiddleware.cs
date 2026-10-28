namespace PlexAutoplay.Web.Middleware;

using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;
using System.Threading.Tasks;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Read correlation ID from request header or generate a new one
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store correlation ID in Items for access by downstream components
        context.Items[CorrelationIdHeader] = correlationId;

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }
            return Task.CompletedTask;
        });

        // Enrich Serilog logs with correlation ID
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Start an Activity so downstream spans (including HttpClient outgoing calls)
            // inherit the same trace context. The correlation ID is stored as a tag.
            using var activity = new Activity("HTTP " + context.Request.Method);
            activity.SetTag("correlation_id", correlationId);
            activity.Start();

            await _next(context);

            activity.Stop();
        }
    }
}
