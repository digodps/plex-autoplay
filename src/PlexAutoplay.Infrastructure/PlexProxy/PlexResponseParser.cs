using System.Text.Json;
using PlexAutoplay.Application;

namespace PlexAutoplay.Infrastructure.PlexProxy;

/// <summary>
/// Parser puro (sem I/O) das respostas JSON da API do Plex, baseado em
/// desserialização tipada via System.Text.Json com case-insensitive matching.
/// </summary>
public static class PlexResponseParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Extrai o ID numérico da seção de uma chave do Plex.</summary>
    private static string ExtractSectionId(string? rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return rawKey ?? "";

        var trimmed = rawKey.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }

    public static IReadOnlyList<PlexLibraryDto> ParseLibraries(string json)
    {
        var root = JsonSerializer.Deserialize<PlexRoot>(json, Options);
        // Envelope padrão do Plex aninha Directory em MediaContainer;
        // alguns proxies/mock devolvem Directory na raiz.
        var directory = root?.Directory ?? root?.MediaContainer?.Directory;
        if (directory is null)
            return Array.Empty<PlexLibraryDto>();

        var libraries = new List<PlexLibraryDto>();
        foreach (var item in directory)
        {
            libraries.Add(new PlexLibraryDto(
                ExtractSectionId(item.Key),
                item.Title ?? "",
                item.Type ?? "",
                item.Agent ?? ""));
        }

        return libraries;
    }

    public static IReadOnlyList<PlexEpisodeDto> ParseEpisodes(string json)
        => ParseEpisodesInternal(json);

    public static IReadOnlyList<PlexEpisodeDto> ParseSeriesEpisodes(string json)
        => ParseEpisodesInternal(json);

    public static (IReadOnlyList<PlexSeriesDto> Items, int TotalSize) ParseSeries(string json)
    {
        var root = JsonSerializer.Deserialize<PlexRoot>(json, Options);
        var container = root?.MediaContainer;
        if (container is null)
            return (Array.Empty<PlexSeriesDto>(), 0);

        var series = new List<PlexSeriesDto>();
        if (container.Metadata is not null)
        {
            foreach (var item in container.Metadata)
            {
                // Filtro defensivo: apenas séries (por segurança contra versões que ignoram type=show)
                if (item.Type is not null && item.Type != "show")
                    continue;

                series.Add(new PlexSeriesDto(
                    item.RatingKey ?? "",
                    item.Title ?? "",
                    item.Summary ?? "",
                    item.Thumb ?? ""));
            }
        }

        return (series, container.TotalSize);
    }

    private static IReadOnlyList<PlexEpisodeDto> ParseEpisodesInternal(string json)
    {
        var root = JsonSerializer.Deserialize<PlexRoot>(json, Options);
        var metadata = root?.MediaContainer?.Metadata;
        if (metadata is null)
            return Array.Empty<PlexEpisodeDto>();

        var episodes = new List<PlexEpisodeDto>();
        foreach (var item in metadata)
        {
            // Only process episodes (type "episode")
            if (item.Type is not null && item.Type != "episode")
                continue;

            string? filePath = null;
            if (item.Media is not null)
            {
                foreach (var media in item.Media)
                {
                    if (media.Part is not null && media.Part.Count > 0)
                    {
                        filePath = media.Part[0].File;
                        break;
                    }
                }
            }

            episodes.Add(new PlexEpisodeDto(
                item.RatingKey ?? "",
                item.Title ?? "",
                item.ParentIndex,
                item.Index,
                filePath ?? ""));
        }

        return episodes;
    }

    // --- Tipos de resposta (desserialização tipada) ---

    private sealed class PlexRoot
    {
        public PlexContainer? MediaContainer { get; set; }
        public List<PlexDirectoryItem>? Directory { get; set; }
    }

    private sealed class PlexContainer
    {
        public int TotalSize { get; set; }
        public List<PlexMetadataItem>? Metadata { get; set; }
        public List<PlexDirectoryItem>? Directory { get; set; }
    }

    private sealed class PlexDirectoryItem
    {
        public string? Key { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Agent { get; set; }
    }

    private sealed class PlexMetadataItem
    {
        public string? RatingKey { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Thumb { get; set; }
        public string? Type { get; set; }
        public int ParentIndex { get; set; }
        public int Index { get; set; }
        public List<PlexMediaItem>? Media { get; set; }
    }

    private sealed class PlexMediaItem
    {
        public List<PlexPartItem>? Part { get; set; }
    }

    private sealed class PlexPartItem
    {
        public string? File { get; set; }
    }
}
