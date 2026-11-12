using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de status de sessões ativas e histórico de skips.
/// Lê snapshot mantido pelo ISkipMonitor (atualizado a cada 2s pelo background service).
/// </summary>
public static class SessionsEndpoints
{
    public static void MapSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sessions/skip-status",
            ([FromServices] ISkipMonitor monitor) =>
            {
                return Results.Ok(new
                {
                    activeSessions = monitor.Sessions.Select(s => new
                    {
                        s.MachineIdentifier,
                        s.Product,
                        s.Platform,
                        s.RatingKey,
                        positionMs = s.PositionMs,
                        durationMs = s.DurationMs,
                        state = s.State,
                        nextMarker = s.NextMarker is null ? null : new
                        {
                            s.NextMarker.Kind,
                            s.NextMarker.StartMs,
                            s.NextMarker.EndMs,
                            msUntilSkip = s.NextMarker.MsUntilSkip
                        }
                    }),
                    recentSkips = monitor.RecentSkips.Select(s => new
                    {
                        s.Timestamp,
                        s.MachineIdentifier,
                        s.Product,
                        s.Platform,
                        s.RatingKey,
                        markerKind = s.MarkerKind,
                        skippedFromMs = s.SkippedFromMs,
                        skippedToMs = s.SkippedToMs
                    })
                });
            });
    }
}