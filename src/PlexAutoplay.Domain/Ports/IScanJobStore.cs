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
