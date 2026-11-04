namespace PlexAutoplay.Application.DTOs;

/// <summary>
/// Payload de criação/atualização de marcador, aceito tanto via corpo JSON quanto via query string.
/// </summary>
public record SaveMarkerRequest
{
    public string RatingKey { get; init; } = "";
    public string Kind { get; init; } = "";
    public long? StartMs { get; init; }
    public long? EndMs { get; init; }
}
