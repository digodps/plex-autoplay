namespace PlexAutoplay.Application.UseCases.Series;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class GetSeriesEpisodesQuery
{
    private readonly ISeriesEpisodeRepository _repository;

    public GetSeriesEpisodesQuery(ISeriesEpisodeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SeriesEpisode>> GetAsync(string seriesRatingKey)
    {
        return await _repository.GetBySeriesRatingKeyAsync(seriesRatingKey);
    }
}