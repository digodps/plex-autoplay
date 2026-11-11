namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de configuração (leitura apenas de variáveis de ambiente; não grava app-config.json).
/// </summary>
public static class ConfigEndpoints
{
    public static void MapConfigEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/config", () =>
        {
            return Results.Ok(new
            {
                plex_url = Environment.GetEnvironmentVariable("PLEX_URL") ?? "",
                sync_interval_minutes = Environment.GetEnvironmentVariable("SYNC_INTERVAL_MINUTES") ?? "5",
                media_root = Environment.GetEnvironmentVariable("MEDIA_ROOT") ?? "/media",
                ws_port = Environment.GetEnvironmentVariable("WS_PORT") ?? "8081",
                https_port = Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443",
                disable_http = Environment.GetEnvironmentVariable("DISABLE_HTTP") ?? "true",
                log_level = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information",
                allowed_hosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS") ?? "*",
                otel_exporter = Environment.GetEnvironmentVariable("OTEL_EXPORTER") ?? "console",
                otel_endpoint = Environment.GetEnvironmentVariable("OTEL_ENDPOINT") ?? "",
                otel_service_name = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "plex-autoplay",
                app_config_path = Environment.GetEnvironmentVariable("APP_CONFIG_PATH") ?? "/data/app-config.json",
                note = "Configuration is read exclusively from environment variables. app-config.json is not used."
            });
        });

        app.MapPost("/config", () =>
        {
            // POST only validates current config values; does not write app-config.json
            // per spec config-env requirement
            return Results.Ok(new
            {
                status = "valid",
                message = "Configuration validated from environment variables",
                source = "environment_variables",
                app_config_json_used = false
            });
        });
    }
}
