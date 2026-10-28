using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoint WebSocket do PlexAutoSkip: recebe estado de playback e envia comandos
/// de seek quando o player está dentro de um intervalo de intro/créditos.
/// </summary>
public static class AutoSkipWebSocketEndpoints
{
    public static void MapAutoSkipWebSocketEndpoints(this IEndpointRouteBuilder app)
    {
        app.Map("/ws", async (HttpContext context, [FromServices] IPlexAutoSkipService autoSkipService) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[4096];

                while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }

                    if (result.MessageType != System.Net.WebSockets.WebSocketMessageType.Text)
                        continue;

                    var json = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        string? ratingKey = root.TryGetProperty("ratingKey", out var rkEl) ? rkEl.GetString() : null;
                        string? state = root.TryGetProperty("state", out var stEl) ? stEl.GetString() : null;
                        long positionMs = root.TryGetProperty("positionMs", out var pmEl) ? pmEl.GetInt64() : 0;

                        if (string.IsNullOrEmpty(ratingKey) || string.IsNullOrEmpty(state))
                            continue;

                        // Only act on play state
                        if (!state.Equals("play", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var markers = autoSkipService.GetMarkersForEpisode(ratingKey);

                        foreach (var marker in markers)
                        {
                            // If player is within intro/credits range, send seek to end of segment
                            if (positionMs >= marker.StartMs && positionMs <= marker.EndMs)
                            {
                                var seekResponse = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    status = "seek",
                                    targetMs = marker.EndMs,
                                    reason = marker.Kind,
                                    ratingKey
                                });
                                var seekBytes = System.Text.Encoding.UTF8.GetBytes(seekResponse);
                                await webSocket.SendAsync(new ArraySegment<byte>(seekBytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            }
                        }

                        // If no seek needed, send heartbeat/ack
                        var ackResponse = System.Text.Json.JsonSerializer.Serialize(new { status = "ok", ratingKey });
                        var ackBytes = System.Text.Encoding.UTF8.GetBytes(ackResponse);
                        await webSocket.SendAsync(new ArraySegment<byte>(ackBytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        // Invalid JSON, ignore
                    }
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });
    }
}