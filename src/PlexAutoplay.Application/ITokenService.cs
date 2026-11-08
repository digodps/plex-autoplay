namespace PlexAutoplay.Application;

/// <summary>
/// Porta para geração e validação de token de autenticação.
/// </summary>
public interface ITokenService
{
    string GenerateToken();
    bool ValidateToken(string token);
}
