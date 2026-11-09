namespace PlexAutoplay.Infrastructure.AutoSkip;

/// <summary>
/// Categoria de cliente Plex baseada em capacidade de seek.
/// </summary>
public enum ClientSeekCategory
{
    /// <summary>Clientes com "Advertise as Player" ativo (Android TV/Mobile, Apple TV, Roku)</summary>
    LegacyCompanion,

    /// <summary>Plex Web (Chrome, Edge, Firefox, Safari) versão >= 4.83.2</summary>
    ModernWeb,

    /// <summary>Plex Desktop (Windows, Mac, Linux) versão >= 1.46.1</summary>
    ModernDesktop,

    /// <summary>Smart TVs (Samsung, LG, Vizio, Hisense, Tizen, webOS)</summary>
    SmartTV,

    /// <summary>Cliente desconhecido — best effort via PMS proxy</summary>
    Unknown
}

/// <summary>
/// Estratégia de seek para um cliente específico.
/// </summary>
public record ClientSeekStrategy(
    ClientSeekCategory Category,
    bool SupportsDirectConnection,
    int DirectPort,
    bool SupportsPmsProxy,
    bool ExpectsNonXmlResponse
);