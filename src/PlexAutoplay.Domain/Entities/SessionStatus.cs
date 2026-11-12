namespace PlexAutoplay.Domain.Entities;

/// <summary>Alvo de skip de uma sessão (próximo marcador de intro/créditos).</summary>
public sealed record MarkerTarget(
    string Kind,
    long StartMs,
    long EndMs,
    long MsUntilSkip);

/// <summary>Snapshot de uma sessão ativa no Plex.</summary>
public sealed record SessionStatus(
    string MachineIdentifier,
    string Product,
    string Platform,
    int RatingKey,
    long PositionMs,
    long DurationMs,
    string State,
    MarkerTarget? NextMarker);
