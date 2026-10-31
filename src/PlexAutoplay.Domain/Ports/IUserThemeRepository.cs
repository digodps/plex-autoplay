namespace PlexAutoplay.Domain.Ports;

using PlexAutoplay.Domain.Entities;
using System.Threading.Tasks;

public interface IUserThemeRepository
{
    Task<UserThemePreference?> GetAsync(string userId);
    Task UpsertAsync(UserThemePreference preference);
}
