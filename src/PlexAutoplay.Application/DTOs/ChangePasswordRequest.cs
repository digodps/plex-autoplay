namespace PlexAutoplay.Application.DTOs;

/// <summary>
/// Payload de troca de senha (PUT /api/auth/password).
/// </summary>
public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = "";
    public string NewPassword { get; init; } = "";
}
