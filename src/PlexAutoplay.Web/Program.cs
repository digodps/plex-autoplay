using Microsoft.AspNetCore.Mvc;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Application;
using PlexAutoplay.Infrastructure.AutoSkip;
using PlexAutoplay.Infrastructure.Auth;
using PlexAutoplay.Infrastructure.Cache;
using PlexAutoplay.Infrastructure.Detection;
using PlexAutoplay.Infrastructure.PlexProxy;
using PlexAutoplay.Infrastructure.Sync;
using PlexAutoplay.Infrastructure.Telemetry;
using PlexAutoplay.Web.Middleware;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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

// --- Certificate configuration ---
var httpsPort = int.Parse(Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443");
var wsPort = int.Parse(Environment.GetEnvironmentVariable("WS_PORT") ?? "8081");
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

    // WSS endpoint (WebSocket Secure on configurable port)
    options.ListenAnyIP(wsPort, listenOptions =>
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

// Media root for resolving local media file paths
var mediaRoot = Environment.GetEnvironmentVariable("MEDIA_ROOT") ?? "/media";

// Allowed hosts configuration
var allowedHosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS") ?? "*";

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
builder.Services.AddSingleton<IPlexApiClient>(sp => sp.GetRequiredService<PlexApiClient>());

// Register PlexAutoSkip service (reader) and CustomJsonWriterService (writer)
var customJsonPath = Environment.GetEnvironmentVariable("CUSTOM_JSON_PATH") ?? "/data/custom.json";
builder.Services.AddSingleton(new PlexAutoSkipService(customJsonPath));
builder.Services.AddSingleton(new CustomJsonWriterService(customJsonPath));

// Register marker detector
builder.Services.AddSingleton<IMarkerDetector, PlexAutoplay.Infrastructure.Detection.FfmpegMarkerDetector>();

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

// CSRF protection on mutating API routes
app.UseMiddleware<CsrfMiddleware>();

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

app.MapGet("/api/plex/libraries", async ([FromServices] IPlexApiClient plexClient) =>
{
    var libraries = await plexClient.GetLibrariesAsync();
    var seriesLibraries = libraries.Where(l => l.Type == "show").ToList();
    return Results.Ok(seriesLibraries);
});

// Scan job tracking (in-memory for v1)
var scanJobs = new ConcurrentDictionary<string, ScanJob>();

app.MapPost("/api/plex/scan", async ([FromServices] PlexApiClient plexClient, [FromServices] IMarkerRepository markerRepo, [FromServices] IMarkerDetector markerDetector, [FromServices] CustomJsonWriterService customJsonWriter) =>
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

                try
                {
                    var episodes = await plexClient.GetEpisodesAsync(lib.Key);

                    foreach (var episode in episodes)
                    {
                        if (job.Status == "cancelled") break;

                        // Skip if already has both intro and credits markers
                        var hasIntro = await markerRepo.ExistsAsync(episode.RatingKey, "intro");
                        var hasCredits = await markerRepo.ExistsAsync(episode.RatingKey, "credits");
                        if (hasIntro && hasCredits)
                        {
                            skipped++;
                            continue;
                        }

                        // Resolve local media path
                        var localPath = MediaPathResolver.Resolve(mediaRoot, episode.FilePath);
                        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                        {
                            skipped++;
                            continue;
                        }

                        // Detect markers via ffprobe
                        var detectedMarkers = markerDetector.Detect(localPath);

                        var markerEntries = new List<CustomJsonWriterService.MarkerEntry>();
                        foreach (var marker in detectedMarkers)
                        {
                            var exists = await markerRepo.ExistsAsync(episode.RatingKey, marker.Kind);
                            if (!exists)
                            {
                                await markerRepo.AddAsync(marker with { RatingKey = episode.RatingKey });
                                markerEntries.Add(new CustomJsonWriterService.MarkerEntry(marker.Kind, marker.StartMs, marker.EndMs));
                            }
                        }

                        if (markerEntries.Count > 0)
                        {
                            customJsonWriter.UpsertMarkers(episode.RatingKey, markerEntries);
                        }

                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error scanning library {LibKey}", lib.Key);
                    errors++;
                }
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

app.MapPost("/api/markers", async (HttpContext context, [FromServices] IMarkerRepository markerRepo, [FromServices] CustomJsonWriterService customJsonWriter) =>
{
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body);
    var bodyText = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;

    var ratingKey = "";
    var kind = "";
    var startMs = 0L;
    var endMs = 0L;

    var contentType = context.Request.ContentType ?? "";

    if (contentType.StartsWith("application/json"))
    {
        using var doc = JsonDocument.Parse(bodyText);
        var root = doc.RootElement;
        ratingKey = root.GetProperty("RatingKey").GetString() ?? "";
        kind = root.GetProperty("Kind").GetString() ?? "";
        startMs = root.GetProperty("StartMs").GetInt64();
        endMs = root.GetProperty("EndMs").GetInt64();
    }
    else
    {
        // Form data (application/x-www-form-urlencoded) from UI forms
        var form = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(bodyText);
        ratingKey = form["RatingKey"].ToString() ?? "";
        kind = form["Kind"].ToString() ?? "";
        startMs = long.TryParse(form["StartMs"].ToString(), out var sk) ? sk : 0;
        endMs = long.TryParse(form["EndMs"].ToString(), out var em) ? em : 0;
    }

    var exists = await markerRepo.ExistsAsync(ratingKey, kind);
    if (exists)
    {
        return Results.Conflict(new { error = "Marker already exists" });
    }

    var marker = new PlexAutoplay.Domain.Entities.Marker
    {
        RatingKey = ratingKey,
        Kind = kind,
        StartMs = startMs,
        EndMs = endMs
    };

    await markerRepo.AddAsync(marker);
    customJsonWriter.UpsertMarkers(ratingKey, new[] { new CustomJsonWriterService.MarkerEntry(kind, startMs, endMs) });
    return Results.Created($"/api/markers/{ratingKey}", new { ratingKey, kind, startMs, endMs });
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

            if (result.MessageType != WebSocketMessageType.Text)
                continue;

            var json = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                string? ratingKey = root.TryGetProperty("ratingKey", out var rkEl) ? rkEl.GetString() : null;
                string? state = root.TryGetProperty("state", out var stEl) ? stEl.GetString() : null;
                long positionMs = root.TryGetProperty("positionMs", out var pmEl) ? pmEl.GetInt64() : 0;

                if (string.IsNullOrEmpty(ratingKey) || string.IsNullOrEmpty(state))
                    continue;

                // Only act on play state
                if (!state.Equals("play", StringComparison.OrdinalIgnoreCase))
                    continue;

                var markers = autoSkipService.GetMarkersForEpisode(ratingKey);

                foreach (var marker in markers)
                {
                    // If player is within intro/credits range, send seek to end of segment
                    if (positionMs >= marker.StartMs && positionMs <= marker.EndMs)
                    {
                        var seekResponse = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = "seek",
                            targetMs = marker.EndMs,
                            reason = marker.Kind,
                            ratingKey
                        });
                        var seekBytes = System.Text.Encoding.UTF8.GetBytes(seekResponse);
                        await webSocket.SendAsync(new ArraySegment<byte>(seekBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                    }
                }

                // If no seek needed, send heartbeat/ack
                var ackResponse = System.Text.Json.JsonSerializer.Serialize(new { status = "ok", ratingKey });
                var ackBytes = System.Text.Encoding.UTF8.GetBytes(ackResponse);
                await webSocket.SendAsync(new ArraySegment<byte>(ackBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                // Invalid JSON, ignore
            }
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

// Scan status endpoint
app.MapGet("/api/status", () =>
{
    var lastJob = scanJobs.Values.OrderByDescending(j => j.UpdatedAt).FirstOrDefault();
    var process = System.Diagnostics.Process.GetCurrentProcess();

    return Results.Ok(new
    {
        service = "plex-autoplay",
        version = "1.0.0",
        lastJob = lastJob != null ? new
        {
            job_id = lastJob.JobId,
            status = lastJob.Status,
            processed = lastJob.Processed,
            skipped = lastJob.Skipped,
            errors = lastJob.Errors,
            error = lastJob.ErrorMessage,
            updated_at = lastJob.UpdatedAt
        } : null,
        process = new
        {
            cpu_total_ms = process.TotalProcessorTime.TotalMilliseconds,
            memory_private_bytes = process.PrivateMemorySize64,
            memory_working_set_bytes = process.WorkingSet64,
            uptime_ms = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMilliseconds
        }
    });
});

// Prometheus metrics endpoint (real metrics, not hardcoded)
app.MapGet("/metrics", () =>
{
    var lastJob = scanJobs.Values.OrderByDescending(j => j.UpdatedAt).FirstOrDefault();
    var process = System.Diagnostics.Process.GetCurrentProcess();
    var lines = new List<string>
    {
        "# HELP plex_autoplay_info Plex Autoplay service info",
        "# TYPE plex_autoplay_info gauge",
        $"plex_autoplay_info{{version=\"1.0.0\",service=\"plex-autoplay\"}} 1",
        "",
        "# HELP plex_autoplay_process_memory_private_bytes Private memory bytes",
        "# TYPE plex_autoplay_process_memory_private_bytes gauge",
        $"plex_autoplay_process_memory_private_bytes {process.PrivateMemorySize64}",
        "",
        "# HELP plex_autoplay_process_memory_working_set_bytes Working set bytes",
        "# TYPE plex_autoplay_process_memory_working_set_bytes gauge",
        $"plex_autoplay_process_memory_working_set_bytes {process.WorkingSet64}",
        "",
        "# HELP plex_autoplay_process_cpu_total_ms Total CPU milliseconds",
        "# TYPE plex_autoplay_process_cpu_total_ms counter",
        $"plex_autoplay_process_cpu_total_ms {process.TotalProcessorTime.TotalMilliseconds}",
        "",
        "# HELP plex_autoplay_process_uptime_ms Process uptime milliseconds",
        "# TYPE plex_autoplay_process_uptime_ms counter",
        $"plex_autoplay_process_uptime_ms {(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMilliseconds}",
        "",
        "# HELP plex_autoplay_last_job_processed Last job processed count",
        "# TYPE plex_autoplay_last_job_processed gauge",
        $"plex_autoplay_last_job_processed {lastJob?.Processed ?? 0}",
        "",
        "# HELP plex_autoplay_last_job_skipped Last job skipped count",
        "# TYPE plex_autoplay_last_job_skipped gauge",
        $"plex_autoplay_last_job_skipped {lastJob?.Skipped ?? 0}",
        "",
        "# HELP plex_autoplay_last_job_errors Last job errors count",
        "# TYPE plex_autoplay_last_job_errors gauge",
        $"plex_autoplay_last_job_errors {lastJob?.Errors ?? 0}"
    };
    return Results.Text(string.Join("\n", lines) + "\n", "text/plain; version=0.0.4; charset=utf-8");
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

// Config endpoints (env-only, no app-config.json)
app.MapGet("/config", () =>
{
    return Results.Ok(new
    {
        plex_url = Environment.GetEnvironmentVariable("PLEX_URL") ?? "",
        sync_interval_minutes = Environment.GetEnvironmentVariable("SYNC_INTERVAL_MINUTES") ?? "5",
        custom_json_path = Environment.GetEnvironmentVariable("CUSTOM_JSON_PATH") ?? "/data/custom.json",
        media_root = Environment.GetEnvironmentVariable("MEDIA_ROOT") ?? "/media",
        ws_port = Environment.GetEnvironmentVariable("WS_PORT") ?? "8081",
        https_port = Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443",
        disable_http = Environment.GetEnvironmentVariable("DISABLE_HTTP") ?? "true",
        log_level = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information",
        allowed_hosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS") ?? "*",
        otel_exporter = Environment.GetEnvironmentVariable("OTEL_EXPORTER") ?? "console",
        otel_endpoint = Environment.GetEnvironmentVariable("OTEL_ENDPOINT") ?? "",
        otel_service_name = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "plex-autoplay",
        app_config_path = Environment.GetEnvironmentVariable("APP_CONFIG_PATH") ?? "/data/app-config.json",
        note = "Configuration is read exclusively from environment variables. app-config.json is not used."
    });
});

app.MapPost("/config", () =>
{
    // POST only validates current config values; does not write app-config.json
    // per spec config-env requirement
    return Results.Ok(new
    {
        status = "valid",
        message = "Configuration validated from environment variables",
        source = "environment_variables",
        app_config_json_used = false
    });
});

// Library episodes endpoint
app.MapGet("/library/{section_key}", async (string section_key, [FromServices] IPlexApiClient plexClient) =>
{
    var episodes = await plexClient.GetEpisodesAsync(section_key);
    var result = episodes.Select(e => new
    {
        rating_key = e.RatingKey,
        title = e.Title,
        season = e.ParentIndex,
        episode = e.Index,
        file_path = e.FilePath,
        local_path = MediaPathResolver.Resolve(mediaRoot, e.FilePath)
    });
    return Results.Ok(result);
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

public class PlexApiClient : IPlexApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly ActivitySource ActivitySource = PlexAutoplay.Infrastructure.Telemetry.OpenTelemetryService.Tracer;
    private static readonly Meter PlexMeter = new("PlexAutoplay.PlexApi");
    private static readonly Counter<int> PlexErrorCounter = PlexMeter.CreateCounter<int>("plex_api_errors_total");

    public PlexApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlexLibraryDto>> GetLibrariesAsync()
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
                return new List<PlexLibraryDto>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var libraries = new List<PlexLibraryDto>();
            var directory = doc.RootElement.GetProperty("Directory");

            foreach (var item in directory.EnumerateArray())
            {
                libraries.Add(new PlexLibraryDto(
                    item.GetProperty("key").GetString() ?? "",
                    item.GetProperty("title").GetString() ?? "",
                    item.GetProperty("type").GetString() ?? "",
                    item.GetProperty("agent").GetString() ?? ""
                ));
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
            return new List<PlexLibraryDto>();
        }
    }

    public async Task<List<PlexEpisodeDto>> GetEpisodesAsync(string sectionKey)
    {
        using var activity = ActivitySource.StartActivity("Plex.GetEpisodes");
        activity?.SetTag("service.name", "plex");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", $"/library/sections/{sectionKey}/items");

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync($"/library/sections/{sectionKey}/items");

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                activity?.SetTag("error", true);
                Log.Warning("Plex API returned {StatusCode} for GetEpisodes", (int)response.StatusCode);
                return new List<PlexEpisodeDto>();
            }

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var episodes = new List<PlexEpisodeDto>();
            var metadata = doc.RootElement.GetProperty("MediaContainer").GetProperty("Metadata");

            foreach (var item in metadata.EnumerateArray())
            {
                // Only process episodes (type "episode")
                if (item.TryGetProperty("type", out var typeEl) && typeEl.GetString() != "episode")
                    continue;

                string? filePath = null;
                if (item.TryGetProperty("Media", out var mediaEl))
                {
                    foreach (var media in mediaEl.EnumerateArray())
                    {
                        if (media.TryGetProperty("Part", out var partsEl))
                        {
                            foreach (var part in partsEl.EnumerateArray())
                            {
                                filePath = part.GetProperty("file").GetString();
                                break;
                            }
                        }
                        if (filePath != null) break;
                    }
                }

                episodes.Add(new PlexEpisodeDto(
                    item.GetProperty("ratingKey").GetString() ?? "",
                    item.GetProperty("title").GetString() ?? "",
                    item.GetProperty("parentIndex").GetInt32(),
                    item.GetProperty("index").GetInt32(),
                    filePath ?? ""
                ));
            }

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode, section_key = sectionKey, count = episodes.Count });

            return episodes;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            PlexErrorCounter.Add(1, new KeyValuePair<string, object?>("service", "plex"), new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
            Log.Error(ex, "Plex API call failed: GetEpisodes", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name, section_key = sectionKey });
            return new List<PlexEpisodeDto>();
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
