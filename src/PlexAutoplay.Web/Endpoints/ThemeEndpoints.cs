namespace PlexAutoplay.Web.Endpoints;

using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Entities;

public static class ThemeEndpoints
{
    public static void MapThemeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/theme", async ([FromServices] GetThemeQuery themeQuery) =>
        {
            var preference = await themeQuery.GetAsync();
            return Results.Ok(preference ?? new UserThemePreference
            {
                AccentColor = "#171717",
                Mode = "system"
            });
        });

        app.MapPut("/api/theme", async ([FromServices] SaveThemeCommand themeCommand, UserThemePreference preference) =>
        {
            if (preference.Mode is not ("light" or "dark" or "system") ||
                preference.AccentColor is not ("#171717" or "#2563eb" or "#16a34a" or "#7c3aed" or "#db2777" or "#4f46e5"))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["theme"] = ["Tema inválido."]
                });
            }

            await themeCommand.SaveAsync(preference);
            return Results.NoContent();
        });
    }
}
