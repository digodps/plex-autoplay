namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ScanLibrariesUseCase
{
    private readonly IMarkerDetector _markerDetector;
    private readonly IMarkerRepository _markerRepository;

    public ScanLibrariesUseCase(IMarkerDetector markerDetector, IMarkerRepository markerRepository)
    {
        _markerDetector = markerDetector;
        _markerRepository = markerRepository;
    }

    public async Task ScanEpisodeAsync(string filePath, string ratingKey)
    {
        var alreadyHasIntro = await _markerRepository.ExistsAsync(ratingKey, "intro");
        var alreadyHasCredits = await _markerRepository.ExistsAsync(ratingKey, "credits");
        if (alreadyHasIntro && alreadyHasCredits)
        {
            return;
        }

        var detectedMarkers = _markerDetector.Detect(filePath);

        foreach (var marker in detectedMarkers)
        {
            var markerWithKey = marker with { RatingKey = ratingKey };
            var exists = await _markerRepository.ExistsAsync(ratingKey, markerWithKey.Kind);
            if (!exists)
            {
                await _markerRepository.AddAsync(markerWithKey);
            }
        }
    }

    public async Task<IEnumerable<Marker>> GetAllMarkersAsync()
    {
        return await _markerRepository.GetAllAsync();
    }
}
