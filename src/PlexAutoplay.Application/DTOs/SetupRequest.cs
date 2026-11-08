namespace PlexAutoplay.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Payload do setup inicial (POST /api/auth/setup).
/// </summary>
public record SetupRequest
{
    [JsonPropertyName("token")]
    public string Token { get; init; } = "";

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; init; } = "";

    [JsonPropertyName("plexUrl")]
    public string? PlexUrl { get; init; }

    [JsonPropertyName("plexToken")]
    public string? PlexToken { get; init; }

    [JsonPropertyName("mediaRoots")]
    public IReadOnlyList<string>? MediaRoots { get; init; }
}
