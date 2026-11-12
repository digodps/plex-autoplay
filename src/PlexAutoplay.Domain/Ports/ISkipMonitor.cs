namespace PlexAutoplay.Domain.Ports;

using PlexAutoplay.Domain.Entities;
using System.Collections.Generic;

/// <summary>Porta para monitoramento em tempo real de sessões ativas e histórico de skips.</summary>
public interface ISkipMonitor
{
    IReadOnlyList<SessionStatus> Sessions { get; }
    IReadOnlyList<SkipHistoryEntry> RecentSkips { get; }
    void UpdateSessions(IReadOnlyList<SessionStatus> sessions);
    void RecordSkip(SkipHistoryEntry entry);
}
