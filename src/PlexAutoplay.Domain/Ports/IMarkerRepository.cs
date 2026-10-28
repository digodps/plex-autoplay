namespace PlexAutoplay.Domain.Ports;

using PlexAutoplay.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMarkerRepository
{
    Task<IEnumerable<Marker>> GetAllAsync();
    Task<IEnumerable<Marker>> GetByRatingKeyAsync(string ratingKey);
    Task AddAsync(Marker marker);
    Task UpdateAsync(Marker marker);
    Task DeleteAsync(string ratingKey, string kind);
    Task<bool> ExistsAsync(string ratingKey, string kind);
}
