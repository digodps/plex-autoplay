using PlexAutoplay.Domain.Entities;

namespace PlexAutoplay.Domain.Ports;

/// <summary>Acesso ao catálogo de séries.</summary>
public interface ISeriesRepository
{
    Task<(IEnumerable<Series> Items, int Total)> GetByLibraryAsync(string libraryRatingKey, int offset, int limit, string? query);
    Task<Series?> GetByRatingKeyAsync(string ratingKey);
    Task UpsertAsync(Series series);
    Task DeleteAsync(string ratingKey);
    Task DeleteByLibraryAsync(string libraryRatingKey);
}
