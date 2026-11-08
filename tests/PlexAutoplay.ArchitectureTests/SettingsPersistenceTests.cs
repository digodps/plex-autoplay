using PlexAutoplay.Application;
using PlexAutoplay.Infrastructure.Persistence;
using PlexAutoplay.Infrastructure.Persistence.Repositories;

namespace PlexAutoplay.ArchitectureTests;

public class SettingsPersistenceTests
{
    private static string TempConnectionString(out string dir)
    {
        dir = Path.Combine(Path.GetTempPath(), "plex-autoplay-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        return $"Data Source={Path.Combine(dir, "app.db")}";
    }

    [Fact]
    public async Task Store_roundtrip_keeps_all_values()
    {
        var connectionString = TempConnectionString(out var dir);
        try
        {
            await DatabaseInitializer.InitializeAsync(connectionString);
            var store = new SqliteAppSettingsStore(connectionString);

            await store.SaveAsync(new AppSettingsSnapshot(
                "http://plex.local:32400", "token-123", [dir, "/media/extra"], 7, 42));

            var loaded = await store.GetAsync();

            Assert.Equal("http://plex.local:32400", loaded.PlexUrl);
            Assert.Equal("token-123", loaded.PlexToken);
            Assert.Equal(new[] { dir, "/media/extra" }, loaded.MediaRoots);
            Assert.Equal(7, loaded.SyncIntervalMinutes);
            Assert.Equal(42, loaded.SyncCatalogIntervalMinutes);
            Assert.True(loaded.PlexTokenSet);
        }
        finally
        {
            TestDirectory.Delete(dir);
        }
    }

    [Fact]
    public async Task Seed_runs_once_and_does_not_overwrite_saved_values()
    {
        var connectionString = TempConnectionString(out var dir);
        var original = Environment.GetEnvironmentVariable(AppSettingKeys.PlexUrl);
        try
        {
            await DatabaseInitializer.InitializeAsync(connectionString);
            Environment.SetEnvironmentVariable(AppSettingKeys.PlexUrl, "http://seed.local:32400");

            await AppSettingsSeeder.SeedAsync(connectionString);
            var store = new SqliteAppSettingsStore(connectionString);
            Assert.Equal("http://seed.local:32400", (await store.GetAsync()).PlexUrl);

            await store.SaveAsync((await store.GetAsync()) with { PlexUrl = "http://saved.local:32400" });
            Environment.SetEnvironmentVariable(AppSettingKeys.PlexUrl, "http://other.local:32400");
            await AppSettingsSeeder.SeedAsync(connectionString);

            Assert.Equal("http://saved.local:32400", (await store.GetAsync()).PlexUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AppSettingKeys.PlexUrl, original);
            TestDirectory.Delete(dir);
        }
    }

    [Fact]
    public async Task UseCase_rejects_invalid_input_without_writing()
    {
        var dir = Path.Combine(Path.GetTempPath(), "plex-autoplay-media-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var store = new InMemorySettingsStore(AppSettingsSnapshot.Defaults with { MediaRoots = [dir] });
            var useCase = new UpdateAppSettingsUseCase(store);

            var result = await useCase.ExecuteAsync(new UpdateAppSettingsRequest(
                "ftp://invalid", "new-token", [dir], 0, 5000));

            Assert.False(result.Success);
            Assert.Equal(3, result.Errors.Count);
            Assert.False(store.Saved);
            Assert.Equal(AppSettingDefaults.PlexToken, (await store.GetAsync()).PlexToken);
        }
        finally
        {
            TestDirectory.Delete(dir);
        }
    }

    [Fact]
    public async Task UseCase_preserves_token_when_not_sent()
    {
        var dir = Path.Combine(Path.GetTempPath(), "plex-autoplay-media-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var store = new InMemorySettingsStore(AppSettingsSnapshot.Defaults with
            {
                PlexToken = "existing-token",
                MediaRoots = [dir]
            });
            var useCase = new UpdateAppSettingsUseCase(store);

            var result = await useCase.ExecuteAsync(new UpdateAppSettingsRequest(
                "https://plex.local:32400", null, [dir], 10, 60));

            Assert.True(result.Success);
            Assert.Equal("existing-token", (await store.GetAsync()).PlexToken);
            Assert.Equal(10, (await store.GetAsync()).SyncIntervalMinutes);
        }
        finally
        {
            TestDirectory.Delete(dir);
        }
    }

    private sealed class InMemorySettingsStore : IAppSettingsStore
    {
        private AppSettingsSnapshot _settings;

        public InMemorySettingsStore(AppSettingsSnapshot settings)
        {
            _settings = settings;
        }

        public bool Saved { get; private set; }

        public Task<AppSettingsSnapshot> GetAsync() => Task.FromResult(_settings);

        public Task SaveAsync(AppSettingsSnapshot settings)
        {
            _settings = settings;
            Saved = true;
            return Task.CompletedTask;
        }
    }

    private static class TestDirectory
    {
        public static void Delete(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
            catch
            {
                // best effort cleanup
            }
        }
    }
}
