namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class GetSeriesQuery
{
    private readonly ISeriesRepository _repository;

    public GetSeriesQuery(ISeriesRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<Series> Items, int Total)> ListAsync(
        string libraryRatingKey, int offset, int limit, string? query)
    {
        return await _repository.GetByLibraryAsync(libraryRatingKey, offset, limit, query);
    }
}
