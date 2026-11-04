using PlexAutoplay.Application;

namespace PlexAutoplay.Infrastructure.Platform;

/// <summary>
/// Resolve o caminho local do arquivo testando os candidatos de
/// <see cref="MediaPathResolver"/> no disco (File.Exists).
/// </summary>
public sealed class PathResolver : IPathResolver
{
    public string Resolve(string mediaRoot, string plexFilePath)
    {
        if (string.IsNullOrEmpty(plexFilePath))
            return "";

        foreach (var candidate in MediaPathResolver.BuildCandidates(mediaRoot, plexFilePath))
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return MediaPathResolver.Fallback(mediaRoot, plexFilePath);
    }
}
