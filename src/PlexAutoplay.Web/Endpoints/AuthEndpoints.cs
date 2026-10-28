using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Infrastructure.Auth;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de autenticação (POST /auth/login emite JWT).
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app, string adminPassword)
    {
        app.MapGet("/auth/login", () => Results.Ok(new { message = "Use POST to authenticate" }));

        app.MapPost("/auth/login", (HttpContext context, [FromServices] JwtTokenService tokenService) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var json = reader.ReadToEndAsync().Result;

            var password = ExtractPassword(json);

            if (password == adminPassword)
            {
                var token = tokenService.GenerateToken();
                return Results.Ok(new { token });
            }

            return Results.Unauthorized();
        });
    }

    private static string? ExtractPassword(string json)
    {
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""password""\s*:\s*""([^""]*)""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
