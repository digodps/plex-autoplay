namespace PlexAutoplay.UnitTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlexAutoplay.Domain;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Web.Endpoints;

/// <summary>
/// Host de teste em memória (TestServer) com o endpoint real de status
/// (`GET /api/status`) e repositório de jobs em memória.
/// </summary>
public class StatusEndpointsFixture : IDisposable
{
    public HttpClient Client { get; }
    public IScanJobStore ScanJobs { get; }
    private readonly IHost _host;

    public StatusEndpointsFixture()
    {
        ScanJobs = new TestScanJobStore();

        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton<IScanJobStore>(ScanJobs);
                    services.AddSingleton<IProcessInfoService, ProcessInfoService>();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapMonitoringEndpoints());
                });
            })
            .Start();

        Client = _host.GetTestClient();
    }

    public void Dispose()
    {
        _host.Dispose();
    }

    private sealed class TestScanJobStore : IScanJobStore
    {
        private readonly Dictionary<string, ScanJob> _jobs = [];

        public void Add(ScanJob job) => _jobs[job.JobId] = job;

        public ScanJob? Get(string jobId) => _jobs.GetValueOrDefault(jobId);

        public ScanJob? GetLatest() => _jobs.Values.MaxBy(job => job.UpdatedAt);

        public void AddLog(string jobId, string level, string message, string? suggestion = null)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                job.Logs.Add(new ScanLogEntry(DateTime.UtcNow, message, level, suggestion));
            }
        }
    }
}
