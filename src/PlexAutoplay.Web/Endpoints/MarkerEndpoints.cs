using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexAutoplay.Application;
using PlexAutoplay.Application.DTOs;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de CRUD de marcadores (intro/créditos) por RatingKey.
/// </summary>
public static class MarkerEndpoints
{
    public static void MapMarkerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/markers", async ([FromServices] GetMarkersQuery markersQuery) =>
        {
            var markers = await markersQuery.GetAllAsync();
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapGet("/api/markers/{ratingKey}", async (string ratingKey, [FromServices] GetMarkersQuery markersQuery) =>
        {
            var markers = await markersQuery.GetByRatingKeyAsync(ratingKey);
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapPost("/api/markers", async (
            HttpContext context,
            [FromServices] SaveMarkerUseCase saveMarker,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SaveMarkerRequest? body) =>
        {
            // Prefer query string (works with Playwright), fallback to JSON body (Vue form)
            var ratingKey = context.Request.Query["RatingKey"].FirstOrDefault();
            var kind = context.Request.Query["Kind"].FirstOrDefault();

            SaveMarkerRequest effective;
            if (!string.IsNullOrEmpty(ratingKey))
            {
                effective = new SaveMarkerRequest
                {
                    RatingKey = ratingKey!,
                    Kind = kind ?? "",
                    StartMs = long.TryParse(context.Request.Query["StartMs"].FirstOrDefault(), out var qStart) ? qStart : 0,
                    EndMs = long.TryParse(context.Request.Query["EndMs"].FirstOrDefault(), out var qEnd) ? qEnd : 0
                };
            }
            else
            {
                effective = body ?? new SaveMarkerRequest();
            }

            if (string.IsNullOrEmpty(effective.RatingKey))
            {
                return Results.BadRequest(new { error = "Missing RatingKey" });
            }

            var result = await saveMarker.AddMarkerAsync(
                effective.RatingKey, effective.Kind ?? "", effective.StartMs ?? 0, effective.EndMs ?? 0);

            if (result == SaveMarkerResult.AlreadyExists)
            {
                return Results.Conflict(new { error = "Marker already exists" });
            }

            return Results.Created($"/api/markers/{effective.RatingKey}", new
            {
                ratingKey = effective.RatingKey,
                kind = effective.Kind ?? "",
                startMs = effective.StartMs ?? 0,
                endMs = effective.EndMs ?? 0,
                origin = "manual"
            });
        });

        app.MapPut("/api/markers/{ratingKey}/{kind}", async (
            string ratingKey,
            string kind,
            HttpContext context,
            [FromServices] SaveMarkerUseCase saveMarker,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SaveMarkerRequest? body) =>
        {
            long? startMs = null;
            long? endMs = null;

            // Aceita body JSON (StartMs/EndMs) e também query string (consistência com o POST)
            if (long.TryParse(context.Request.Query["StartMs"].FirstOrDefault(), out var qsStart))
                startMs = qsStart;
            if (long.TryParse(context.Request.Query["EndMs"].FirstOrDefault(), out var qsEnd))
                endMs = qsEnd;

            if (startMs is null) startMs = body?.StartMs;
            if (endMs is null) endMs = body?.EndMs;

            if (startMs is null || endMs is null)
            {
                return Results.BadRequest(new { error = "Missing StartMs/EndMs" });
            }

            var updated = await saveMarker.UpdateMarkerAsync(ratingKey, kind, startMs.Value, endMs.Value);
            if (updated is null)
            {
                return Results.NotFound(new { error = "Marker not found" });
            }

            return Results.Ok(new { ratingKey, kind, startMs, endMs, origin = updated.Origin });
        });

        // DELETE removes from SQLite only - custom.json is no longer present
        app.MapDelete("/api/markers/{ratingKey}/{kind}", async (
            string ratingKey,
            string kind,
            [FromServices] SaveMarkerUseCase saveMarker) =>
        {
            await saveMarker.RemoveMarkerAsync(ratingKey, kind);
            return Results.Ok();
        });

        app.MapPost("/api/markers/reprocess/{seriesKey}", async (
            string seriesKey,
            [FromServices] IPlexApiClient plexClient,
            [FromServices] LibraryScanUseCase libraryScan,
            [FromServices] IMarkerRepository markerRepository,
            [FromServices] IAppSettingsStore settings) =>
        {
            var episodesResult = await plexClient.GetSeriesEpisodesAsync(seriesKey);
            if (episodesResult.Failed)
            {
                return Results.Problem(episodesResult.ErrorMessage ?? "Falha ao buscar episódios da série.", statusCode: 502);
            }

            var episodes = episodesResult.Episodes.ToList();
            foreach (var ep in episodes)
            {
                await markerRepository.DeleteAsync(ep.RatingKey, "intro");
                await markerRepository.DeleteAsync(ep.RatingKey, "credits");
            }

            var currentSettings = await settings.GetAsync();
            var seeds = episodes.Select(e => new EpisodeSeed(e.RatingKey, e.FilePath));
            var summary = await libraryScan.ScanLibraryAsync(seeds, currentSettings.MediaRoots, origin: "detected");

            return Results.Ok(new { processed = summary.Processed, skipped = summary.Skipped, total = episodes.Count });
        });
    }
}
