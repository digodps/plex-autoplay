namespace PlexAutoplay.UnitTests;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Infrastructure.Persistence.Repositories;
using Xunit;

/// <summary>
/// Verifica a listagem paginada de séries por biblioteca, inclusive sem filtro.
/// </summary>
public class SqliteSeriesRepositoryTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"series-test-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_dbPath}";

    public void Dispose()
    {
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    private static Series Make(string ratingKey, string title) => new()
    {
        RatingKey = ratingKey,
        LibraryRatingKey = "6",
        Title = title,
        Summary = string.Empty,
        Thumb = string.Empty,
    };

    /// <summary>Cria a tabela referenciada pela FK de Series.</summary>
    private SqliteSeriesRepository CreateRepository()
    {
        using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Library (RatingKey TEXT NOT NULL PRIMARY KEY); INSERT OR IGNORE INTO Library (RatingKey) VALUES ('6');";
            cmd.ExecuteNonQuery();
        }

        return new SqliteSeriesRepository(ConnectionString);
    }

    [Fact]
    public async Task GetByLibraryAsync_WithoutQuery_ReturnsPageAndTotal()
    {
        var repo = CreateRepository();
        await repo.UpsertAsync(Make("1", "Bleach"));
        await repo.UpsertAsync(Make("2", "Naruto"));

        var (items, total) = await repo.GetByLibraryAsync("6", 0, 10, null);

        Assert.Equal(2, total);
        Assert.Equal(new[] { "Bleach", "Naruto" }, items.Select(s => s.Title));
    }

    [Fact]
    public async Task GetByLibraryAsync_WithQuery_FiltersItemsAndTotal()
    {
        var repo = CreateRepository();
        await repo.UpsertAsync(Make("1", "Bleach"));
        await repo.UpsertAsync(Make("2", "Naruto"));

        var (items, total) = await repo.GetByLibraryAsync("6", 0, 10, "ble");

        Assert.Equal(1, total);
        Assert.Equal("Bleach", Assert.Single(items).Title);
    }
}
