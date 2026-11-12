using System.Collections.Concurrent;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Infrastructure;

/// <summary>
/// Monitor singleton thread-safe para sessões ativas e histórico de skips.
/// Mantém últimos 50 skips em anel (memória, perdido no restart — upgrade path: tabela SQLite).
/// </summary>
public sealed class SkipMonitor : ISkipMonitor
{
    private const int MaxRecentSkips = 50;
    private readonly object _lock = new();
    private List<SessionStatus> _sessions = new();
    private readonly Queue<SkipHistoryEntry> _skips = new();

    public IReadOnlyList<SessionStatus> Sessions
    {
        get
        {
            lock (_lock)
            {
                return _sessions.ToList().AsReadOnly();
            }
        }
    }

    public IReadOnlyList<SkipHistoryEntry> RecentSkips
    {
        get
        {
            lock (_lock)
            {
                return _skips.Reverse().ToList().AsReadOnly();
            }
        }
    }

    public void UpdateSessions(IReadOnlyList<SessionStatus> sessions)
    {
        lock (_lock)
        {
            _sessions = sessions.ToList();
        }
    }

    public void RecordSkip(SkipHistoryEntry entry)
    {
        lock (_lock)
        {
            _skips.Enqueue(entry);
            while (_skips.Count > MaxRecentSkips)
                _skips.Dequeue();
        }
    }
}