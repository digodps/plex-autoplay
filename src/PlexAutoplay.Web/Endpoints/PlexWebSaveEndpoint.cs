using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint para salvar configurações web do Plex.
/// </summary>
public static class PlexWebSaveEndpoint
{
    private const string EnvFilePath = "/home/hermes/plex-autoplay/.env";

    public static IEndpointRouteBuilder MapPlexWebSaveEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/plex/web-save", async (
            [FromBody] PlexSaveRequest request,
            CancellationToken cancellationToken) =>
        {
            if (request == null)
            {
                return Results.BadRequest("Corpo da requisição inválido");
            }

            // Validar formato de URL
            if (!Uri.TryCreate(request.PlexUrl, UriKind.Absolute, out var uri) ||
                !uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("URL do Plex inválida. Use http:// ou https://");
            }

            // Ler .env atual
            var envLines = new List<string>();
            if (File.Exists(EnvFilePath))
            {
                envLines = File.ReadAllLines(EnvFilePath).ToList();
            }

            // Atualizar ou adicionar PLEX_URL e PLEX_TOKEN
            var urlKey = "PLEX_URL=";
            var tokenKey = "PLEX_TOKEN=";
            var urlUpdated = false;
            var tokenUpdated = false;

            for (int i = 0; i < envLines.Count; i++)
            {
                if (envLines[i].StartsWith(urlKey))
                {
                    envLines[i] = urlKey + request.PlexUrl;
                    urlUpdated = true;
                }
                else if (envLines[i].StartsWith(tokenKey))
                {
                    envLines[i] = tokenKey + request.PlexToken;
                    tokenUpdated = true;
                }
            }

            if (!urlUpdated)
                envLines.Add(urlKey + request.PlexUrl);
            if (!tokenUpdated)
                envLines.Add(tokenKey + request.PlexToken);

            // Persistir .env
            await File.WriteAllLinesAsync(EnvFilePath, envLines, cancellationToken);

            return Results.Ok(new { success = true, message = "Configurações do Plex salvas com sucesso" });
        })
        .Produces<ApiResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithSummary("Salvar configurações Plex Web")
        .WithDescription("Recebe URL e token do frontend e persiste no arquivo .env");

        return endpoints;
    }

    /// <summary>
    /// Request model para salvar configurações do Plex.
    /// </summary>
    public class PlexSaveRequest
    {
        /// <summary>
        /// URL do servidor Plex
        /// </summary>
        [JsonPropertyName("plexUrl")]
        public string PlexUrl { get; set; } = "";

        /// <summary>
        /// Token de autenticação do Plex
        /// </summary>
        [JsonPropertyName("plexToken")]
        public string PlexToken { get; set; } = "";
    }
}