namespace PlexAutoplay.Application;

/// <summary>
/// Porta para a credencial administrativa persistida no SQLite.
/// </summary>
public interface IAuthCredentialStore
{
    Task<bool> IsConfiguredAsync();
    Task<string?> GetHashAsync();
    Task<bool> CreateAsync(string passwordHash);
    Task<bool> UpdateAsync(string passwordHash);
}
