namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class GetThemeQuery
{
    private readonly IUserThemeRepository _userThemeRepository;

    public GetThemeQuery(IUserThemeRepository userThemeRepository)
    {
        _userThemeRepository = userThemeRepository;
    }

    public async Task<UserThemePreference?> GetAsync()
    {
        return await _userThemeRepository.GetAsync("admin");
    }
}
