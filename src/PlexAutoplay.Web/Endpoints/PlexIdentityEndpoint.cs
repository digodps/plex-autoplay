using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using PlexAutoplay.Application;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint para validar token/URL do Plex chamando /identity do servidor Plex.
/// Usado pelo frontend para testar a conexão antes de salvar.
/// </summary>
public static class PlexIdentityEndpoint
{
    public static void MapPlexIdentityEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plex/identity", async (
            [FromBody] PlexIdentityRequest request,
            [FromServices] IPlexApiClient plexClient,
            [FromServices] ILoggerFactory loggerFactory) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlexUrl))
                {
                    return Results.BadRequest("URL do Plex é obrigatória");
                }

                var (plexUrl, plexUrlError) = PlexUrlRules.Normalize(request.PlexUrl);
                if (plexUrlError is not null)
                {
                    return Results.BadRequest("URL do Plex deve começar com http:// ou https://");
                }

                var result = await plexClient.GetIdentityAsync(plexUrl, request.PlexToken ?? "");

                var logger = loggerFactory.CreateLogger("PlexIdentityEndpoint");
                logger.LogInformation("Plex identity check: URL={Url}, Success={Success}", request.PlexUrl, !result.Failed);

                if (result.Failed)
                {
                    return Results.Ok(new
                    {
                        success = false,
                        message = result.ErrorMessage,
                        machineIdentifier = result.MachineIdentifier,
                        friendlyName = result.FriendlyName,
                        version = result.Version
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    message = "Conexão com Plex validada com sucesso",
                    machineIdentifier = result.MachineIdentifier,
                    friendlyName = result.FriendlyName,
                    version = result.Version
                });
            }
            catch (Exception ex)
            {
                var log = loggerFactory.CreateLogger("PlexIdentityEndpoint");
                log.LogError(ex, "Erro ao validar identidade do Plex");
                return Results.Problem($"Erro interno: {ex.Message}", statusCode: 500);
            }
        });
    }

    /// <summary>
    /// Request model for Plex identity validation
    /// </summary>
    public class PlexIdentityRequest
    {
        [JsonPropertyName("plexUrl")]
        public string PlexUrl { get; set; } = "";

        [JsonPropertyName("plexToken")]
        public string PlexToken { get; set; } = "";
    }
}
