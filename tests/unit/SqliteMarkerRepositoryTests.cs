namespace PlexAutoplay.UnitTests;

using Xunit;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Infrastructure.Persistence.Repositories;

/// <summary>
/// Verifica a migração aditiva da coluna Origin e o roundtrip de persistência.
/// </summary>
public class SqliteMarkerRepositoryTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"markers-test-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_dbPath}";

    public void Dispose()
    {
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    /// <summary>
    /// Simula o schema antigo (sem coluna Origin), como em bancos criados antes desta change.
    /// </summary>
    private void CreateLegacySchema()
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE Markers (
            RatingKey TEXT NOT NULL,
            Kind TEXT NOT NULL,
            StartMs INTEGER NOT NULL,
            EndMs INTEGER NOT NULL,
            PRIMARY KEY (RatingKey, Kind))";
        cmd.ExecuteNonQuery();
    }

    [Fact]
    public void Migration_OnLegacyDatabase_AddsOriginColumnWithManualDefault()
    {
        CreateLegacySchema();

        // Inserir linha de estilo antigo (sem Origin)
        using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Markers (RatingKey, Kind, StartMs, EndMs) VALUES ('1001', 'intro', 0, 90000)";
            cmd.ExecuteNonQuery();
        }

        // Construir o repositório dispara a migração no boot
        var repo = new SqliteMarkerRepository(ConnectionString);
        var markers = repo.GetAllAsync().Result.ToList();

        var marker = Assert.Single(markers);
        Assert.Equal("1001", marker.RatingKey);
        Assert.Equal("manual", marker.Origin);
    }

    [Fact]
    public void Migration_RunsIdempotently()
    {
        CreateLegacySchema();
        _ = new SqliteMarkerRepository(ConnectionString);
        // Segunda instanciação não deve lançar exceção ("duplicate column name")
        _ = new SqliteMarkerRepository(ConnectionString);
    }

    [Fact]
    public async Task AddAndRead_PreservesOrigin()
    {
        var repo = new SqliteMarkerRepository(ConnectionString);

        await repo.AddAsync(new Marker { RatingKey = "2001", Kind = "intro", StartMs = 10, EndMs = 90000, Origin = "detected" });
        await repo.AddAsync(new Marker { RatingKey = "2002", Kind = "credits", StartMs = 90000, EndMs = 150000, Origin = "manual" });

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);

        var detected = all.Single(m => m.RatingKey == "2001");
        Assert.Equal("detected", detected.Origin);

        var manual = all.Single(m => m.RatingKey == "2002");
        Assert.Equal("manual", manual.Origin);

        // GetByRatingKeyAsync também expõe a origem
        var byKey = (await repo.GetByRatingKeyAsync("2001")).ToList();
        Assert.Equal("detected", byKey.Single().Origin);
    }

    [Fact]
    public async Task Add_WithoutOrigin_DefaultsToManual()
    {
        var repo = new SqliteMarkerRepository(ConnectionString);
        var marker = new Marker { RatingKey = "3001", Kind = "intro", StartMs = 0, EndMs = 60000 };
        Assert.Equal("manual", marker.Origin); // default da entidade

        await repo.AddAsync(marker);
        var persisted = (await repo.GetByRatingKeyAsync("3001")).Single();
        Assert.Equal("manual", persisted.Origin);
    }
}
