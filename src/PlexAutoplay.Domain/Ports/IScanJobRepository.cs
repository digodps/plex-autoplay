using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexAutoplay.Domain.Ports;

/// <summary>Porta para persistência e recuperação de jobs de varredura.</summary>
public interface IScanJobRepository
{
    /// <summary>Adiciona novo job.</summary>
    Task AddAsync(ScanJob job);
    /// <summary>Obtém job pelo ID.</summary>
    Task<ScanJob?> GetAsync(string jobId);
    /// <summary>Obtém job mais recente.</summary>
    Task<ScanJob?> GetLatestAsync();
    /// <summary>Lista histórico paginado de jobs.</summary>
    Task<List<ScanJob>> GetHistoryAsync(int page = 1, int pageSize = 100);
    /// <summary>Atualiza status e contadores do job.</summary>
    Task UpdateAsync(ScanJob job);
    /// <summary>Remove todos os jobs e seus logs.</summary>
    Task<int> ClearJobsAsync();
}

/// <summary>Porta para persistência e recuperação de linhas de log de varredura.</summary>
public interface IScanJobLogStore
{
    /// <summary>Adiciona linha ao log do job.</summary>
    Task AddLogAsync(string jobId, string level, string message, string? suggestion = null);
    /// <summary>Obtém logs filtrados e paginados.</summary>
    Task<List<ScanLogEntry>> GetLogsAsync(string? jobId, DateTime? from, DateTime? to, string? level, string? textFilter, int page = 1, int pageSize = 100);
    /// <summary>Remove todas as linhas de log.</summary>
    Task<int> ClearLogsAsync();
    /// <summary>Remove logs com timestamp anterior a N dias e jobs órfãos.</summary>
    Task<int> CleanupOldLogsAsync(int retentionDays = 30);
}