namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SaveMarkerUseCase
{
    private readonly IMarkerRepository _markerRepository;

    public SaveMarkerUseCase(IMarkerRepository markerRepository)
    {
        _markerRepository = markerRepository;
    }

    public async Task AddMarkerAsync(string ratingKey, string kind, long startMs, long endMs)
    {
        var exists = await _markerRepository.ExistsAsync(ratingKey, kind);
        if (exists)
        {
            return;
        }

        var marker = new Marker
        {
            RatingKey = ratingKey,
            Kind = kind,
            StartMs = startMs,
            EndMs = endMs
        };

        await _markerRepository.AddAsync(marker);
    }

    public async Task RemoveMarkerAsync(string ratingKey, string kind)
    {
        await _markerRepository.DeleteAsync(ratingKey, kind);
    }

    public async Task<IEnumerable<Marker>> GetAllMarkersAsync()
    {
        return await _markerRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Marker>> GetMarkersByRatingKeyAsync(string ratingKey)
    {
        return (await _markerRepository.GetByRatingKeyAsync(ratingKey)).ToList();
    }
}
