using PlexAutoplay.Domain.Entities;

namespace PlexAutoplay.Domain.Ports;

/// <summary>Acesso ao progresso de visualização de episódios de série.</summary>
public interface ISeriesEpisodeRepository
{
    Task<SeriesEpisode?> GetByRatingKeyAsync(string ratingKey);
    Task<IEnumerable<SeriesEpisode>> GetBySeriesRatingKeyAsync(string seriesRatingKey);
    Task UpsertAsync(SeriesEpisode episode);
    Task UpdateWatchProgressAsync(string ratingKey, bool watched, int watchProgress);
    Task DeleteBySeriesRatingKeyAsync(string seriesRatingKey);
}