namespace PlexAutoplay.Application.UseCases;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

public class SaveThemeCommand
{
    private readonly IUserThemeRepository _userThemeRepository;

    public SaveThemeCommand(IUserThemeRepository userThemeRepository)
    {
        _userThemeRepository = userThemeRepository;
    }

    public async Task SaveAsync(UserThemePreference preference)
    {
        await _userThemeRepository.UpsertAsync(preference with { UserId = "admin" });
    }
}
