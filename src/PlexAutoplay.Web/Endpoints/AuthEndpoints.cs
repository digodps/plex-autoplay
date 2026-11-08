using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexAutoplay.Application.DTOs;
using PlexAutoplay.Application.UseCases;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de autenticação (POST /auth/login define cookie).
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app, string adminPassword, bool disableHttp = false)
    {
        app.MapGet("/auth/login", () => Results.Ok(new { message = "Use POST to authenticate" }));

        app.MapPost("/auth/login", async (
            HttpContext context,
            [FromServices] LoginUseCase loginUseCase,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LoginRequest? request) =>
        {
            var password = request?.Password ?? "";

            var result = await loginUseCase.TryGenerateAsync(password);

            if (!result.Success)
            {
                return Results.Unauthorized();
            }

            // Cookie legível pelo frontend: o guarda do Vue lê o token em
            // document.cookie e o envia como Bearer.
            var cookieSecure = !disableHttp; // Secure only when HTTPS is enabled
            context.Response.Cookies.Append("auth_token", result.Token!, new CookieOptions
            {
                HttpOnly = false,
                Secure = cookieSecure,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                IsEssential = true
            });

            return Results.Ok(new { success = true, token = result.Token! });
        });
    }
}
