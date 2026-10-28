namespace PlexAutoplay.UnitTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.AutoSkip;
using PlexAutoplay.Infrastructure.Persistence.Repositories;
using PlexAutoplay.Web.Endpoints;

/// <summary>
/// Host de teste em memória (TestServer) com os endpoints reais de marcadores,
/// repositório SQLite temporário e custom.json temporário.
/// </summary>
public class MarkerEndpointsFixture : IDisposable
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"markers-api-{Guid.NewGuid():N}.db");
    public string CustomJsonPath { get; } = Path.Combine(Path.GetTempPath(), $"custom-{Guid.NewGuid():N}.json");

    public HttpClient Client { get; }
    private readonly IHost _host;

    public MarkerEndpointsFixture()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton<IMarkerRepository>(
                        new SqliteMarkerRepository($"Data Source={DbPath}"));
                    services.AddSingleton(new CustomJsonWriterService(CustomJsonPath));
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapMarkerEndpoints());
                });
            })
            .Start();

        Client = _host.GetTestClient();
    }

    public void Dispose()
    {
        _host.Dispose();
        if (File.Exists(DbPath)) File.Delete(DbPath);
        if (File.Exists(CustomJsonPath)) File.Delete(CustomJsonPath);
    }
}
