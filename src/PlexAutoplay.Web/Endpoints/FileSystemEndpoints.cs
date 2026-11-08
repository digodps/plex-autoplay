using Microsoft.AspNetCore.Mvc;

namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Navegador de diretórios do servidor usado pelo seletor de mídia.
/// Lista somente diretórios; nunca lê conteúdo de arquivos.
/// </summary>
public static class FileSystemEndpoints
{
    private const int MaxEntries = 2000;

    public static void MapFileSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/filesystem/directories", ([FromQuery] string? path) =>
        {
            var requested = string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

            if (!Path.IsPathRooted(requested))
            {
                return Results.BadRequest(new { errors = new[] { $"path: {requested} não é um caminho absoluto" } });
            }

            var full = Path.GetFullPath(requested);

            try
            {
                var entries = Directory
                    .GetDirectories(full)
                    .Where(directory => !Path.GetFileName(directory).StartsWith('.'))
                    .OrderBy(directory => Path.GetFileName(directory), StringComparer.OrdinalIgnoreCase)
                    .Take(MaxEntries)
                    .Select(directory => new
                    {
                        name = Path.GetFileName(directory),
                        path = directory
                    })
                    .ToList();

                var parent = full.Length > 1 ? Path.GetDirectoryName(full) : null;

                return Results.Ok(new { path = full, parent, entries });
            }
            catch (DirectoryNotFoundException)
            {
                return Results.Json(
                    new { message = $"Diretório não encontrado: {full}" },
                    statusCode: StatusCodes.Status404NotFound);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Json(
                    new { message = $"Sem permissão para listar: {full}" },
                    statusCode: StatusCodes.Status403Forbidden);
            }
            catch (IOException ex)
            {
                return Results.Json(
                    new { message = $"Falha ao listar {full}: {ex.Message}" },
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });
    }
}
