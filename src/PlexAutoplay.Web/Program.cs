using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.AutoSkip;
using PlexAutoplay.Infrastructure.Auth;
using PlexAutoplay.Infrastructure.Cache;
using PlexAutoplay.Infrastructure.PlexProxy;
using PlexAutoplay.Infrastructure.Sync;
using PlexAutoplay.Infrastructure.Telemetry;
using PlexAutoplay.Web.Middleware;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception:lj}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "plex-autoplay")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// --- Certificate configuration ---
var httpsPort = int.Parse(Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443");
var disableHttpRaw = Environment.GetEnvironmentVariable("DISABLE_HTTP") ?? "true";
var disableHttp = disableHttpRaw.Equals("true", StringComparison.OrdinalIgnoreCase);
var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD");
var certBase64 = Environment.GetEnvironmentVariable("CERT_BASE64");

X509Certificate2? serverCert = null;

// Priority: CERT_BASE64 > CERT_PATH > auto-generate self-signed
if (!string.IsNullOrEmpty(certBase64))
{
    try
    {
        var certBytes = Convert.FromBase64String(certBase64);
        serverCert = string.IsNullOrEmpty(certPassword)
            ? new X509Certificate2(certBytes)
            : new X509Certificate2(certBytes, certPassword);
        Log.Information("Certificado carregado de CERT_BASE64: {Subject} (expire: {NotAfter})", serverCert.Subject, serverCert.NotAfter);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "CERT_BASE64 inválido: não é um PFX válido ou não pôde ser decodificado");
        Environment.Exit(1);
    }
}
else if (!string.IsNullOrEmpty(certPath))
{
    if (!File.Exists(certPath))
    {
        Log.Fatal("CERT_PATH aponta para arquivo inexistente: {CertPath}", certPath);
        Environment.Exit(1);
    }
    try
    {
        serverCert = string.IsNullOrEmpty(certPassword)
            ? new X509Certificate2(certPath)
            : new X509Certificate2(certPath, certPassword);
        Log.Information("Certificado carregado de CERT_PATH: {Subject} (expire: {NotAfter})", serverCert.Subject, serverCert.NotAfter);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "CERT_PATH inválido: não é um PFX válido: {CertPath}", certPath);
        Environment.Exit(1);
    }
}

// Validate certificate not expired
if (serverCert != null && serverCert.NotAfter < DateTimeOffset.Now)
{
    Log.Fatal("Certificado fornecido está expirado: {NotAfter}", serverCert.NotAfter);
    Environment.Exit(1);
}

// Generate self-signed certificate when none provided
if (serverCert == null)
{
    Log.Information("Nenhum certificado configurado via CERT_PATH ou CERT_BASE64. Gerando certificado auto-assinado...");
    serverCert = GenerateSelfSignedCertificate();
    Log.Information("Certificado auto-assinado gerado: {Subject} (expire: {NotAfter})", serverCert.Subject, serverCert.NotAfter);
}

// Configure Kestrel: HTTPS-only (plus optional HTTP when DISABLE_HTTP=false)
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTPS endpoint (configurable port, default 443)
    options.ListenAnyIP(httpsPort, listenOptions =>
    {
        listenOptions.UseHttps(serverCert!);
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // WSS endpoint (WebSocket Secure on port 8081)
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps(serverCert!);
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

// Add services to the container.
builder.Services.AddOpenApi();

// Register OpenTelemetry telemetry
builder.Services.AddOpenTelemetryTelemetry(builder.Configuration);

// Register periodic sync background service
builder.Services.AddHostedService<SyncBackgroundService>();

// Register in-memory cache and the generic cache port (pluggable backend)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, ImageCacheService>();

// Register SQLite marker repository
var sqliteConnectionString = Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING") ?? "Data Source=/data/app.db";
builder.Services.AddSingleton<PlexAutoplay.Domain.Ports.IMarkerRepository>(
    new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteMarkerRepository(sqliteConnectionString));

// Register JwtTokenService from environment variable
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret-change-in-production";
builder.Services.AddSingleton(new JwtTokenService(jwtSecret));

// Admin password from environment
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin";

// Plex configuration
var plexUrl = Environment.GetEnvironmentVariable("PLEX_URL") ?? "http://localhost:32400";
var plexToken = Environment.GetEnvironmentVariable("PLEX_TOKEN") ?? "";

// Configure HttpClient for image proxy
builder.Services.AddSingleton<ImageProxyService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    httpClient.BaseAddress = new Uri(plexUrl);
    httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
    return new ImageProxyService(httpClient, plexToken, plexUrl, sp.GetRequiredService<ICacheService>());
});

// Configure HttpClient for Plex API calls
builder.Services.AddHttpClient<PlexApiClient>((sp, client) =>
{
    client.BaseAddress = new Uri(plexUrl);
    client.DefaultRequestHeaders.Add("X-Plex-Token", plexToken);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register PlexAutoSkip service
var customJsonPath = Environment.GetEnvironmentVariable("CUSTOM_JSON_PATH") ?? "/data/custom.json";
builder.Services.AddSingleton(new PlexAutoSkipService(customJsonPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapGet("/auth/login", () => Results.Ok(new { message = "Use POST to authenticate" }));

app.MapPost("/auth/login", (HttpContext context, [FromServices] JwtTokenService tokenService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = reader.ReadToEndAsync().Result;

    var password = ExtractPassword(json);

    if (password == adminPassword)
    {
        var token = tokenService.GenerateToken();
        return Results.Ok(new { token });
    }

    return Results.Unauthorized();
});

app.MapGet("/api/plex/libraries", async ([FromServices] PlexApiClient plexClient) =>
{
    var libraries = await plexClient.GetLibrariesAsync();
    var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();
    return Results.Ok(seriesLibraries);
});

// Scan job tracking (in-memory for v1)
var scanJobs = new ConcurrentDictionary<string, ScanJob>();

app.MapPost("/api/plex/scan", async ([FromServices] PlexApiClient plexClient, [FromServices] IMarkerRepository markerRepo, [FromServices] IMarkerDetector markerDetector) =>
{
    var jobId = Guid.NewGuid().ToString();
    var job = new ScanJob { JobId = jobId, Status = "queued" };
    scanJobs[jobId] = job;

    _ = Task.Run(async () =>
    {
        try
        {
            job.Status = "running";
            job.UpdatedAt = DateTime.UtcNow;

            var libraries = await plexClient.GetLibrariesAsync();
            var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();

            int processed = 0;
            int skipped = 0;
            int errors = 0;

            foreach (var lib in seriesLibraries)
            {
                if (job.Status == "cancelled") break;

                // In a full implementation, we'd list episodes from Plex and
                // detect markers for each media file via ffprobe.
                // For now, we simulate the scan flow.
                await Task.Delay(500);
                processed++;
            }

            job.Status = "completed";
            job.Processed = processed;
            job.Skipped = skipped;
            job.Errors = errors;
            job.UpdatedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            job.Status = "failed";
            job.ErrorMessage = ex.Message;
            job.UpdatedAt = DateTime.UtcNow;
        }
    });

    return Results.Ok(new { job_id = jobId });
});

app.MapGet("/api/sync/status/{jobId}", (string jobId) =>
{
    if (scanJobs.TryGetValue(jobId, out var job))
    {
        return Results.Ok(new
        {
            job_id = job.JobId,
            status = job.Status,
            processed = job.Processed,
            skipped = job.Skipped,
            errors = job.Errors,
            error = job.ErrorMessage
        });
    }
    return Results.NotFound();
});

app.MapGet("/api/sync/log/stream/{jobId}", async (string jobId, HttpContext context) =>
{
    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    while (!context.RequestAborted.IsCancellationRequested)
    {
        if (scanJobs.TryGetValue(jobId, out var job))
        {
            await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { status = job.Status, processed = job.Processed, skipped = job.Skipped })}\n\n");
            await context.Response.Body.FlushAsync();

            if (job.Status == "completed" || job.Status == "failed" || job.Status == "cancelled")
                break;
        }

        await Task.Delay(1000);
    }
});

app.MapPost("/api/sync/cancel/{jobId}", (string jobId) =>
{
    if (scanJobs.TryGetValue(jobId, out var job))
    {
        job.Status = "cancelled";
        job.UpdatedAt = DateTime.UtcNow;
        return Results.Ok(new { cancelled = true });
    }
    return Results.NotFound();
});

// Marker CRUD endpoints
app.MapGet("/api/markers", async ([FromServices] IMarkerRepository markerRepo) =>
{
    var markers = await markerRepo.GetAllAsync();
    return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs }));
});

app.MapGet("/api/markers/{ratingKey}", async (string ratingKey, [FromServices] IMarkerRepository markerRepo) =>
{
    var markers = await markerRepo.GetByRatingKeyAsync(ratingKey);
    return Results.Ok(markers.Select(m => new { m.RatingKey, m.Kind, m.StartMs, m.EndMs }));
});

app.MapPost("/api/markers", async ([FromBody] MarkerCreateRequest req, [FromServices] IMarkerRepository markerRepo) =>
{
    var exists = await markerRepo.ExistsAsync(req.RatingKey, req.Kind);
    if (exists)
    {
        return Results.Conflict(new { error = "Marker already exists" });
    }

    var marker = new PlexAutoplay.Domain.Entities.Marker
    {
        RatingKey = req.RatingKey,
        Kind = req.Kind,
        StartMs = req.StartMs,
        EndMs = req.EndMs
    };

    await markerRepo.AddAsync(marker);
    return Results.Created($"/api/markers/{req.RatingKey}", new { marker.RatingKey, marker.Kind, marker.StartMs, marker.EndMs });
});

app.MapDelete("/api/markers/{ratingKey}/{kind}", async (string ratingKey, string kind, [FromServices] IMarkerRepository markerRepo) =>
{
    await markerRepo.DeleteAsync(ratingKey, kind);
    return Results.Ok(new { deleted = true });
});

// Image proxy endpoint
app.MapGet("/api/images/{**imagePath}", async (string imagePath, [FromServices] ImageProxyService proxyService) =>
{
    try
    {
        var imageBytes = await proxyService.GetImageAsync("/" + imagePath);
        return Results.File(imageBytes, "image/jpeg");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 502);
    }
});

// WebSocket endpoint for PlexAutoSkip (port 8081)
app.Map("/ws", async (HttpContext context, [FromServices] PlexAutoSkipService autoSkipService) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[4096];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                break;
            }

            // Echo back a simple response - in full implementation, parse player
            // commands and send seek commands based on custom.json markers
            var response = JsonSerializer.Serialize(new { status = "ok", markers = autoSkipService.GetMarkersForEpisode("") });
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Serve Vue SPA static files
app.UseStaticFiles();

// Prometheus metrics endpoint
app.MapGet("/metrics", () => 
{
    var metrics = "# HELP plex_autoplay_info Plex Autoplay service info\n# TYPE plex_autoplay_info gauge\nplex_autoplay_info{version=\"1.0.0\",service=\"plex-autoplay\"} 1\n";
    return Results.Text(metrics, "text/plain; version=0.0.4; charset=utf-8");
});

// Observability summary endpoint
app.MapGet("/api/observability/summary", () => 
{
    var summary = new
    {
        service = "plex-autoplay",
        version = "1.0.0",
        telemetry = new
        {
            tracing_enabled = true,
            metrics_enabled = true,
            logging_enabled = true
        },
        metrics = new
        {
            http_requests_total = "counter",
            http_request_duration_ms = "histogram",
            plex_api_errors_total = "counter",
            plex_scans_total = "counter",
            plex_scan_duration_ms = "histogram"
        },
        endpoints = new[]
        {
            "/metrics",
            "/api/observability/summary"
        }
    };
    return Results.Ok(summary);
});

// SPA fallback: routes not matching API go to index.html
app.MapFallbackToFile("index.html");

app.Run();

static string? ExtractPassword(string json)
{
    var match = System.Text.RegularExpressions.Regex.Match(json, @"""password""\s*:\s*""([^""]*)""");
    return match.Success ? match.Groups[1].Value : null;
}

static X509Certificate2 GenerateSelfSignedCertificate()
{
    using var ecdsa = ECDsa.Create(ECCurve.CreateFromValue("1.2.840.10045.3.1.7")); // nistP256 / P-256
    var request = new CertificateRequest(new X500DistinguishedName("cn=plex-autoplay"), ecdsa, HashAlgorithmName.SHA256);
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // Server Authentication
    var sanBuilder = new SubjectAlternativeNameBuilder();
    sanBuilder.AddDnsName("localhost");
    sanBuilder.AddIpAddress(System.Net.IPAddress.Loopback);
    request.CertificateExtensions.Add(sanBuilder.Build());
    var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
    return new X509Certificate2(cert);
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class PlexLibrary
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public string Agent { get; set; } = "";
}

public class PlexApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly ActivitySource ActivitySource = PlexAutoplay.Infrastructure.Telemetry.OpenTelemetryService.Tracer;
    private static readonly Meter PlexMeter = new("PlexAutoplay.PlexApi");
    private static readonly Counter<int> PlexErrorCounter = PlexMeter.CreateCounter<int>("plex_api_errors_total");

    public PlexApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlexLibrary>> GetLibrariesAsync()
    {
        using var activity = ActivitySource.StartActivity("Plex.GetLibraries");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", "/library/sections");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync("/library/sections");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetLibraries", (int)response.StatusCode);
                return new List<PlexLibrary>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var libraries = new List<PlexLibrary>();
            var directory = doc.RootElement.GetProperty("Directory");
            
            foreach (var item in directory.EnumerateArray())
            {
                libraries.Add(new PlexLibrary
                {
                    Key = item.GetProperty("key").GetString() ?? "",
                    Title = item.GetProperty("title").GetString() ?? "",
                    Type = item.GetProperty("type").GetString() ?? "",
                    Agent = item.GetProperty("agent").GetString() ?? ""
                });
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetLibraries", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, count = libraries.Count });

            return libraries;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetLibraries", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return new List<PlexLibrary>();
        }
    }
}

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

public class MarkerCreateRequest
{
    public string RatingKey { get; set; } = "";
    public string Kind { get; set; } = "";
    public long StartMs { get; set; }
    public long EndMs { get; set; }
}
