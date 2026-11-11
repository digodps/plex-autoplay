using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Infrastructure.Persistence;
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

<<<<<<< HEAD
=======
// Admin password for auth endpoints
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin";
Log.Information("Admin password set.");

// Media root for resolving local media file paths
var mediaRoot = Environment.GetEnvironmentVariable("MEDIA_ROOT") ?? "/media";
Log.Information("Media root: {MediaRoot}", mediaRoot);

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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

// app.UseHttpsRedirection();  // Disabled for e2e tests - use HTTP only to avoid IPv6 TLS handshake failures

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

// Correlation ID middleware (before auth so all downstream middleware has the ID)
app.UseMiddleware<CorrelationIdMiddleware>();

// JWT Authentication middleware (before routing)
app.UseMiddleware<JwtAuthMiddleware>();

// --- Endpoints ---
<<<<<<< HEAD
app.MapAuthEndpoints(disableHttp);
app.MapPlexEndpoints();
app.MapSeriesEndpoints();
=======
app.MapAuthEndpoints(adminPassword);
app.MapPlexEndpoints(mediaRoot);
app.MapSeriesEndpoints(mediaRoot);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
app.MapSyncEndpoints();
app.MapMarkerEndpoints();
app.MapImageEndpoints();
app.MapAutoSkipWebSocketEndpoints();
app.MapMonitoringEndpoints();
<<<<<<< HEAD
app.MapSettingsEndpoints();
app.MapFileSystemEndpoints();
app.MapPlexIdentityEndpoint();
=======
app.MapConfigEndpoints();
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
app.MapThemeEndpoints();

// Serve Vue SPA static files
app.UseStaticFiles();

// SPA fallback: routes not matching API go to index.html
app.MapFallbackToFile("index.html");

// --- Database schema initialization (composition root, before serving) ---
Log.Information("Inicializando schema do banco...");
await DatabaseInitializer.InitializeAsync(DatabaseInitializer.DefaultConnectionString);
await PlexAutoplay.Infrastructure.Persistence.Repositories.AppSettingsSeeder.SeedAsync(DatabaseInitializer.DefaultConnectionString);
Log.Information("Schema do banco inicializado e configurações semeadas.");

// Setup inicial: sem credencial no SQLite, emite um token de uso único no log.
var credentials = app.Services.GetRequiredService<PlexAutoplay.Application.IAuthCredentialStore>();
if (!await credentials.IsConfiguredAsync())
{
    var setupToken = app.Services.GetRequiredService<PlexAutoplay.Infrastructure.Auth.SetupTokenService>().EnsureGenerated();
    Console.Out.WriteLine("============================================================");
    Console.Out.WriteLine("CONFIGURACAO INICIAL - TOKEN DE SETUP (USO UNICO)");
    Console.Out.WriteLine($"SETUP_TOKEN={setupToken}");
    Console.Out.WriteLine("Informe este token em /setup para criar a senha do operador.");
    Console.Out.WriteLine("O token deixa de valer apos a primeira conclusao do setup.");
    Console.Out.WriteLine("============================================================");
    Console.Out.Flush();
}

app.Run();
