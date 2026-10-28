using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.AutoSkip;
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
            var libraries = await plexClient.GetLibrariesAsync();
            var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();
            return Results.Ok(seriesLibraries);
        });

        app.MapPost("/api/plex/scan", async (
            [FromServices] PlexApiClient plexClient,
            [FromServices] IMarkerRepository markerRepo,
            [FromServices] IMarkerDetector markerDetector,
            [FromServices] CustomJsonWriterService customJsonWriter,
            [FromServices] ScanJobStore scanJobs) =>
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

                    var libraries = await plexClient.GetLibrariesAsync();
                    var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();

                    int processed = 0;
                    int skipped = 0;
                    int errors = 0;

                    foreach (var lib in seriesLibraries)
                    {
                        if (job.Status == "cancelled") break;

                        try
                        {
                            var episodes = await plexClient.GetEpisodesAsync(lib.Key);

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

                                var markerEntries = new List<CustomJsonWriterService.MarkerEntry>();
                                foreach (var marker in detectedMarkers)
                                {
                                    var exists = await markerRepo.ExistsAsync(episode.RatingKey, marker.Kind);
                                    if (!exists)
                                    {
                                        await markerRepo.AddAsync(marker with { RatingKey = episode.RatingKey });
                                        markerEntries.Add(new CustomJsonWriterService.MarkerEntry(marker.Kind, marker.StartMs, marker.EndMs));
                                    }
                                }

                                if (markerEntries.Count > 0)
                                {
                                    customJsonWriter.UpsertMarkers(episode.RatingKey, markerEntries);
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
            var episodes = await plexClient.GetEpisodesAsync(section_key);
            var result = episodes.Select(e => new
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
