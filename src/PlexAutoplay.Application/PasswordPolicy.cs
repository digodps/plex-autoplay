namespace PlexAutoplay.Application;

/// <summary>
/// Regra única de tamanho de senha do operador.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static bool IsValid(string? password) =>
        password is not null && password.Length >= MinLength && password.Length <= MaxLength;
}
