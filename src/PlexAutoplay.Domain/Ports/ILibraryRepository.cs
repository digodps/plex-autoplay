using PlexAutoplay.Domain.Entities;

namespace PlexAutoplay.Domain.Ports;

/// <summary>Acesso ao catálogo de bibliotecas.</summary>
public interface ILibraryRepository
{
    Task<IEnumerable<Library>> GetAllAsync();
    Task<Library?> GetByRatingKeyAsync(string ratingKey);
    Task UpsertAsync(Library library);
    Task DeleteAsync(string ratingKey);
}
