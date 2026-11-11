namespace PlexAutoplay.Web.Endpoints;

<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Entities;
=======
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

public static class ThemeEndpoints
{
    public static void MapThemeEndpoints(this IEndpointRouteBuilder app)
    {
<<<<<<< HEAD
        app.MapGet("/api/theme", async ([FromServices] GetThemeQuery themeQuery) =>
        {
            var preference = await themeQuery.GetAsync();
            return Results.Ok(preference ?? new UserThemePreference
            {
                AccentColor = "#171717",
=======
        app.MapGet("/api/theme", async (IUserThemeRepository repo) =>
        {
            var preference = await repo.GetAsync("admin");
            return Results.Ok(preference ?? new UserThemePreference
            {
                AccentColor = "#e5a00d",
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
                Mode = "system"
            });
        });

<<<<<<< HEAD
        app.MapPut("/api/theme", async ([FromServices] SaveThemeCommand themeCommand, UserThemePreference preference) =>
        {
            if (preference.Mode is not ("light" or "dark" or "system") ||
                preference.AccentColor is not ("#171717" or "#2563eb" or "#16a34a" or "#7c3aed" or "#db2777" or "#4f46e5"))
=======
        app.MapPut("/api/theme", async (IUserThemeRepository repo, UserThemePreference preference) =>
        {
            if (preference.Mode is not ("light" or "dark" or "system") ||
                preference.AccentColor is not ("#e5a00d" or "#2563eb" or "#16a34a" or "#7c3aed" or "#db2777" or "#4f46e5"))
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["theme"] = ["Tema inválido."]
                });
            }

<<<<<<< HEAD
            await themeCommand.SaveAsync(preference);
            return Results.NoContent();
        });
    }
}
=======
            await repo.UpsertAsync(preference with { UserId = "admin" });
            return Results.NoContent();
        });
    }
}
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
