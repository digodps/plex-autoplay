namespace PlexAutoplay.Application;

/// <summary>
/// Porta para geração de token de autenticação.
/// </summary>
public interface ITokenService
{
    string GenerateToken();
}
