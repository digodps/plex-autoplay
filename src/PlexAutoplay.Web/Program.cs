using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Web.Configuration;
using PlexAutoplay.Web.Endpoints;
using PlexAutoplay.Web.Middleware;
using Serilog;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// --- Logging bootstrap ---
var logLevelRaw = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";
var logLevel = logLevelRaw switch
{
    "Debug" => Serilog.Events.LogEventLevel.Debug,
    "Verbose" => Serilog.Events.LogEventLevel.Verbose,
    "Warning" => Serilog.Events.LogEventLevel.Warning,
    "Warn" => Serilog.Events.LogEventLevel.Warning,
    "Error" => Serilog.Events.LogEventLevel.Error,
    "Fatal" => Serilog.Events.LogEventLevel.Fatal,
    _ => Serilog.Events.LogEventLevel.Information
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(logLevel))
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception:lj}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "plex-autoplay")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// --- Certificate & Kestrel ---
Log.Information("Carregando certificado...");
var disableHttp = (Environment.GetEnvironmentVariable("DISABLE_HTTP") ?? "true").Equals("true", StringComparison.OrdinalIgnoreCase);
var serverCert = ServerCertificateProvider.GetServerCertificate();
Log.Information("Certificado carregado, configurando Kestrel...");
KestrelSetup.Configure(builder, serverCert, disableHttp);
Log.Information("Kestrel configurado.");

// --- Services (DI) ---
Log.Information("Adicionando serviços de aplicação...");
builder.AddApplicationServices();
Log.Information("Serviços de aplicação adicionados.");

// Admin password for auth endpoints
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin";
Log.Information("Admin password set.");

// Media root for resolving local media file paths
var mediaRoot = Environment.GetEnvironmentVariable("MEDIA_ROOT") ?? "/media";
Log.Information("Media root: {MediaRoot}", mediaRoot);

Log.Information("Chamando builder.Build()...");
WebApplication app;
try
{
    app = builder.Build();
    Log.Information("Aplicação construída.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro ao construir a aplicação!");
    throw;
}

// --- HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

// Correlation ID middleware (before auth so all downstream middleware has the ID)
app.UseMiddleware<CorrelationIdMiddleware>();

// JWT Authentication middleware (before routing)
app.UseMiddleware<JwtAuthMiddleware>();

// --- Endpoints ---
app.MapAuthEndpoints(adminPassword);
app.MapPlexEndpoints(mediaRoot);
app.MapSeriesEndpoints(mediaRoot);
app.MapSyncEndpoints();
app.MapMarkerEndpoints();
app.MapImageEndpoints();
app.MapAutoSkipWebSocketEndpoints();
app.MapMonitoringEndpoints();
app.MapConfigEndpoints();

// Serve Vue SPA static files
app.UseStaticFiles();

// SPA fallback: routes not matching API go to index.html
app.MapFallbackToFile("index.html");

app.Run();
