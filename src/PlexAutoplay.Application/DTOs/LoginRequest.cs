namespace PlexAutoplay.Application.DTOs;

/// <summary>
/// Payload de login (POST /auth/login).
/// </summary>
public record LoginRequest
{
    public string Password { get; init; } = "";
}
