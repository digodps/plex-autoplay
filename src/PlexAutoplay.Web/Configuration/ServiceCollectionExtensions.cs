using PlexAutoplay.Application;
<<<<<<< HEAD
using PlexAutoplay.Application.UseCases;
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
using PlexAutoplay.Domain;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Auth;
using PlexAutoplay.Infrastructure.AutoSkip;
using PlexAutoplay.Infrastructure.Cache;
using PlexAutoplay.Infrastructure;
using PlexAutoplay.Infrastructure.Persistence.Repositories;
using PlexAutoplay.Infrastructure.Detection;
using PlexAutoplay.Infrastructure.PlexProxy;
using PlexAutoplay.Infrastructure.Sync;
using PlexAutoplay.Infrastructure.Telemetry;

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

<<<<<<< HEAD
        // In-memory cache and the generic cache port (pluggable backend)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, ImageCacheService>();
=======
        // Periodic sync background service
        services.AddHostedService<SyncBackgroundService>();
        services.AddHostedService<CatalogSyncBackgroundService>();

        // In-memory cache and the generic cache port (pluggable backend)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, ImageCacheService>();

        // SQLite repositories
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
        var sqliteConnectionString = Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING") ?? "Data Source=/data/app.db";
        services.AddSingleton<IMarkerRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteMarkerRepository(sqliteConnectionString));
        services.AddSingleton<IUserThemeRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteUserThemeRepository(sqliteConnectionString));
        services.AddSingleton<ILibraryRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteLibraryRepository(sqliteConnectionString));
        services.AddSingleton<ISeriesRepository>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteSeriesRepository(sqliteConnectionString));
<<<<<<< HEAD
        services.AddSingleton<IAppSettingsStore>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteAppSettingsStore(sqliteConnectionString));
        services.AddSingleton<UpdateAppSettingsUseCase>();

        // Credencial administrativa, setup inicial e troca de senha
        services.AddSingleton<IAuthCredentialStore>(
            new PlexAutoplay.Infrastructure.Persistence.Repositories.SqliteAuthCredentialStore(sqliteConnectionString));
        services.AddSingleton<IPasswordHasher>(new Pbkdf2PasswordHasher());
        services.AddSingleton<SetupTokenService>();
        services.AddSingleton<ISetupTokenValidator>(sp => sp.GetRequiredService<SetupTokenService>());
        services.AddSingleton<LoginUseCase>();
        services.AddTransient<CompleteSetupUseCase>();
        services.AddSingleton<ChangePasswordUseCase>();
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

        // JWT token service
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret-change-in-production";
        services.AddSingleton<JwtTokenService>(sp => new JwtTokenService(jwtSecret, sp.GetRequiredService<ILogger<JwtTokenService>>()));
        services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<JwtTokenService>());

        // Plex configuration is resolved per request from the database
        services.AddTransient<PlexRequestHandler>();
        services.AddHttpClient<PlexApiClient>()
            .AddHttpMessageHandler<PlexRequestHandler>();
        services.AddTransient<IPlexApiClient>(sp => sp.GetRequiredService<PlexApiClient>());

        // HttpClient for image proxy
        services.AddSingleton<IImageProxyService>(sp =>
            new ImageProxyService(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                sp.GetRequiredService<IAppSettingsStore>(),
                sp.GetRequiredService<ICacheService>()));

        // PlexAutoSkip reads markers from SQLite.
        services.AddSingleton<IPlexAutoSkipService>(sp =>
            new PlexAutoSkipService(sp.GetRequiredService<IMarkerRepository>()));

<<<<<<< HEAD
        // Modern client seek strategy
        services.AddSingleton<IClientSeekStrategyProvider, PlexModernClientDetector>();

        // Use cases (application boundary)
        services.AddSingleton<LibraryScanUseCase>();
        services.AddSingleton<SaveMarkerUseCase>();
        services.AddSingleton<GetMarkersQuery>();
        services.AddSingleton<GetSeriesQuery>();
        services.AddTransient<CatalogSyncUseCase>();
        services.AddSingleton<GetThemeQuery>();
        services.AddSingleton<SaveThemeCommand>();
=======
        // PlexAutoSkip reads markers from SQLite.
        services.AddSingleton<IPlexAutoSkipService>(sp =>
            new PlexAutoSkipService(sp.GetRequiredService<IMarkerRepository>()));
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        // Marker detector
        services.AddSingleton<IMarkerDetector, PlexAutoplay.Infrastructure.Detection.FfmpegMarkerDetector>();

<<<<<<< HEAD
        // Scan persistence (SQLite)
        services.AddSingleton<IScanJobRepository>(sp =>
            new SqliteScanJobRepository(sqliteConnectionString));
        services.AddSingleton<IScanJobLogStore>(sp =>
            new SqliteScanJobLogStore(sqliteConnectionString));

        // Limpeza automática de logs antigos (executa no startup e a cada 24h)
        services.AddHostedService<LogCleanupBackgroundService>();

        // Process information service
        services.AddSingleton<IProcessInfoService, PlexAutoplay.Infrastructure.Platform.ProcessInfoService>();

        // Local media path resolver (probes the filesystem)
        services.AddSingleton<IPathResolver, PlexAutoplay.Infrastructure.Platform.PathResolver>();

=======
        // In-memory scan job tracking
        services.AddSingleton<IScanJobStore, ScanJobStore>();

        // Process information service
        services.AddSingleton<IProcessInfoService, ProcessInfoService>();
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    }
}