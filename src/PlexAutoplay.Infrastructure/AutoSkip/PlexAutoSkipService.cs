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

    public IReadOnlyList<MarkerInfo> GetMarkersForEpisode(string ratingKey)
    {
        return _markerRepository.GetByRatingKeyAsync(ratingKey).GetAwaiter().GetResult()
            .Select(marker => new MarkerInfo(marker.Kind, marker.StartMs, marker.EndMs))
            .ToList();
    }
}
