using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Infrastructure.PlexProxy;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint de proxy de imagens do Plex (com cache).
/// </summary>
public static class ImageEndpoints
{
    public static void MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/images/{**imagePath}", async (string imagePath, [FromServices] ImageProxyService proxyService) =>
        {
            try
            {
                var imageBytes = await proxyService.GetImageAsync("/" + imagePath);
                return Results.File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 502);
            }
        });
    }
}
