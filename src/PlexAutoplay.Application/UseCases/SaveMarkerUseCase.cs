namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class SaveMarkerUseCase
{
    private readonly IMarkerRepository _markerRepository;

    public SaveMarkerUseCase(IMarkerRepository markerRepository)
    {
        _markerRepository = markerRepository;
    }

    public async Task<SaveMarkerResult> AddMarkerAsync(string ratingKey, string kind, long startMs, long endMs)
    {
        var exists = await _markerRepository.ExistsAsync(ratingKey, kind);
        if (exists)
        {
            return SaveMarkerResult.AlreadyExists;
        }

        await _markerRepository.AddAsync(new Marker
        {
            RatingKey = ratingKey,
            Kind = kind,
            StartMs = startMs,
            EndMs = endMs,
            Origin = "manual"
        });

        return SaveMarkerResult.Created;
    }

    /// <summary>Atualiza o intervalo de um marcador existente. Retorna null se não existir.</summary>
    public async Task<Marker?> UpdateMarkerAsync(string ratingKey, string kind, long startMs, long endMs)
    {
        var current = (await _markerRepository.GetByRatingKeyAsync(ratingKey))
            .FirstOrDefault(m => string.Equals(m.Kind, kind, StringComparison.OrdinalIgnoreCase));
        if (current is null)
        {
            return null;
        }

        var updated = current with { StartMs = startMs, EndMs = endMs };
        await _markerRepository.UpdateAsync(updated);
        return updated;
    }

    public async Task RemoveMarkerAsync(string ratingKey, string kind)
    {
        await _markerRepository.DeleteAsync(ratingKey, kind);
    }
}
