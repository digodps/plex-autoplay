using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PlexAutoplay.Infrastructure.Telemetry;

public static class OpenTelemetryService
{
    private static readonly ActivitySource ActivitySource = new("PlexAutoplay");

    public static ActivitySource Tracer => ActivitySource;

    public static IServiceCollection AddOpenTelemetryTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "plex-autoplay";
        var otelExporter = configuration["OTEL_EXPORTER"] ?? "console";
        var otelEndpoint = configuration["OTEL_ENDPOINT"] ?? "";

        var builder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                }))
            .WithTracing(tracing => tracing
                .AddSource(ActivitySource.Name)
                .AddSource("PlexAutoplay.Infrastructure")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .SetSampler(new AlwaysOnSampler()))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("PlexAutoplay"));

        // Configure exporters based on OTEL_EXPORTER
        if (otelExporter.Equals("otlp", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(otelEndpoint))
        {
            builder.WithTracing(tracing => tracing
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(otelEndpoint);
                }));
            builder.WithMetrics(metrics => metrics
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(otelEndpoint);
                }));
        }
        else if (otelExporter.Equals("console", StringComparison.OrdinalIgnoreCase))
        {
            builder.WithTracing(tracing => tracing
                .AddConsoleExporter());
            builder.WithMetrics(metrics => metrics
                .AddConsoleExporter());
        }
        else
        {
            // Default: Prometheus exporter for metrics
            builder.WithMetrics(metrics => metrics
                .AddPrometheusExporter());
        }

        return services;
    }
}
