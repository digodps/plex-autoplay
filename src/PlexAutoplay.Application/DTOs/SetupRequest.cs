namespace PlexAutoplay.Application.DTOs;

/// <summary>
/// Payload do setup inicial (POST /api/auth/setup).
/// </summary>
public record SetupRequest
{
    public string Token { get; init; } = "";
    public string NewPassword { get; init; } = "";
}
