namespace PlexAutoplay.Application;

public static class MediaPathResolver
{
    /// <summary>
    /// Resolves the local media file path by combining MEDIA_ROOT with the Plex MediaPart.file value.
    /// Example: MEDIA_ROOT="/media", file="/Volumes/Media/Show/S01E01.mkv" → "/media/Volumes/Media/Show/S01E01.mkv"
    /// </summary>
    public static string Resolve(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        var normalizedMediaRoot = mediaRoot.TrimEnd('/');
        var normalizedFilePath = plexFilePath.StartsWith("/") ? plexFilePath : "/" + plexFilePath;
        return normalizedMediaRoot + normalizedFilePath;
    }
}
