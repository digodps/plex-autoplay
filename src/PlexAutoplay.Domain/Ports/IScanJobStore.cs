namespace PlexAutoplay.Domain.Ports;

/// <summary>Porta para armazenamento e recuperação de jobs de varredura em memória.</summary>
public interface IScanJobStore
{
    /// <summary>Adiciona novo job.</summary>
    void Add(ScanJob job);
    /// <summary>Obtém job pelo ID.</summary>
    ScanJob? Get(string jobId);
    /// <summary>Obtém job mais recente.</summary>
    ScanJob? GetLatest();
    /// <summary>Adiciona linha ao log do job.</summary>
    void AddLog(string jobId, string level, string message, string? suggestion = null);
}

/// <summary>Linha de log de varredura.</summary>
public sealed record ScanLogEntry(DateTime Timestamp, string Message, string Level, string? Suggestion);

/// <summary>Representa job de varredura.</summary>
public class ScanJob
{
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "queued";
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ScanLogEntry> Logs { get; } = [];
}
