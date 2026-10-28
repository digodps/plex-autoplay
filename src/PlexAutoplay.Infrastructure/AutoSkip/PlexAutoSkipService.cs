namespace PlexAutoplay.Infrastructure.AutoSkip;

using System.Text.Json;
using System.Threading.Channels;

public class PlexAutoSkipService
{
    private readonly string _customJsonPath;
    private readonly Dictionary<string, List<MarkerInfo>> _markers = new();
    private readonly FileSystemWatcher? _watcher;

    public PlexAutoSkipService(string customJsonPath)
    {
        _customJsonPath = customJsonPath;
        LoadMarkers();

        // Watch for changes to custom.json
        var dir = Path.GetDirectoryName(_customJsonPath);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            _watcher = new FileSystemWatcher(dir, Path.GetFileName(_customJsonPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            };
            _watcher.Changed += (_, _) => LoadMarkers();
            _watcher.EnableRaisingEvents = true;
        }
    }

    private void LoadMarkers()
    {
        try
        {
            if (!File.Exists(_customJsonPath)) return;

            var json = File.ReadAllText(_customJsonPath);
            using var doc = JsonDocument.Parse(json);

            _markers.Clear();
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var ratingKey = property.Name;
                var markerList = new List<MarkerInfo>();
                foreach (var marker in property.Value.EnumerateArray())
                {
                    markerList.Add(new MarkerInfo(
                        marker.GetProperty("kind").GetString() ?? "",
                        marker.GetProperty("start_ms").GetInt64(),
                        marker.GetProperty("end_ms").GetInt64()
                    ));
                }
                _markers[ratingKey] = markerList;
            }
        }
        catch
        {
            // Ignore parse errors, keep existing markers
        }
    }

    public List<MarkerInfo> GetMarkersForEpisode(string ratingKey)
    {
        return _markers.TryGetValue(ratingKey, out var markers) ? markers : new List<MarkerInfo>();
    }
}

public record MarkerInfo(string Kind, long StartMs, long EndMs);
