using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
<<<<<<< HEAD
using PlexAutoplay.Application.UseCases;
=======
using PlexAutoplay.Domain.Ports;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints do catálogo de séries: listagem paginada por biblioteca e episódios de uma série.
/// </summary>
public static class SeriesEndpoints
{
<<<<<<< HEAD
    public static void MapSeriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/series/{sectionKey}", async (
            string sectionKey,
            [FromServices] GetSeriesQuery seriesQuery,
=======
    public static void MapSeriesEndpoints(this IEndpointRouteBuilder app, string mediaRoot)
    {
        app.MapGet("/api/series/{sectionKey}", async (
            string sectionKey,
            [FromServices] ISeriesRepository seriesRepository,
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            int? page = 1,
            int? pageSize = 24,
            string? query = null) =>
        {
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 24;
            var pageIndex = page is > 0 ? page.Value : 1;
            var offset = (pageIndex - 1) * size;

<<<<<<< HEAD
            var (items, total) = await seriesQuery.ListAsync(sectionKey, offset, size, query);
=======
            var (items, total) = await seriesRepository.GetByLibraryAsync(sectionKey, offset, size, query);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

            var resultItems = items.Select(s => new
            {
                ratingKey = s.RatingKey,
                title = s.Title,
                summary = s.Summary,
                thumb = string.IsNullOrEmpty(s.Thumb) ? null : "/api/images/" + s.Thumb.TrimStart('/')
            });

            return Results.Ok(new
            {
                items = resultItems,
                totalSize = total,
                page = pageIndex,
                pageSize = size
            });
        });

        app.MapGet("/api/series/{sectionKey}/{seriesKey}/episodes", async (
            string seriesKey,
<<<<<<< HEAD
            [FromServices] IPlexApiClient plexClient,
            [FromServices] IPathResolver pathResolver,
            [FromServices] IAppSettingsStore settings) =>
=======
            [FromServices] IPlexApiClient plexClient) =>
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
        {
            var episodesResult = await plexClient.GetSeriesEpisodesAsync(seriesKey);

            if (episodesResult.Failed)
            {
                return Results.Problem(
                    episodesResult.ErrorMessage ?? "Não foi possível consultar os episódios da série no Plex. Verifique a conexão com o servidor e tente novamente.",
                    statusCode: 502);
            }

<<<<<<< HEAD
            var current = await settings.GetAsync();
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            var result = episodesResult.Episodes.Select(e => new
            {
                rating_key = e.RatingKey,
                title = e.Title,
                season = e.ParentIndex,
                episode = e.Index,
<<<<<<< HEAD
                local_path = pathResolver.Resolve(current.MediaRoots, e.FilePath),
                thumb = string.IsNullOrEmpty(e.Thumb) ? null : "/api/images/" + e.Thumb.TrimStart('/')
=======
                local_path = MediaPathResolver.Resolve(mediaRoot, e.FilePath)
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            });
            return Results.Ok(result);
        });
    }
}
