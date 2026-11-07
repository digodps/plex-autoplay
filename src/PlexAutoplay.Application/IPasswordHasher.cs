namespace PlexAutoplay.Application;

/// <summary>
/// Porta para hash e verificação de senhas do operador.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
