namespace PlexAutoplay.Application.DTOs;

public record MarkerDto
{
    public required string RatingKey { get; init; }
    public required string Kind { get; init; }
    public required long StartMs { get; init; }
    public required long EndMs { get; init; }
}

public record DetectMarkerRequest
{
    public required string FilePath { get; init; }
    public required string RatingKey { get; init; }
}

public record DetectMarkerResponse
{
    public required string RatingKey { get; init; }
    public required IEnumerable<MarkerDto> Markers { get; init; }
}
