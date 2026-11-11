using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexAutoplay.Application.DTOs;
using PlexAutoplay.Application.UseCases;
=======
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Text.Json;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

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
<<<<<<< HEAD
            var markers = await markersQuery.GetAllAsync();
=======
            var markers = await markerRepo.GetAllAsync();
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapGet("/api/markers/{ratingKey}", async (string ratingKey, [FromServices] GetMarkersQuery markersQuery) =>
        {
<<<<<<< HEAD
            var markers = await markersQuery.GetByRatingKeyAsync(ratingKey);
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapPost("/api/markers", async (
            HttpContext context,
            [FromServices] SaveMarkerUseCase saveMarker,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SaveMarkerRequest? body) =>
=======
            var markers = await markerRepo.GetByRatingKeyAsync(ratingKey);
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapPost("/api/markers", async (HttpContext context, [FromServices] IMarkerRepository markerRepo) =>
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
                ratingKey = effective.RatingKey,
                kind = effective.Kind ?? "",
                startMs = effective.StartMs ?? 0,
                endMs = effective.EndMs ?? 0,
                origin = "manual"
            });
=======
                RatingKey = ratingKey,
                Kind = kind ?? "",
                StartMs = startMs,
                EndMs = endMs,
                Origin = "manual"
            };

            await markerRepo.AddAsync(marker);
            return Results.Created($"/api/markers/{ratingKey}", new { ratingKey, kind, startMs, endMs, origin = "manual" });
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
        });

        app.MapPut("/api/markers/{ratingKey}/{kind}", async (
            string ratingKey,
            string kind,
            HttpContext context,
<<<<<<< HEAD
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
=======
            [FromServices] IMarkerRepository markerRepo) =>
        {
            long? startMs = null;
            long? endMs = null;

            // Aceita body JSON (StartMs/EndMs) e também query string (consistência com o POST)
            if (context.Request.Query["StartMs"].FirstOrDefault() is { } qsStart && long.TryParse(qsStart, out var s0))
                startMs = s0;
            if (context.Request.Query["EndMs"].FirstOrDefault() is { } qsEnd && long.TryParse(qsEnd, out var e0))
                endMs = e0;

            if (startMs is null || endMs is null)
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    if (startMs is null && root.TryGetProperty("StartMs", out var sm)) startMs = sm.GetInt64();
                    if (startMs is null && root.TryGetProperty("startMs", out var sm2)) startMs = sm2.GetInt64();
                    if (startMs is null && root.TryGetProperty("start_ms", out var sm3)) startMs = sm3.GetInt64();
                    if (endMs is null && root.TryGetProperty("EndMs", out var em)) endMs = em.GetInt64();
                    if (endMs is null && root.TryGetProperty("endMs", out var em2)) endMs = em2.GetInt64();
                    if (endMs is null && root.TryGetProperty("end_ms", out var em3)) endMs = em3.GetInt64();
                }
                catch (JsonException)
                {
                    return Results.BadRequest(new { error = "Invalid JSON body" });
                }
            }

            if (startMs is null || endMs is null)
            {
                return Results.BadRequest(new { error = "Missing StartMs/EndMs" });
            }

            var existing = await markerRepo.GetByRatingKeyAsync(ratingKey);
            var current = existing.FirstOrDefault(m => string.Equals(m.Kind, kind, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                return Results.NotFound(new { error = "Marker not found" });
            }

            await markerRepo.UpdateAsync(current with { StartMs = startMs.Value, EndMs = endMs.Value });

            return Results.Ok(new { ratingKey, kind, startMs, endMs, current.Origin });
        });

        // DELETE removes from SQLite only - custom.json is no longer present
        app.MapDelete("/api/markers/{ratingKey}/{kind}", async (
            string ratingKey,
            string kind,
            [FromServices] IMarkerRepository markerRepo) =>
        {
            await markerRepo.DeleteAsync(ratingKey, kind);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            return Results.Ok();
        });
    }
}