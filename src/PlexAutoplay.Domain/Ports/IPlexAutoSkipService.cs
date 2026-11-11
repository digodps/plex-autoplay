namespace PlexAutoplay.Domain.Ports;

/// <summary>
/// Porta para o serviço PlexAutoSkip (leitura de marcadores para WebSocket).
/// </summary>
public interface IPlexAutoSkipService
{
    /// <summary>
    /// Obtém marcadores para um episódio específico.
    /// </summary>
<<<<<<< HEAD
    Task<IReadOnlyList<MarkerInfo>> GetMarkersForEpisodeAsync(string ratingKey);
=======
    IReadOnlyList<MarkerInfo> GetMarkersForEpisode(string ratingKey);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
}

/// <summary>
/// Informação de marcador para o PlexAutoSkip.
/// </summary>
<<<<<<< HEAD
public readonly record struct MarkerInfo(string Kind, long StartMs, long EndMs);
=======
public readonly record struct MarkerInfo(string Kind, long StartMs, long EndMs);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
