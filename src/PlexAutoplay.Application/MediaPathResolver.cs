namespace PlexAutoplay.Application;

public static class MediaPathResolver
{
    /// <summary>
    /// Resolves the local path of a Plex MediaPart.file.
    /// Plex reports paths from its own mount (e.g. /data/Animes/Show/E01.mkv) while this app
    /// mounts the same files under MEDIA_ROOT (e.g. /media/Animes/Show/E01.mkv). Probe the
    /// trailing segments until one exists on disk, falling back to MEDIA_ROOT + full path.
    /// ponytail: brute-force suffix probe; replace with an explicit PLEX_PATH_MAP prefix
    /// table if mounts ever diverge by more than a leading path prefix.
    /// </summary>
    public static string Resolve(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        var root = string.IsNullOrEmpty(mediaRoot) || mediaRoot == "/" ? "" : mediaRoot.TrimEnd('/');
        var file = plexFilePath.Replace('\\', '/');
        if (!file.StartsWith('/'))
            file = "/" + file;

        var segments = file.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var start = 0; start < segments.Length; start++)
        {
            var candidate = root + "/" + string.Join('/', segments.Skip(start));
            if (File.Exists(candidate))
                return candidate;
        }

        return root + file;
    }
}
