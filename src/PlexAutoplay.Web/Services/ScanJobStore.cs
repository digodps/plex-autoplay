using PlexAutoplay.Domain.Ports;
using System.Collections.Concurrent;

namespace PlexAutoplay.Web.Services;

/// <summary>
/// Implementação em memória de <see cref="IScanJobStore"/>.
/// </summary>
public class ScanJobStore : IScanJobStore
{
    private readonly ConcurrentDictionary<string, ScanJob> _jobs = new();

    public void Add(ScanJob job) => _jobs[job.JobId] = job;

    public ScanJob? Get(string jobId) => _jobs.TryGetValue(jobId, out var job) ? job : null;

    public ScanJob? GetLatest() => _jobs.Values.OrderByDescending(j => j.UpdatedAt).FirstOrDefault();
}