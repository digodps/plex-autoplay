using System.Collections.Concurrent;

namespace PlexAutoplay.Web.Services;

/// <summary>
/// Representa um job de varredura de marcadores (em memória, v1).
/// </summary>
public class ScanJob
{
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "queued";
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Armazenamento em memória dos jobs de varredura (scan).
/// Compartilhado pelos endpoints de scan, status, cancelamento e monitoramento.
/// </summary>
public class ScanJobStore
{
    private readonly ConcurrentDictionary<string, ScanJob> _jobs = new();

    public void Add(ScanJob job) => _jobs[job.JobId] = job;

    public ScanJob? Get(string jobId) => _jobs.TryGetValue(jobId, out var job) ? job : null;

    public ScanJob? GetLatest() => _jobs.Values.OrderByDescending(j => j.UpdatedAt).FirstOrDefault();
}
