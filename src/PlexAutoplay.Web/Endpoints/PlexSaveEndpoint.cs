using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint para salvar configurações do Plex no .env do container.
/// Recebe URL e token do frontend e persiste no arquivo .env.
/// </summary>
public static class PlexSaveEndpoint
{
    private const string EnvFilePath = "/home/hermes/plex-autoplay/.env";

    public static void MapPlexSaveEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plex/save", async (
            [FromBody] PlexSaveRequest request,
            [FromServices] ILogger<PlexSaveEndpoint> logger) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlexUrl))
                {
                    return Results.BadRequest("URL do Plex é obrigatória");
                }

                // Validate URL format (basic check)
                if (!request.PlexUrl.StartsWith("http://") && !request.PlexUrl.StartsWith("https://"))
                {
                    return Results.BadRequest("URL do Plex deve começar com http:// ou https://");
                }

                // Read existing .env file
                string[] envLines;
                if (System.IO.File.Exists(EnvFilePath))
                {
                    envLines = await System.IO.File.ReadAllLinesAsync(EnvFilePath);
                }
                else
                {
                    // Create default .env if doesn't exist
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

                // Update PLEX_URL and PLEX_TOKEN
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

                // If .env didn't have PLEX_URL/PLEX_TOKEN, add them before the first empty line after "# Plex"
                if (!updated)
                {
                    // Find the "# Plex" section and add at the end of that section
                    int plexSectionEnd = 0;
                    for (int i = 0; i < envLines.Length; i++)
                    {
                        if (envLines[i].StartsWith("# Plex"))
                        {
                            plexSectionEnd = i;
                            // Find next section or end
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
                    newLines.Insert(plexSectionEnd,
                        $"PLEX_URL={request.PlexUrl}");
                    newLines.Insert(plexSectionEnd + 1,
                        $"PLEX_TOKEN={request.PlexToken ?? ""}");
                    envLines = newLines.ToArray();
                }

                // Write back the .env file
                await System.IO.File.WriteAllLinesAsync(EnvFilePath, envLines, Encoding.UTF8);

                logger.LogInformation("Plex config saved: URL={Url}, Token={TokenLen} chars", 
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
                logger.LogError(ex, "Erro ao salvar configurações do Plex");
                return Results.Problem($"Erro interno: {ex.Message}", statusCode: 500);
            }
        });
    }

    /// <summary>
    /// Request model for saving Plex config
    /// </summary>
    public class PlexSaveRequest
    {
        [JsonPropertyName("plexUrl")]
        public string PlexUrl { get; set; } = "";

        [JsonPropertyName("plexToken")]
        public string PlexToken { get; set; } = "";
    }
}