namespace PlexAutoplay.Application.UseCases.Series;

using PlexAutoplay.Domain.Ports;

public class UpdateWatchProgressCommand
{
    private readonly ISeriesEpisodeRepository _repository;

    public UpdateWatchProgressCommand(ISeriesEpisodeRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(string ratingKey, bool watched, int watchProgress)
    {
        await _repository.UpdateWatchProgressAsync(ratingKey, watched, watchProgress);
    }
}