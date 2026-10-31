using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Text.Json;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de CRUD de marcadores (intro/créditos) por RatingKey.
/// </summary>
public static class MarkerEndpoints
{
    public static void MapMarkerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/markers", async ([FromServices] IMarkerRepository markerRepo) =>
        {
            var markers = await markerRepo.GetAllAsync();
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapGet("/api/markers/{ratingKey}", async (string ratingKey, [FromServices] IMarkerRepository markerRepo) =>
        {
            var markers = await markerRepo.GetByRatingKeyAsync(ratingKey);
            return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs, m.Origin }));
        });

        app.MapPost("/api/markers", async (HttpContext context, [FromServices] IMarkerRepository markerRepo) =>
        {
            // Prefer query string (works with Playwright), fallback to JSON body (Vue form)
            var ratingKey = context.Request.Query["RatingKey"].FirstOrDefault() ?? "";
            var kind = context.Request.Query["Kind"].FirstOrDefault() ?? "";
            var startMsStr = context.Request.Query["StartMs"].FirstOrDefault() ?? "";
            var endMsStr = context.Request.Query["EndMs"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(ratingKey))
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                ratingKey = root.TryGetProperty("RatingKey", out var rk) ? rk.GetString() ?? ""
                            : root.TryGetProperty("rating_key", out var rk2) ? rk2.GetString() ?? ""
                            : root.TryGetProperty("ratingKey", out var rk3) ? rk3.GetString() ?? ""
                            : "";
                kind = root.TryGetProperty("Kind", out var k) ? k.GetString() ?? ""
                       : root.TryGetProperty("kind", out var k2) ? k2.GetString() ?? ""
                       : "";
                startMsStr = root.TryGetProperty("StartMs", out var sm) ? sm.GetInt64().ToString()
                             : root.TryGetProperty("start_ms", out var sm2) ? sm2.GetInt64().ToString()
                             : root.TryGetProperty("startMs", out var sm3) ? sm3.GetInt64().ToString()
                             : "";
                endMsStr = root.TryGetProperty("EndMs", out var em) ? em.GetInt64().ToString()
                           : root.TryGetProperty("end_ms", out var em2) ? em2.GetInt64().ToString()
                           : root.TryGetProperty("endMs", out var em3) ? em3.GetInt64().ToString()
                           : "";
            }

            var startMs = long.TryParse(startMsStr, out var sk) ? sk : 0;
            var endMs = long.TryParse(endMsStr, out var ek) ? ek : 0;

            if (string.IsNullOrEmpty(ratingKey))
            {
                return Results.BadRequest(new { error = "Missing RatingKey" });
            }

            var exists = await markerRepo.ExistsAsync(ratingKey, kind ?? "");
            if (exists)
            {
                return Results.Conflict(new { error = "Marker already exists" });
            }

            var marker = new PlexAutoplay.Domain.Entities.Marker
            {
                RatingKey = ratingKey,
                Kind = kind ?? "",
                StartMs = startMs,
                EndMs = endMs,
                Origin = "manual"
            };

            await markerRepo.AddAsync(marker);
            return Results.Created($"/api/markers/{ratingKey}", new { ratingKey, kind, startMs, endMs, origin = "manual" });
        });

        app.MapPut("/api/markers/{ratingKey}/{kind}", async (
            string ratingKey,
            string kind,
            HttpContext context,
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
            return Results.Ok();
        });
    }
}