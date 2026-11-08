namespace PlexAutoplay.Infrastructure.AutoSkip;

/// <summary>
/// Fornece a estratégia de seek para um cliente Plex a partir de seus atributos de sessão.
/// Abstração para permitir testes e futuras implementações.
/// </summary>
public interface IClientSeekStrategyProvider
{
    ClientSeekStrategy Detect(string product, string platform, string version);
}