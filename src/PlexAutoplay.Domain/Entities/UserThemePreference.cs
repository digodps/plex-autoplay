namespace PlexAutoplay.Domain.Entities;

public record UserThemePreference
{
    public string UserId { get; set; } = "admin";
    public required string AccentColor { get; init; }
    public required string Mode { get; init; }
}
