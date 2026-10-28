namespace PlexAutoplay.Web.Middleware;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CsrfMiddleware
{
    private readonly RequestDelegate _next;
    private const string CsrfHeader = "X-CSRF-TOKEN";

    public CsrfMiddleware(RequestDelegate next)
    {
        _next = next;
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

        // Allow login endpoint without CSRF
        if (path == "/auth/login")
        {
            await _next(context);
            return;
        }

        // Only enforce CSRF on mutating API routes under /api/
        var method = context.Request.Method.ToUpperInvariant();
        var isMutatingApi = (method == "POST" || method == "PUT" || method == "DELETE")
            && path.StartsWith("/api/");

        if (!isMutatingApi)
        {
            await _next(context);
            return;
        }

        // Require CSRF token header
        var csrfToken = context.Request.Headers[CsrfHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(csrfToken))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"CSRF token required\"}");
            return;
        }

        await _next(context);
    }
}
