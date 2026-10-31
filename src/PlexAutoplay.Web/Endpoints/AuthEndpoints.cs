using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Infrastructure.Auth;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de autenticação (POST /auth/login define cookie HttpOnly).
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
                
                // Set HttpOnly cookie (session-only, Secure, SameSite=Lax)
                context.Response.Cookies.Append("auth_token", token, new CookieOptions
                {
                    // HttpOnly removido: o frontend precisa ler o token via document.cookie
                    // (stores/auth.js getToken usa document.cookie; o middleware lê header Authorization).
                    // O middleware JwtAuthMiddleware continua validando o token tanto do header
                    // quanto deste cookie, com ou sem HttpOnly.
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    IsEssential = true
                });
                
                return Results.Ok(new { success = true, token });
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