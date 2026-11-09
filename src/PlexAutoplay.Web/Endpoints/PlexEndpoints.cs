using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Ports;
using Serilog;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>Endpoints de integração com o Plex: bibliotecas e sincronização.</summary>
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

        app.MapPost("/api/plex/scan", (
            [FromServices] IScanJobStore scanJobs,
            [FromServices] SynchronizationOrchestrator orchestrator,
            [FromServices] ILoggerFactory loggerFactory) =>
        {
            var jobId = Guid.NewGuid().ToString();
            scanJobs.Add(new ScanJob { JobId = jobId, Status = "queued" });

            _ = Task.Run(async () =>
            {
                var logger = loggerFactory.CreateLogger("PlexScanAlias");
                var job = scanJobs.Get(jobId)!;
                job.Status = "running";
                job.UpdatedAt = DateTime.UtcNow;

                try
                {
                    var result = await orchestrator.RunAsync(jobId, CancellationToken.None);
                    job.Status = result.Success ? "completed" : "failed";
                    job.Processed = result.Stage2Processed;
                    job.Skipped = result.Stage2Skipped;
                    job.Errors = result.Stage2Errors;
                    job.ErrorMessage = result.ErrorMessage;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha ao executar sincronização forçada (alias /api/plex/scan)");
                    job.Status = "failed";
                    job.ErrorMessage = ex.Message;
                }
                finally
                {
                    job.UpdatedAt = DateTime.UtcNow;
                }
            });

            return Results.Ok(new { job_id = jobId });
        });

        app.MapGet("/library/{section_key}", async (string section_key, [FromServices] IPlexApiClient plexClient, [FromServices] IPathResolver pathResolver, [FromServices] IAppSettingsStore settings) =>
        {
            var episodesResult = await plexClient.GetEpisodesAsync(section_key);
            if (episodesResult.Failed) return Results.Problem(episodesResult.ErrorMessage ?? "Não foi possível consultar os episódios no Plex. Verifique a conexão com o servidor e tente novamente.", statusCode: 502);
            var current = await settings.GetAsync();
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