namespace PlexAutoplay.Application;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPlexApiClient
{
    Task<List<PlexLibraryDto>> GetLibrariesAsync();
    Task<List<PlexEpisodeDto>> GetEpisodesAsync(string sectionKey);
}

public record PlexLibraryDto(string Key, string Title, string Type, string Agent);
public record PlexEpisodeDto(string RatingKey, string Title, int ParentIndex, int Index, string FilePath);
