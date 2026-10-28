namespace PlexAutoplay.Domain.Ports;

/// <summary>
/// Porta para escrita/leitura do arquivo custom.json do PlexAutoSkip.
/// </summary>
public interface ICustomJsonWriter
{
    /// <summary>
    /// Atualiza ou cria marcadores para um RatingKey específico.
    /// </summary>
    void UpsertMarkers(string ratingKey, IEnumerable<MarkerEntry> markers);

    /// <summary>
    /// Remove um marcador específico.
    /// </summary>
    void RemoveMarker(string ratingKey, string kind);

    /// <summary>
    /// Obtém todos os marcadores persistidos.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<MarkerEntry>> GetAllMarkers();

    /// <summary>
    /// Entrada de marcador para o custom.json
    /// </summary>
    public readonly record struct MarkerEntry(string Kind, long StartMs, long EndMs);
}