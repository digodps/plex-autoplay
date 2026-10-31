namespace PlexAutoplay.Web.Endpoints;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public static class ThemeEndpoints
{
    public static void MapThemeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/theme", async (IUserThemeRepository repo) =>
        {
            var preference = await repo.GetAsync("admin");
            return Results.Ok(preference ?? new UserThemePreference
            {
                AccentColor = "#e5a00d",
                Mode = "system"
            });
        });

        app.MapPut("/api/theme", async (IUserThemeRepository repo, UserThemePreference preference) =>
        {
            if (preference.Mode is not ("light" or "dark" or "system") ||
                preference.AccentColor is not ("#e5a00d" or "#2563eb" or "#16a34a" or "#7c3aed" or "#db2777" or "#4f46e5"))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["theme"] = ["Tema inválido."]
                });
            }

            await repo.UpsertAsync(preference with { UserId = "admin" });
            return Results.NoContent();
        });
    }
}