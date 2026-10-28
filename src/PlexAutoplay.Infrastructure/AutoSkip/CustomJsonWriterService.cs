namespace PlexAutoplay.Infrastructure.AutoSkip;

using PlexAutoplay.Domain.Ports;
using System.Text.Json;
using System.Threading;

/// <summary>
/// Implementação de <see cref="ICustomJsonWriter"/> para o arquivo custom.json do PlexAutoSkip.
/// </summary>
public class CustomJsonWriterService : ICustomJsonWriter
{
    private readonly string _customJsonPath;
    private static readonly SemaphoreSlim _writeLock = new(1, 1);

    public CustomJsonWriterService(string customJsonPath)
    {
        _customJsonPath = customJsonPath;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyList<ICustomJsonWriter.MarkerEntry>> GetAllMarkers()
    {
        var markers = ReadMarkers();
        return markers.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<ICustomJsonWriter.MarkerEntry>)kvp.Value);
    }

    /// <inheritdoc />
    public void UpsertMarkers(string ratingKey, IEnumerable<ICustomJsonWriter.MarkerEntry> markers)
    {
        var all = ReadMarkers();
        all[ratingKey] = markers.ToList();
        WriteMarkers(new Dictionary<string, List<ICustomJsonWriter.MarkerEntry>>(all));
    }

    /// <inheritdoc />
    public void RemoveMarker(string ratingKey, string kind)
    {
        var all = ReadMarkers();
        if (all.TryGetValue(ratingKey, out var list))
        {
            var filtered = list.Where(m => m.Kind != kind).ToList();
            if (filtered.Count > 0)
                all[ratingKey] = filtered;
            else
                all.Remove(ratingKey);
            WriteMarkers(new Dictionary<string, List<ICustomJsonWriter.MarkerEntry>>(all));
        }
    }

    private Dictionary<string, List<ICustomJsonWriter.MarkerEntry>> ReadMarkers()
    {
        try
        {
            if (!File.Exists(_customJsonPath))
                return new(StringComparer.OrdinalIgnoreCase);

            var json = File.ReadAllText(_customJsonPath);
            using var doc = JsonDocument.Parse(json);

            var markers = new Dictionary<string, List<ICustomJsonWriter.MarkerEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var ratingKey = property.Name;
                var entryList = new List<ICustomJsonWriter.MarkerEntry>();
                foreach (var marker in property.Value.EnumerateArray())
                {
                    entryList.Add(new ICustomJsonWriter.MarkerEntry(
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
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Atomically writes the full set of markers to custom.json in PlexAutoSkip format.
    /// </summary>
    private void WriteMarkers(Dictionary<string, List<ICustomJsonWriter.MarkerEntry>> markers)
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
}