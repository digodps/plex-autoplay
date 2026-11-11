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

<<<<<<< HEAD
    public async Task<IReadOnlyList<MarkerInfo>> GetMarkersForEpisodeAsync(string ratingKey)
    {
        var markers = await _markerRepository.GetByRatingKeyAsync(ratingKey);
        return markers.Select(marker => new MarkerInfo(marker.Kind, marker.StartMs, marker.EndMs)).ToList();
=======
    public IReadOnlyList<MarkerInfo> GetMarkersForEpisode(string ratingKey)
    {
        return _markerRepository.GetByRatingKeyAsync(ratingKey).GetAwaiter().GetResult()
            .Select(marker => new MarkerInfo(marker.Kind, marker.StartMs, marker.EndMs))
            .ToList();
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    }
}
