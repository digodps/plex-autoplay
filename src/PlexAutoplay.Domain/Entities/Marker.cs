namespace PlexAutoplay.Domain.Entities;

public record Marker
{
    public required string RatingKey { get; init; }
    public required string Kind { get; init; } // "intro" or "credits"
    public required long StartMs { get; init; }
    public required long EndMs { get; init; }

    /// <summary>
    /// Origem do marcador: "manual" (criado via UI/API) ou "detected" (scanner ffprobe).
    /// </summary>
    public string Origin { get; init; } = "manual";
}
