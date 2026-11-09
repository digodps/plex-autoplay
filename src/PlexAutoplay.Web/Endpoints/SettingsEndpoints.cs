using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using System.Text.Json.Serialization;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Configurações operacionais persistidas no SQLite. O token do Plex nunca é
/// devolvido; a página envia um novo token apenas quando deseja alterá-lo.
/// </summary>
public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/settings", async ([FromServices] IAppSettingsStore store) =>
        {
            var settings = await store.GetAsync();
            return Results.Ok(ToResponse(settings));
        });

        app.MapPut("/api/settings", async (
            [FromBody] SettingsRequest request,
            [FromServices] UpdateAppSettingsUseCase useCase,
            [FromServices] IAppSettingsStore store) =>
        {
            var result = await useCase.ExecuteAsync(new UpdateAppSettingsRequest(
                request.PlexUrl,
                request.PlexToken,
                request.MediaRoots,
                request.SyncIntervalMinutes));

            if (!result.Success)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(ToResponse(await store.GetAsync()));
        });
    }

    private static object ToResponse(AppSettingsSnapshot settings) => new
    {
        plex_url = settings.PlexUrl,
        plex_token_set = settings.PlexTokenSet,
        media_roots = settings.MediaRoots,
        sync_interval_minutes = settings.SyncIntervalMinutes
    };

    public sealed class SettingsRequest
    {
        [JsonPropertyName("plex_url")]
        public string? PlexUrl { get; set; }

        [JsonPropertyName("plex_token")]
        public string? PlexToken { get; set; }

        [JsonPropertyName("media_roots")]
        public IReadOnlyList<string>? MediaRoots { get; set; }

        [JsonPropertyName("sync_interval_minutes")]
        public int? SyncIntervalMinutes { get; set; }
    }
}
