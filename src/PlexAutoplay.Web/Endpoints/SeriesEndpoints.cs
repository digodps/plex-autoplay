using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Application.UseCases.Series;
using PlexAutoplay.Application.UseCases.UserPreferences;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints do catálogo de séries: listagem paginada por biblioteca, episódios de uma série,
/// progresso de visualização e preferência de auto-play.
/// </summary>
public static class SeriesEndpoints
{
    public static void MapSeriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/series/{sectionKey}", async (
            string sectionKey,
            [FromServices] GetSeriesQuery seriesQuery,
            int? page = 1,
            int? pageSize = 24,
            string? query = null) =>
        {
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 24;
            var pageIndex = page is > 0 ? page.Value : 1;
            var offset = (pageIndex - 1) * size;

            var (items, total) = await seriesQuery.ListAsync(sectionKey, offset, size, query);

            var resultItems = items.Select(s => new
            {
                ratingKey = s.RatingKey,
                title = s.Title,
                summary = s.Summary,
                thumb = string.IsNullOrEmpty(s.Thumb) ? null : "/api/images/" + s.Thumb.TrimStart('/'),
                genre = s.Genre
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
            [FromServices] IPlexApiClient plexClient,
            [FromServices] IPathResolver pathResolver,
            [FromServices] IAppSettingsStore settings,
            [FromServices] GetSeriesEpisodesQuery localEpisodesQuery) =>
        {
            var episodesResult = await plexClient.GetSeriesEpisodesAsync(seriesKey);

            if (episodesResult.Failed)
            {
                return Results.Problem(
                    episodesResult.ErrorMessage ?? "Não foi possível consultar os episódios da série no Plex. Verifique a conexão com o servidor e tente novamente.",
                    statusCode: 502);
            }

            var current = await settings.GetAsync();
            var localEpisodes = await localEpisodesQuery.GetAsync(seriesKey);
            var localEpisodeDict = localEpisodes.ToDictionary(e => e.RatingKey);

            var result = episodesResult.Episodes.Select(e =>
            {
                localEpisodeDict.TryGetValue(e.RatingKey, out var localEpisode);
                return new
                {
                    rating_key = e.RatingKey,
                    title = e.Title,
                    season = e.ParentIndex,
                    episode = e.Index,
                    local_path = pathResolver.Resolve(current.MediaRoots, e.FilePath),
                    thumb = string.IsNullOrEmpty(e.Thumb) ? null : "/api/images/" + e.Thumb.TrimStart('/'),
                    watched = localEpisode?.Watched ?? false,
                    watch_progress = localEpisode?.WatchProgress ?? 0,
                    duration_seconds = localEpisode?.DurationSeconds ?? 0
                };
            });
            return Results.Ok(result);
        });

        app.MapPost("/api/series/episodes/{ratingKey}/progress", async (
            string ratingKey,
            [FromBody] WatchProgressRequest request,
            [FromServices] UpdateWatchProgressCommand updateCommand) =>
        {
            await updateCommand.ExecuteAsync(ratingKey, request.Watched, request.WatchProgress);
            return Results.Ok();
        });

        app.MapGet("/api/user/preferences/autoplay", async (
            [FromServices] GetAutoPlayPreferenceQuery query) =>
        {
            var enabled = await query.GetAsync();
            return Results.Ok(new { enabled });
        });

        app.MapPut("/api/user/preferences/autoplay", async (
            [FromBody] AutoPlayPreferenceRequest request,
            [FromServices] SetAutoPlayPreferenceCommand command) =>
        {
            await command.ExecuteAsync(request.Enabled);
            return Results.Ok();
        });
    }

    public sealed record WatchProgressRequest(bool Watched, int WatchProgress);
    public sealed record AutoPlayPreferenceRequest(bool Enabled);
}