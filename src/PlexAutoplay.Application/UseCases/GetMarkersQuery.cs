namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class GetMarkersQuery
{
    private readonly IMarkerRepository _markerRepository;

    public GetMarkersQuery(IMarkerRepository markerRepository)
    {
        _markerRepository = markerRepository;
    }

    public async Task<IEnumerable<Marker>> GetAllAsync()
    {
        return await _markerRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Marker>> GetByRatingKeyAsync(string ratingKey)
    {
        return (await _markerRepository.GetByRatingKeyAsync(ratingKey)).ToList();
    }
}
