namespace PlexAutoplay.Web.Middleware;

using Microsoft.AspNetCore.Http;
using PlexAutoplay.Infrastructure.Auth;
using System.Threading.Tasks;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenService _tokenService;

    public JwtAuthMiddleware(RequestDelegate next, JwtTokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Allow WebSocket upgrade requests (PlexAutoSkip players on port 8081)
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        // Allow API auth endpoint without authentication
        if (path == "/auth/login")
        {
            await _next(context);
            return;
        }

        // Allow static file requests (Vue SPA assets)
        if (path.StartsWith("/_content/") || path.StartsWith("/css/") || path.StartsWith("/js/") || path.StartsWith("/assets/") || path.StartsWith("/favicon"))
        {
            await _next(context);
            return;
        }

        // Allow image proxy requests (Plex thumbs - public content, no sensitive data)
        // Browsers don't send Authorization headers or cookies in  tags,
        // so these must be accessible without authentication.
        if (path.StartsWith("/api/images/"))
        {
            await _next(context);
            return;
        }

        // Allow SPA routes (Vue Router handles these client-side)
        // Only API routes under /api/ require authentication
        if (!path.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        // Check for bearer token on API routes
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            if (_tokenService.ValidateToken(token))
            {
                await _next(context);
                return;
            }
        }

        // Also accept token from auth cookie (needed for EventSource/SSE which
        // cannot send custom Authorization headers)
        var cookieToken = context.Request.Cookies["auth_token"];
        if (!string.IsNullOrEmpty(cookieToken) && _tokenService.ValidateToken(cookieToken))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
}
