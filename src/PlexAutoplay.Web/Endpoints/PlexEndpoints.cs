using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
<<<<<<< HEAD
=======
using PlexAutoplay.Web.Services;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
using Serilog;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>Endpoints de integração com o Plex: bibliotecas, varredura de marcadores e episódios.</summary>
public static class PlexEndpoints
{
    public static void MapPlexEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plex/libraries", async ([FromServices] IPlexApiClient plexClient) =>
        {
            var result = await plexClient.GetLibrariesAsync();
            if (result.Failed) return Results.Problem(result.ErrorMessage ?? "Não foi possível consultar as bibliotecas no Plex. Verifique a conexão com o servidor e tente novamente.", statusCode: 502);
            return Results.Ok(result.Libraries.Where(l => l.Type == "show").ToList());
        });

        app.MapPost("/api/plex/scan", async (
            [FromServices] IPlexApiClient plexClient,
<<<<<<< HEAD
            [FromServices] LibraryScanUseCase libraryScan,
            [FromServices] IScanJobRepository scanJobs,
            [FromServices] IScanJobLogStore scanJobLogs,
            [FromServices] IPathResolver pathResolver,
            [FromServices] IAppSettingsStore settings) =>
=======
            [FromServices] IMarkerRepository markerRepo,
            [FromServices] IMarkerDetector markerDetector,
            [FromServices] IScanJobStore scanJobs) =>
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new ScanJob { JobId = jobId, Status = "queued", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await scanJobs.AddAsync(job);

            _ = Task.Run(async () =>
            {
                try
                {
                    job.Status = "running";
                    job.UpdatedAt = DateTime.UtcNow;
                    await scanJobs.UpdateAsync(job);

                    var librariesResult = await plexClient.GetLibrariesAsync();
                    if (librariesResult.Failed)
                    {
                        job.Status = "failed";
                        job.ErrorMessage = librariesResult.ErrorMessage ?? "Falha ao buscar bibliotecas do Plex";
                        job.UpdatedAt = DateTime.UtcNow;
<<<<<<< HEAD
                        await scanJobs.UpdateAsync(job);
                        return;
                    }
                    var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();
                    await scanJobLogs.AddLogAsync(jobId, "info", $"{seriesLibraries.Count} biblioteca(s) encontrada(s)");
=======
                        return;
                    }
                    var seriesLibraries = librariesResult.Libraries.Where(l => l.Type == "show").ToList();
                    scanJobs.AddLog(jobId, "info", $"{seriesLibraries.Count} biblioteca(s) encontrada(s)");
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

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
<<<<<<< HEAD
=======
                            {
                                errors++;
                                scanJobs.AddLog(jobId, "error", $"Falha ao buscar episódios da biblioteca {lib.Key}", "Verifique conexão com Plex e tente novamente.");
                                continue;
                            }
                            var episodes = episodesResult.Episodes;

                            foreach (var episode in episodes)
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
                            {
                                errors++;
                                await scanJobLogs.AddLogAsync(jobId, "error", $"Falha ao buscar episódios da biblioteca {lib.Key}", "Verifique conexão com Plex e tente novamente.");
                                continue;
                            }

                            var seeds = episodesResult.Episodes.Select(e => new EpisodeSeed(e.RatingKey, e.FilePath));
                            var current = await settings.GetAsync();
                            var summary = await libraryScan.ScanLibraryAsync(seeds, current.MediaRoots, origin: "detected");

<<<<<<< HEAD
                            processed += summary.Processed;
                            skipped += summary.Skipped;

                            if (summary.Skipped > 0)
                            {
                                await scanJobLogs.AddLogAsync(jobId, "skip", $"{summary.Skipped} episódio(s) pulado(s) na biblioteca {lib.Key}");
=======
                                // Resolve local media path
                                var localPath = MediaPathResolver.Resolve(mediaRoot, episode.FilePath);
                                if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                                {
                                    skipped++;
                                    scanJobs.AddLog(jobId, "skip", $"Episódio pulado: {episode.Title}");
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
                    await scanJobs.UpdateAsync(job);
                }
                catch (Exception ex)
                {
                    job.Status = "failed";
                    job.ErrorMessage = ex.Message;
                    job.UpdatedAt = DateTime.UtcNow;
                    await scanJobs.UpdateAsync(job);
                }
            });

            return Results.Ok(new { job_id = jobId });
        });

        app.MapGet("/library/{section_key}", async (string section_key, [FromServices] IPlexApiClient plexClient, [FromServices] IPathResolver pathResolver, [FromServices] IAppSettingsStore settings) =>
        {
            var episodesResult = await plexClient.GetEpisodesAsync(section_key);
            if (episodesResult.Failed) return Results.Problem(episodesResult.ErrorMessage ?? "Não foi possível consultar os episódios no Plex. Verifique a conexão com o servidor e tente novamente.", statusCode: 502);
<<<<<<< HEAD
            var current = await settings.GetAsync();
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
            var result = episodesResult.Episodes.Select(e => new
            {
                rating_key = e.RatingKey,
                title = e.Title,
                season = e.ParentIndex,
                episode = e.Index,
                file_path = e.FilePath,
                local_path = pathResolver.Resolve(current.MediaRoots, e.FilePath)
            });
            return Results.Ok(result);
        });
    }
}