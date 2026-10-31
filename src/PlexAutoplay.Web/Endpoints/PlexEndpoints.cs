using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Web.Services;
using Serilog;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Endpoints de integração com o Plex: bibliotecas, varredura de marcadores e episódios.
/// </summary>
public static class PlexEndpoints
{
    public static void MapPlexEndpoints(this IEndpointRouteBuilder app, string mediaRoot)
    {
        app.MapGet("/api/plex/libraries", async ([FromServices] IPlexApiClient plexClient) =>
        {
            var result = await plexClient.GetLibrariesAsync();

            if (result.Failed)
            {
                return Results.Problem(
                    result.ErrorMessage ?? "Não foi possível consultar as bibliotecas no Plex. Verifique a conexão com o servidor e tente novamente.",
                    statusCode: 502);
            }

            var seriesLibraries = result.Libraries.Where(l => l.Type == "show").ToList();
            return Results.Ok(seriesLibraries);
        });

        app.MapPost("/api/plex/scan", async (
            [FromServices] IPlexApiClient plexClient,
            [FromServices] IMarkerRepository markerRepo,
            [FromServices] IMarkerDetector markerDetector,
            [FromServices] IScanJobStore scanJobs) =>
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new ScanJob { JobId = jobId, Status = "queued" };
            scanJobs.Add(job);

            _ = Task.Run(async () =>
            {
                try
                {
                    job.Status = "running";
                    job.UpdatedAt = DateTime.UtcNow;

                    var librariesResult = await plexClient.GetLibrariesAsync();
                    if (librariesResult.Failed)
                    {
                        job.Status = "failed";
                        job.ErrorMessage = librariesResult.ErrorMessage ?? "Falha ao buscar bibliotecas do Plex";
                        job.UpdatedAt = DateTime.UtcNow;
                        return;
                    }
                    var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();

                    int processed = 0;
                    int skipped = 0;
                    int errors = 0;

                    foreach (var lib in seriesLibraries)
                    {
                        if (job.Status == "cancelled") break;

                        try
                        {
                            var episodesResult = await plexClient.GetEpisodesAsync(lib.Key);
                            if (episodesResult.Failed)
                            {
                                Log.Warning("Falha ao buscar episódios da biblioteca {LibKey}: {ErrorMessage}", lib.Key, episodesResult.ErrorMessage);
                                errors++;
                                continue;
                            }
                            var episodes = episodesResult.Episodes;

                            foreach (var episode in episodes)
                            {
                                if (job.Status == "cancelled") break;

                                // Skip if already has both intro and credits markers
                                var hasIntro = await markerRepo.ExistsAsync(episode.RatingKey, "intro");
                                var hasCredits = await markerRepo.ExistsAsync(episode.RatingKey, "credits");
                                if (hasIntro && hasCredits)
                                {
                                    skipped++;
                                    continue;
                                }

                                // Resolve local media path
                                var localPath = MediaPathResolver.Resolve(mediaRoot, episode.FilePath);
                                if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                                {
                                    skipped++;
                                    continue;
                                }

                                // Detect markers via ffprobe
                                var detectedMarkers = markerDetector.Detect(localPath);

                                foreach (var marker in detectedMarkers)
                                {
                                    var exists = await markerRepo.ExistsAsync(episode.RatingKey, marker.Kind);
                                    if (!exists)
                                    {
                                        await markerRepo.AddAsync(marker with { RatingKey = episode.RatingKey, Origin = "detected" });
                                    }
                                }

                                processed++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Error scanning library {LibKey}", lib.Key);
                            errors++;
                        }
                    }

                    job.Status = "completed";
                    job.Processed = processed;
                    job.Skipped = skipped;
                    job.Errors = errors;
                    job.UpdatedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    job.Status = "failed";
                    job.ErrorMessage = ex.Message;
                    job.UpdatedAt = DateTime.UtcNow;
                }
            });

            return Results.Ok(new { job_id = jobId });
        });

        app.MapGet("/library/{section_key}", async (string section_key, [FromServices] IPlexApiClient plexClient) =>
        {
            var episodesResult = await plexClient.GetEpisodesAsync(section_key);

            if (episodesResult.Failed)
            {
                return Results.Problem(
                    episodesResult.ErrorMessage ?? "Não foi possível consultar os episódios no Plex. Verifique a conexão com o servidor e tente novamente.",
                    statusCode: 502);
            }

            var result = episodesResult.Episodes.Select(e => new
            {
                rating_key = e.RatingKey,
                title = e.Title,
                season = e.ParentIndex,
                episode = e.Index,
                file_path = e.FilePath,
                local_path = MediaPathResolver.Resolve(mediaRoot, e.FilePath)
            });
            return Results.Ok(result);
        });
    }
}