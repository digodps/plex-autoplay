using PlexAutoplay.Application;
using PlexAutoplay.Domain;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Auth;
using PlexAutoplay.Infrastructure.AutoSkip;
using PlexAutoplay.Infrastructure.Cache;
using PlexAutoplay.Infrastructure.Detection;
using PlexAutoplay.Infrastructure.PlexProxy;
using PlexAutoplay.Infrastructure.Sync;
using PlexAutoplay.Infrastructure.Telemetry;
using PlexAutoplay.Web.Services;

namespace PlexAutoplay.Web.Configuration;

/// <summary>
/// Registra todos os serviços da aplicação no contêiner de injeção de dependência.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddOpenApi();
        services.AddOpenTelemetryTelemetry(builder.Configuration);

        // Periodic sync background service
        services.AddHostedService<SyncBackgroundService>();
        services.AddHostedService<CatalogSyncBackgroundService>();

        // In-memory cache and the generic cache port (pluggable backend)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, ImageCacheService>();

        // SQLite repositories
        var sqliteConnectionString = Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING") ?? "Data Source=/data/app.db";
        services.AddSingleton<IMarkerRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteMarkerRepository(sqliteConnectionString));
        services.AddSingleton<IUserThemeRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteUserThemeRepository(sqliteConnectionString));
        services.AddSingleton<ILibraryRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteLibraryRepository(sqliteConnectionString));
        services.AddSingleton<ISeriesRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteSeriesRepository(sqliteConnectionString));

        // JWT token service
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret-change-in-production";
        services.AddSingleton(new JwtTokenService(jwtSecret));

        // Plex configuration
        var plexUrl = Environment.GetEnvironmentVariable("PLEX_URL") ?? "http://localhost:32400";
        var plexToken = Environment.GetEnvironmentVariable("PLEX_TOKEN") ?? "";

        // HttpClient for image proxy
        services.AddSingleton<ImageProxyService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(plexUrl);
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            return new ImageProxyService(httpClient, plexToken, plexUrl, sp.GetRequiredService<ICacheService>());
        });

        // HttpClient for Plex API calls
        services.AddHttpClient<PlexApiClient>(client =>
        {
            client.BaseAddress = new Uri(plexUrl);
            client.DefaultRequestHeaders.Add("X-Plex-Token", plexToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddSingleton<IPlexApiClient>(sp => sp.GetRequiredService<PlexApiClient>());

        // PlexAutoSkip reads markers from SQLite.
        services.AddSingleton<IPlexAutoSkipService>(sp =>
            new PlexAutoSkipService(sp.GetRequiredService<IMarkerRepository>()));

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        // Marker detector
        services.AddSingleton<IMarkerDetector, PlexAutoplay.Infrastructure.Detection.FfmpegMarkerDetector>();

        // In-memory scan job tracking
        services.AddSingleton<IScanJobStore, ScanJobStore>();

        // Process information service
        services.AddSingleton<IProcessInfoService, ProcessInfoService>();
    }
}