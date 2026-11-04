using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using PlexAutoplay.Application;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint para salvar configurações do Plex no .env do container.
/// Recebe URL e token do frontend e persiste via <see cref="IPlexConfigStore"/>.
/// </summary>
public static class PlexSaveEndpoint
{
    public static void MapPlexSaveEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plex/save", async (
            [FromBody] PlexSaveRequest request,
            [FromServices] IPlexConfigStore configStore,
            [FromServices] ILoggerFactory loggerFactory) =>
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

                await configStore.SaveAsync(request.PlexUrl, request.PlexToken ?? "");

                loggerFactory.CreateLogger("PlexSaveEndpoint").LogInformation("Plex config saved: URL={Url}, Token={TokenLen} chars",
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
