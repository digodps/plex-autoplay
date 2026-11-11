namespace PlexAutoplay.Application;

/// <summary>
/// Regra pura de montagem de caminho local a partir do MediaPart.file do Plex.
/// Plex reports paths from its own mount (e.g. /data/Animes/Show/E01.mkv) while this app
/// mounts the same files under one or more MEDIA_ROOTS (e.g. /media/Animes/Show/E01.mkv).
/// Não acessa o disco: apenas gera candidatos, ordenados por especificidade
/// (caminho completo primeiro; sufixos mais curtos depois) e, dentro de cada sufixo,
/// pela ordem das raízes configuradas.
/// </summary>
public static class MediaPathResolver
{
    /// <summary>Candidatos na ordem de preferência.</summary>
    public static IEnumerable<string> BuildCandidates(IReadOnlyList<string> mediaRoots, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            yield break;

        var roots = NormalizeRoots(mediaRoots);
        if (roots.Count == 0)
            yield break;

        var segments = NormalizeFile(plexFilePath).Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var start = 0; start < segments.Length; start++)
        {
            var suffix = string.Join('/', segments.Skip(start));
            foreach (var root in roots)
            {
                yield return root + "/" + suffix;
            }
        }
    }

    /// <summary>Caminho de fallback (primeira raiz + caminho completo normalizado).</summary>
    public static string Fallback(IReadOnlyList<string> mediaRoots, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        var roots = NormalizeRoots(mediaRoots);
        if (roots.Count == 0)
            return "";

        return roots[0] + NormalizeFile(plexFilePath);
    }

    /// <summary>Resolve um caminho local a partir do MediaPart.file do Plex.</summary>
    public static string Resolve(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        var roots = NormalizeRoots(new[] { mediaRoot });
        if (roots.Count == 0)
            return "";

        var segments = NormalizeFile(plexFilePath).Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var start = 0; start < segments.Length; start++)
        {
            var suffix = string.Join('/', segments.Skip(start));
            var candidate = roots[0] + "/" + suffix;
            if (System.IO.File.Exists(candidate))
                return candidate;
        }

        return Fallback(roots, plexFilePath);
    }

    private static List<string> NormalizeRoots(IReadOnlyList<string> mediaRoots) =>
        [.. (mediaRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(root => root.Trim().TrimEnd('/'))];

    private static string NormalizeFile(string plexFilePath)
    {
        var file = plexFilePath.Replace('\\', '/');
        return file.StartsWith('/') ? file : "/" + file;
    }
}
