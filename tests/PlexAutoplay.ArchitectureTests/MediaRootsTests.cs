using PlexAutoplay.Application;
using PlexAutoplay.Infrastructure.PlexProxy;

namespace PlexAutoplay.ArchitectureTests;

public class MediaRootsTests
{
    [Fact]
    public void Normalize_canonicalizes_dedupes_and_rejects_invalid()
    {
        var dir = Directory.CreateTempSubdirectory("plex-roots-").FullName;
        try
        {
            var (roots, errors) = MediaRoots.Normalize([dir + "/", dir, "relativo", "/nao-existe-" + Guid.NewGuid(), "/"]);

            Assert.Equal([dir], roots);
            Assert.Equal(3, errors.Count);
            Assert.All(errors, error => Assert.StartsWith("media_roots:", error));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Normalize_rejects_empty_and_over_limit()
    {
        Assert.Single(MediaRoots.Normalize([]).Errors);
        Assert.Single(MediaRoots.Normalize(Enumerable.Repeat("/media", 33)).Errors);
    }

    [Fact]
    public void ParseEnvironment_accepts_json_and_separated_lists()
    {
        Assert.Equal(new[] { "/a", "/b" }, MediaRoots.ParseEnvironment("[\"/a\",\"/b\"]"));
        Assert.Equal(new[] { "/a", "/b", "/c" }, MediaRoots.ParseEnvironment("/a\n/b,/c"));
    }

    [Fact]
    public void PathResolver_prefers_exact_path_in_second_root_over_short_suffix_in_first()
    {
        var root1 = Directory.CreateTempSubdirectory("plex-root1-").FullName;
        var root2 = Directory.CreateTempSubdirectory("plex-root2-").FullName;
        try
        {
            var exact = Path.Combine(root2, "Show", "S01E01.mkv");
            Directory.CreateDirectory(Path.GetDirectoryName(exact)!);
            File.WriteAllText(exact, "");
            var homonym = Path.Combine(root1, "S01E01.mkv");
            File.WriteAllText(homonym, "");

            var resolved = new PlexAutoplay.Infrastructure.Platform.PathResolver()
                .Resolve([root1, root2], "/Volumes/Media/Show/S01E01.mkv");

            Assert.Equal(exact, resolved);
        }
        finally
        {
            Directory.Delete(root1, recursive: true);
            Directory.Delete(root2, recursive: true);
        }
    }

    [Fact]
    public void PathResolver_reports_fallback_when_nothing_exists()
    {
        var root = Directory.CreateTempSubdirectory("plex-root-").FullName;
        try
        {
            var resolved = new PlexAutoplay.Infrastructure.Platform.PathResolver()
                .Resolve([root], "/Volumes/Media/Show/S01E01.mkv");

            Assert.Equal(Path.Combine(root, "Volumes/Media/Show/S01E01.mkv"), resolved);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PlexUrl_preserves_path_prefix()
    {
        Assert.Equal(
            "https://exemplo.com/plex/library/sections",
            PlexApiClient.BuildUrl("https://exemplo.com/plex/", "/library/sections").ToString());
        Assert.Equal(
            "http://192.168.1.5:32400/library/sections",
            PlexApiClient.BuildUrl("http://192.168.1.5:32400", "library/sections").ToString());
    }
}
