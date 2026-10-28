using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace PlexAutoplay.Web.Configuration;

/// <summary>
/// Configura os listeners do Kestrel (HTTPS, WSS e HTTP opcional)
/// a partir das variáveis de ambiente HTTPS_PORT, WS_PORT, WEB_PORT e DISABLE_HTTP.
/// </summary>
public static class KestrelSetup
{
    public static void Configure(WebApplicationBuilder builder, X509Certificate2 serverCert, bool disableHttp)
    {
        var httpsPort = int.Parse(Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443");
        var wsPort = int.Parse(Environment.GetEnvironmentVariable("WS_PORT") ?? "8081");

        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTPS endpoint (configurable port, default 443)
            options.ListenAnyIP(httpsPort, listenOptions =>
            {
                listenOptions.UseHttps(serverCert);
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            });

            // WSS endpoint (WebSocket Secure on configurable port)
            options.ListenAnyIP(wsPort, listenOptions =>
            {
                listenOptions.UseHttps(serverCert);
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            });

            // Optional HTTP endpoint when DISABLE_HTTP=false (for cloud load balancers that terminate TLS)
            if (!disableHttp)
            {
                var httpPort = int.Parse(Environment.GetEnvironmentVariable("WEB_PORT") ?? "8080");
                options.ListenAnyIP(httpPort);
                Log.Information("Listener HTTP ativado na porta {HttpPort} (DISABLE_HTTP=false)", httpPort);
            }
            else
            {
                Log.Information("Listener HTTP desabilitado (DISABLE_HTTP=true). Apenas HTTPS e WSS estão disponíveis.");
            }
        });
    }
}
