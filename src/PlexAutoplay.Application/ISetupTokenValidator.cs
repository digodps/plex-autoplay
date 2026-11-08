namespace PlexAutoplay.Application;

/// <summary>
/// Porta para o token de setup de uso único emitido no primeiro start.
/// </summary>
public interface ISetupTokenValidator
{
    bool TryValidate(string token);
    void Invalidate();
}
