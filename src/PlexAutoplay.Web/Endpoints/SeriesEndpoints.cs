using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints do catálogo de séries: listagem paginada por biblioteca e episódios de uma série.
/// </summary>
public static class SeriesEndpoints
{
    public static void MapSeriesEndpoints(this IEndpointRouteBuilder app, string mediaRoot)
    {
        app.MapGet("/api/series/{sectionKey}", async (
            string sectionKey,
            [FromServices] IPlexApiClient plexClient,
            int? page = 1,
            int? pageSize = 24,
            string? query = null) =>
        {
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 24;
            var pageIndex = page is > 0 ? page.Value : 1;
            var containerStart = (pageIndex - 1) * size;

            var result = await plexClient.GetSeriesAsync(sectionKey, containerStart, size, query);

            if (result.Failed)
            {
                return Results.Problem(
                    "Não foi possível consultar o catálogo de séries no Plex. Verifique a conexão com o servidor e tente novamente.",
                    statusCode: 502);
            }

            return Results.Ok(new
            {
                items = result.Items.Select(s => new
                {
                    ratingKey = s.RatingKey,
                    title = s.Title,
                    summary = s.Summary,
                    thumb = string.IsNullOrEmpty(s.Thumb) ? null : "/api/images/" + s.Thumb.TrimStart('/')
                }),
                totalSize = result.TotalSize,
                page = pageIndex,
                pageSize = size
            });
        });

        app.MapGet("/api/series/{sectionKey}/{seriesKey}/episodes", async (
            string seriesKey,
            [FromServices] IPlexApiClient plexClient) =>
        {
            var episodes = await plexClient.GetSeriesEpisodesAsync(seriesKey);
            var result = episodes.Select(e => new
            {
                rating_key = e.RatingKey,
                title = e.Title,
                season = e.ParentIndex,
                episode = e.Index,
                local_path = MediaPathResolver.Resolve(mediaRoot, e.FilePath)
            });
            return Results.Ok(result);
        });
    }
}
