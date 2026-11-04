namespace PlexAutoplay.Application;

/// <summary>
/// Porta para persistir as configurações de conexão do Plex (URL e token)
/// no arquivo .env do container.
/// </summary>
public interface IPlexConfigStore
{
    Task SaveAsync(string plexUrl, string plexToken);
}
