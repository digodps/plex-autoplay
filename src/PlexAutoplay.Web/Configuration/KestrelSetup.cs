using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace PlexAutoplay.Web.Configuration;

/// <summary>
/// Configura os listeners do Kestrel (HTTPS, WSS e HTTP opcional)
/// a partir das variáveis de ambiente HTTPS_PORT, WS_PORT, WEB_PORT e DISABLE_HTTP.
/// Quando DISABLE_HTTP=false, apenas HTTP é ouvido (sem HTTPS) para evitar handshakes IPv6 TLS.
/// </summary>
public static class KestrelSetup
{
    public static void Configure(WebApplicationBuilder builder, X509Certificate2 serverCert, bool disableHttp)
    {
        var wsPort = int.Parse(Environment.GetEnvironmentVariable("WS_PORT") ?? "8081");

        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTP endpoint when DISABLE_HTTP=false (for e2e tests avoiding IPv6 TLS handshake failures)
            if (!disableHttp)
            {
                var httpPort = int.Parse(Environment.GetEnvironmentVariable("WEB_PORT") ?? "80");
                options.ListenAnyIP(httpPort);
                Log.Information("Listener HTTP ativado na porta {HttpPort} (DISABLE_HTTP=false)", httpPort);
            }
            else
            {
                Log.Information("Listener HTTP desabilitado (DISABLE_HTTP=true). Apenas HTTPS e WSS estão disponíveis.");
            }

            // WSS endpoint (WebSocket Secure on configurable port) - only when HTTPS is enabled
            if (!disableHttp)
            {
                options.ListenAnyIP(wsPort, listenOptions =>
                {
                    listenOptions.UseHttps(serverCert);
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
            }
        });
    }
}
