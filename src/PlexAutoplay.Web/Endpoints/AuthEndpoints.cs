using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexAutoplay.Application;
using PlexAutoplay.Application.DTOs;
using PlexAutoplay.Application.UseCases;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de autenticação: setup inicial, login e troca de senha.
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app, bool disableHttp = false)
    {
        app.MapGet("/api/auth/setup/status", async ([FromServices] IAuthCredentialStore credentials) =>
            Results.Ok(new { setupRequired = !await credentials.IsConfiguredAsync() }));

        app.MapPost("/api/auth/setup", async (
            [FromServices] CompleteSetupUseCase setupUseCase,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SetupRequest? request) =>
        {
            var result = await setupUseCase.ExecuteAsync(request?.Token, request?.NewPassword);

            return result switch
            {
                CompleteSetupResult.Success => Results.NoContent(),
                CompleteSetupResult.InvalidToken => Results.Json(
                    new { message = "Token de setup inválido ou já utilizado" },
                    statusCode: StatusCodes.Status401Unauthorized),
                CompleteSetupResult.InvalidPassword => Results.BadRequest(
                    new { message = $"A senha deve ter entre {PasswordPolicy.MinLength} e {PasswordPolicy.MaxLength} caracteres" }),
                CompleteSetupResult.AlreadyConfigured => Results.Json(
                    new { message = "Configuração inicial já concluída" },
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });

        app.MapPost("/api/auth/login", Login);
        app.MapPost("/auth/login", Login);

        app.MapPut("/api/auth/password", async (
            [FromServices] ChangePasswordUseCase changePasswordUseCase,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ChangePasswordRequest? request) =>
        {
            var result = await changePasswordUseCase.ExecuteAsync(request?.CurrentPassword, request?.NewPassword);

            return result switch
            {
                ChangePasswordResult.Success => Results.NoContent(),
                ChangePasswordResult.InvalidPassword => Results.BadRequest(
                    new { message = $"A senha deve ter entre {PasswordPolicy.MinLength} e {PasswordPolicy.MaxLength} caracteres" }),
                ChangePasswordResult.WrongCurrentPassword => Results.Json(
                    new { message = "Senha atual incorreta" },
                    statusCode: StatusCodes.Status403Forbidden),
                ChangePasswordResult.NotConfigured => Results.Json(
                    new { message = "Configuração inicial não concluída" },
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });

        async Task<IResult> Login(
            HttpContext context,
            [FromServices] LoginUseCase loginUseCase,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LoginRequest? request)
        {
            var (success, token) = await loginUseCase.TryGenerateAsync(request?.Password ?? "");
            if (!success)
            {
                return Results.Unauthorized();
            }

            // Cookie legível pelo frontend: o guarda do Vue lê o token em
            // document.cookie e o envia como Bearer.
            var cookieSecure = !disableHttp; // Secure only when HTTPS is enabled
            context.Response.Cookies.Append("auth_token", token!, new CookieOptions
            {
                HttpOnly = false,
                Secure = cookieSecure,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                IsEssential = true
            });

            return Results.Ok(new { success = true, token });
        }
    }
}
