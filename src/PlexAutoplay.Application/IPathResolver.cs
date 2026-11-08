namespace PlexAutoplay.Application;

/// <summary>
/// Porta para resolver o caminho local de um arquivo reportado pelo Plex,
/// considerando os pontos de montagem locais (MEDIA_ROOTS).
/// </summary>
public interface IPathResolver
{
    string Resolve(IReadOnlyList<string> mediaRoots, string plexFilePath);
}
