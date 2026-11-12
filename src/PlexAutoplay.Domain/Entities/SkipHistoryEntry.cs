namespace PlexAutoplay.Domain.Entities;

/// <summary>Registro de um skip executado pelo background service.</summary>
public sealed record SkipHistoryEntry(
    DateTime Timestamp,
    string MachineIdentifier,
    string Product,
    string Platform,
    int RatingKey,
    string MarkerKind,
    long SkippedFromMs,
    long SkippedToMs);
