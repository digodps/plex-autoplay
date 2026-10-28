namespace PlexAutoplay.Application;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPlexApiClient
{
    Task<List<PlexLibraryDto>> GetLibrariesAsync();
    Task<List<PlexEpisodeDto>> GetEpisodesAsync(string sectionKey);
    Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null);
    Task<List<PlexEpisodeDto>> GetSeriesEpisodesAsync(string seriesKey);
}

public record PlexLibraryDto(string Key, string Title, string Type, string Agent);
public record PlexEpisodeDto(string RatingKey, string Title, int ParentIndex, int Index, string FilePath);

/// <summary>
/// Página do catálogo de séries com o total sincronizado do Plex para paginação.
/// </summary>
public record PlexSeriesPage(
    IReadOnlyList<PlexSeriesDto> Items,
    int TotalSize,
    bool Failed = false)
{
    public static readonly PlexSeriesPage Empty = new([], 0);
}

/// <summary>
/// Série do catálogo (thumbnail cru; o endpoint reescreve para o proxy de imagens).
/// </summary>
public record PlexSeriesDto(string RatingKey, string Title, string Summary, string Thumb);
