namespace PlexAutoplay.Application;

/// <summary>
/// Porta para resolver o caminho local de um arquivo reportado pelo Plex,
/// considerando o ponto de montagem local (MEDIA_ROOT).
/// </summary>
public interface IPathResolver
{
    string Resolve(string mediaRoot, string plexFilePath);
}
