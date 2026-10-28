namespace PlexAutoplay.Infrastructure.AutoSkip;

using System.Text.Json;
using System.Threading;

public class CustomJsonWriterService
{
    private readonly string _customJsonPath;
    private static readonly SemaphoreSlim _writeLock = new(1, 1);

    public CustomJsonWriterService(string customJsonPath)
    {
        _customJsonPath = customJsonPath;
    }

    public record MarkerEntry(string Kind, long StartMs, long EndMs);

    /// <summary>
    /// Reads all markers from custom.json. Returns an empty dictionary if the file does not exist or is invalid.
    /// </summary>
    public Dictionary<string, List<MarkerEntry>> ReadMarkers()
    {
        try
        {
            if (!File.Exists(_customJsonPath))
                return new();

            var json = File.ReadAllText(_customJsonPath);
            using var doc = JsonDocument.Parse(json);

            var markers = new Dictionary<string, List<MarkerEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var ratingKey = property.Name;
                var entryList = new List<MarkerEntry>();
                foreach (var marker in property.Value.EnumerateArray())
                {
                    entryList.Add(new MarkerEntry(
                        marker.GetProperty("kind").GetString() ?? "",
                        marker.GetProperty("start_ms").GetInt64(),
                        marker.GetProperty("end_ms").GetInt64()
                    ));
                }
                markers[ratingKey] = entryList;
            }
            return markers;
        }
        catch
        {
            return new();
        }
    }

    /// <summary>
    /// Atomically writes the full set of markers to custom.json in PlexAutoSkip format.
    /// </summary>
    public void WriteMarkers(Dictionary<string, List<MarkerEntry>> markers)
    {
        _writeLock.Wait();
        try
        {
            var dir = Path.GetDirectoryName(_customJsonPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var tmpPath = _customJsonPath + ".tmp";
            using (var stream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                writer.WriteStartObject();
                foreach (var kvp in markers.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    writer.WritePropertyName(kvp.Key);
                    writer.WriteStartArray();
                    foreach (var m in kvp.Value)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("kind", m.Kind);
                        writer.WriteNumber("start_ms", m.StartMs);
                        writer.WriteNumber("end_ms", m.EndMs);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }

            File.Move(tmpPath, _customJsonPath, overwrite: true);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Adds or updates markers for a specific rating key, preserving existing markers for other keys.
    /// </summary>
    public void UpsertMarkers(string ratingKey, IEnumerable<MarkerEntry> markers)
    {
        var all = ReadMarkers();
        all[ratingKey] = markers.ToList();
        WriteMarkers(all);
    }

    /// <summary>
    /// Removes markers for a specific rating key.
    /// </summary>
    public void RemoveMarkers(string ratingKey)
    {
        var all = ReadMarkers();
        all.Remove(ratingKey);
        WriteMarkers(all);
    }
}
