namespace PlexAutoplay.Infrastructure.AutoSkip;

using PlexAutoplay.Domain.Ports;

/// <summary>
/// Implementação de <see cref="IPlexAutoSkipService"/> baseada no SQLite.
/// </summary>
public sealed class PlexAutoSkipService : IPlexAutoSkipService
{
    private readonly IMarkerRepository _markerRepository;

    public PlexAutoSkipService(IMarkerRepository markerRepository)
    {
        _markerRepository = markerRepository;
    }

    public async Task<IReadOnlyList<MarkerInfo>> GetMarkersForEpisodeAsync(string ratingKey)
    {
        var markers = await _markerRepository.GetByRatingKeyAsync(ratingKey);
        return markers.Select(marker => new MarkerInfo(marker.Kind, marker.StartMs, marker.EndMs)).ToList();
    }
}
