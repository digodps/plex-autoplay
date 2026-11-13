namespace PlexAutoplay.Domain.Ports;

using PlexAutoplay.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>Persistência de histórico de skips executados pelo auto-skip (servidor).</summary>
public interface ISkipHistoryRepository
{
    Task AddAsync(SkipHistoryEntry entry);
    Task<IEnumerable<SkipHistoryEntry>> GetRecentAsync(int limit);
}
