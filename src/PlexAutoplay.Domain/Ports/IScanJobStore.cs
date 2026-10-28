namespace PlexAutoplay.Domain.Ports;

/// <summary>
/// Porta para armazenamento e recuperação de jobs de varredura em memória.
/// </summary>
public interface IScanJobStore
{
    /// <summary>
    /// Adiciona um novo job ao armazenamento.
    /// </summary>
    void Add(ScanJob job);

    /// <summary>
    /// Obtém um job pelo ID.
    /// </summary>
    ScanJob? Get(string jobId);

    /// <summary>
    /// Obtém o job mais recente.
    /// </summary>
    ScanJob? GetLatest();
}

/// <summary>
/// Representa um job de varredura em execução ou concluído.
/// </summary>
public class ScanJob
{
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "queued"; // queued, running, completed, failed, cancelled
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UpdatedAt { get; set; }
}