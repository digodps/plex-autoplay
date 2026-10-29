namespace PlexAutoplay.Application;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPlexApiClient
{
    Task<PlexLibrariesResult> GetLibrariesAsync();
    Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey);
    Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null);
    Task<PlexEpisodesResult> GetSeriesEpisodesAsync(string seriesKey);
}

/// <summary>
/// Resultado da consulta de bibliotecas do Plex, incluindo indicação de falha
/// e mensagem de erro para que o cliente possa sugerir uma ação ao usuário.
/// </summary>
public record PlexLibrariesResult(
    IReadOnlyList<PlexLibraryDto> Libraries,
    bool Failed = false,
    string? ErrorMessage = null)
{
    public static PlexLibrariesResult Empty => new(Array.Empty<PlexLibraryDto>());
    public static PlexLibrariesResult FailedResult(string errorMessage) => new(Array.Empty<PlexLibraryDto>(), true, errorMessage);
}

/// <summary>
/// Resultado da consulta de episódios do Plex, incluindo indicação de falha
/// e mensagem de erro para que o cliente possa sugerir uma ação ao usuário.
/// </summary>
public record PlexEpisodesResult(
    IReadOnlyList<PlexEpisodeDto> Episodes,
    bool Failed = false,
    string? ErrorMessage = null)
{
    public static PlexEpisodesResult Empty => new(Array.Empty<PlexEpisodeDto>());
    public static PlexEpisodesResult FailedResult(string errorMessage) => new(Array.Empty<PlexEpisodeDto>(), true, errorMessage);
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
