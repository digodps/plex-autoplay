namespace PlexAutoplay.UnitTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Persistence.Repositories;
using PlexAutoplay.Web.Endpoints;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class SqliteUserThemeRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public SqliteUserThemeRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"theme_test_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [Fact]
    public async Task GetAsync_WhenEmpty_ReturnsNull()
    {
        var repo = new SqliteUserThemeRepository(_connectionString);
        var result = await repo.GetAsync("admin");
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_InsertsAndUpdates()
    {
        var repo = new SqliteUserThemeRepository(_connectionString);

        // Insert
        await repo.UpsertAsync(new UserThemePreference { UserId = "admin", AccentColor = "#2563eb", Mode = "dark" });
        var saved = await repo.GetAsync("admin");
        Assert.NotNull(saved);
        Assert.Equal("#2563eb", saved.AccentColor);
        Assert.Equal("dark", saved.Mode);

        // Update
        await repo.UpsertAsync(new UserThemePreference { UserId = "admin", AccentColor = "#16a34a", Mode = "light" });
        var updated = await repo.GetAsync("admin");
        Assert.NotNull(updated);
        Assert.Equal("#16a34a", updated.AccentColor);
        Assert.Equal("light", updated.Mode);
    }
}

public class ThemeEndpointsTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly string _dbPath;

    public ThemeEndpointsTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"theme_ep_test_{Guid.NewGuid()}.db");
        var connStr = $"Data Source={_dbPath}";

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddSingleton<IUserThemeRepository>(new SqliteUserThemeRepository(connStr));
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapThemeEndpoints());
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [Fact]
    public async Task GetTheme_ReturnsDefault_WhenNotSet()
    {
        var response = await _client.GetAsync("/api/theme");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pref = await response.Content.ReadFromJsonAsync<UserThemePreference>();
        Assert.NotNull(pref);
        Assert.Equal("#e5a00d", pref.AccentColor);
        Assert.Equal("system", pref.Mode);
    }

    [Fact]
    public async Task PutTheme_UpdatesAndGetTheme_ReturnsUpdated()
    {
        var updateRes = await _client.PutAsJsonAsync("/api/theme", new UserThemePreference
        {
            UserId = "admin",
            AccentColor = "#7c3aed",
            Mode = "dark"
        });
        Assert.Equal(HttpStatusCode.NoContent, updateRes.StatusCode);

        var getRes = await _client.GetAsync("/api/theme");
        var pref = await getRes.Content.ReadFromJsonAsync<UserThemePreference>();
        Assert.NotNull(pref);
        Assert.Equal("#7c3aed", pref.AccentColor);
        Assert.Equal("dark", pref.Mode);
    }
}
