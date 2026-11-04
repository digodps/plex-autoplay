namespace PlexAutoplay.Application;

/// <summary>
/// Regra pura de montagem de caminho local a partir do MediaPart.file do Plex.
/// Plex reports paths from its own mount (e.g. /data/Animes/Show/E01.mkv) while this app
/// mounts the same files under MEDIA_ROOT (e.g. /media/Animes/Show/E01.mkv).
/// Não acessa o disco: apenas gera candidatos, do sufixo mais específico ao fallback.
/// ponytail: brute-force suffix probe; replace with an explicit PLEX_PATH_MAP prefix
/// table if mounts ever diverge by more than a leading path prefix.
/// </summary>
public static class MediaPathResolver
{
    /// <summary>Candidatos na ordem de preferência (candidato[0] == fallback MEDIA_ROOT + caminho).</summary>
    public static IEnumerable<string> BuildCandidates(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            yield break;

        var (root, file) = Normalize(mediaRoot, plexFilePath);
        var segments = file.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var start = 0; start < segments.Length; start++)
        {
            yield return root + "/" + string.Join('/', segments.Skip(start));
        }
    }

    /// <summary>Caminho de fallback (MEDIA_ROOT + caminho completo normalizado).</summary>
    public static string Fallback(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        var (root, file) = Normalize(mediaRoot, plexFilePath);
        return root + file;
    }

    private static (string Root, string File) Normalize(string mediaRoot, string plexFilePath)
    {
        var root = string.IsNullOrEmpty(mediaRoot) || mediaRoot == "/" ? "" : mediaRoot.TrimEnd('/');
        var file = plexFilePath.Replace('\\', '/');
        if (!file.StartsWith('/'))
            file = "/" + file;
        return (root, file);
    }
}
