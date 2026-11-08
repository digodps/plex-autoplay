namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Ports;

public record EpisodeSeed(string RatingKey, string FilePath);

public record ScanSummary(int Processed, int Skipped);

/// <summary>
/// Fronteira de escaneamento de bibliotecas: skip-se-já-tem-intro-e-credits,
/// resolve o caminho local, detecta marcadores (ffprobe) e grava no repositório.
/// </summary>
public class LibraryScanUseCase
{
    private readonly IMarkerDetector _markerDetector;
    private readonly IMarkerRepository _markerRepository;
    private readonly IPathResolver _pathResolver;

    public LibraryScanUseCase(
        IMarkerDetector markerDetector,
        IMarkerRepository markerRepository,
        IPathResolver pathResolver)
    {
        _markerDetector = markerDetector;
        _markerRepository = markerRepository;
        _pathResolver = pathResolver;
    }

    public async Task<ScanSummary> ScanLibraryAsync(
        IEnumerable<EpisodeSeed> seeds,
        IReadOnlyList<string> mediaRoots,
        string? origin = null)
    {
        var processed = 0;
        var skipped = 0;

        foreach (var seed in seeds)
        {
            var ratingKey = seed.RatingKey;

            if (await _markerRepository.ExistsAsync(ratingKey, "intro") &&
                await _markerRepository.ExistsAsync(ratingKey, "credits"))
            {
                skipped++;
                continue;
            }

            var localPath = _pathResolver.Resolve(mediaRoots, seed.FilePath);
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                skipped++;
                continue;
            }

            foreach (var marker in _markerDetector.Detect(localPath))
            {
                if (!await _markerRepository.ExistsAsync(ratingKey, marker.Kind))
                {
                    await _markerRepository.AddAsync(marker with { RatingKey = ratingKey, Origin = origin ?? marker.Origin });
                    processed++;
                }
            }
        }

        return new ScanSummary(processed, skipped);
    }
}
