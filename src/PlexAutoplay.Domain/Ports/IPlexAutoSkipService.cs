namespace PlexAutoplay.Domain.Ports;

/// <summary>
/// Porta para o serviço PlexAutoSkip (leitura de marcadores para WebSocket).
/// </summary>
public interface IPlexAutoSkipService
{
    /// <summary>
    /// Obtém marcadores para um episódio específico.
    /// </summary>
    Task<IReadOnlyList<MarkerInfo>> GetMarkersForEpisodeAsync(string ratingKey);
}

/// <summary>
/// Informação de marcador para o PlexAutoSkip.
/// </summary>
public readonly record struct MarkerInfo(string Kind, long StartMs, long EndMs);
