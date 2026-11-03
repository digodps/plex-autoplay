using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace PlexAutoplay.Web.Endpoints;

public static class PlexSaveEndpointRouteExtensions
{
    public static void MapPlexSaveEndpoint(this IEndpointRouteBuilder app, ILoggerFactory loggerFactory)
    {
        app.MapPost("/api/plex/save", async (
            [FromBody] PlexSaveRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlexUrl))
                {
                    return Results.BadRequest("URL do Plex é obrigatória");
                }

                if (!request.PlexUrl.StartsWith("http://") && !request.PlexUrl.StartsWith("https://"))
                {
                    return Results.BadRequest("URL do Plex deve começar com http:// ou https://");
                }

                string[] envLines;
                if (System.IO.File.Exists("/home/hermes/plex-autoplay/.env"))
                {
                    envLines = await System.IO.File.ReadAllLinesAsync("/home/hermes/plex-autoplay/.env");
                }
                else
                {
                    envLines = new[]
                    {
                        "# Plex",
                        $"PLEX_URL={request.PlexUrl}",
                        $"PLEX_TOKEN={request.PlexToken}",
                        "",
                        "# Auth",
                        "JWT_SECRET=7f3c9b1e5a2d48c6b0e94f17ad63c852e1b47d90f3a6c28e5b1d704f9a3c6e21",
                        "ADMIN_PASSWORD=admin",
                        "",
                        "# Ports",
                        "WEB_PORT=8080",
                        "WS_PORT=8081",
                        "HTTPS_PORT=443",
                        "DISABLE_HTTP=false",
                        "",
                        "# App",
                        "ASPNETCORE_ENVIRONMENT=Production",
                        "LOG_LEVEL=Information",
                        "ALLOWED_HOSTS=*",
                        "SYNC_INTERVAL_MINUTES=5",
                        "APP_CONFIG_PATH=/data/app-config.json",
                        "",
                        "# Cert (empty = self-signed)",
                        "CERT_PATH=",
                        "CERT_PASSWORD=",
                        "CERT_BASE64=",
                        "",
                        "# OTEL",
                        "# OTEL_SERVICE_NAME=plex-autoplay"
                    };
                }

                bool updated = false;
                for (int i = 0; i < envLines.Length; i++)
                {
                    if (envLines[i].StartsWith("PLEX_URL="))
                    {
                        envLines[i] = $"PLEX_URL={request.PlexUrl}";
                        updated = true;
                    }
                    else if (envLines[i].StartsWith("PLEX_TOKEN="))
                    {
                        envLines[i] = $"PLEX_TOKEN={request.PlexToken ?? ""}";
                        updated = true;
                    }
                }

                if (!updated)
                {
                    int plexSectionEnd = 0;
                    for (int i = 0; i < envLines.Length; i++)
                    {
                        if (envLines[i].StartsWith("# Plex"))
                        {
                            plexSectionEnd = i;
                            for (int j = i + 1; j < envLines.Length; j++)
                            {
                                if (string.IsNullOrWhiteSpace(envLines[j]) || envLines[j].StartsWith("#"))
                                {
                                    plexSectionEnd = j;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    var newLines = new List<string>(envLines);
                    newLines.Insert(plexSectionEnd, $"PLEX_URL={request.PlexUrl}");
                    newLines.Insert(plexSectionEnd + 1, $"PLEX_TOKEN={request.PlexToken ?? ""}");
                    envLines = newLines.ToArray();
                }

                await System.IO.File.WriteAllLinesAsync("/home/hermes/plex-autoplay/.env", envLines, System.Text.Encoding.UTF8);

                loggerFactory.CreateLogger("PlexSaveEndpoint").LogInformation("Plex config saved: Url={Url}, Token={TokenLen} chars",
                    request.PlexUrl, request.PlexToken?.Length ?? 0);

                return Results.Ok(new
                {
                    success = true,
                    message = "Configurações do Plex salvas com sucesso no .env",
                    plexUrl = request.PlexUrl
                });
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger("PlexSaveEndpoint").LogError(ex, "Erro ao salvar configurações do Plex");
                return Results.Problem($"Erro interno: {ex.Message}", statusCode: 500);
            }
        });
    }
}
