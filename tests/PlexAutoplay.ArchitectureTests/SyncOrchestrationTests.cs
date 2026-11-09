using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using Xunit;

public sealed class SyncOrchestrationTests
{
    private sealed record Call(string Method, string? Arg);

    private sealed class RecordingPlexClient : IPlexApiClient
    {
        public List<Call> Calls { get; } = [];

        public Task<PlexLibrariesResult> GetLibrariesAsync()
        {
            Calls.Add(new Call(nameof(GetLibrariesAsync), null));
            return Task.FromResult(new PlexLibrariesResult([
                new PlexLibraryDto("lib1", "Shows", "show", "tvdb")
            ]));
        }

        public Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey)
        {
            Calls.Add(new Call(nameof(GetEpisodesAsync), sectionKey));
            return Task.FromResult(new PlexEpisodesResult([
                new PlexEpisodeDto("ep1", "S01E01", 1, 1, "/path/ep1.mkv")
            ]));
        }

        public Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null)
        {
            Calls.Add(new Call(nameof(GetSeriesAsync), sectionKey));
            return Task.FromResult(new PlexSeriesPage([
                new PlexSeriesDto("s1", "Series One", "Summary", "/thumb1.jpg")
            ], 1));
        }

        public Task<PlexEpisodesResult> GetSeriesEpisodesAsync(string seriesKey) =>
            Task.FromResult(PlexEpisodesResult.Empty);

        public Task<PlexIdentityResult> GetIdentityAsync(string plexUrl, string plexToken) =>
            Task.FromResult(new PlexIdentityResult("machine", "Plex", "1.0"));
    }

    private sealed class RecordingLibraryRepository : ILibraryRepository
    {
        public List<Call> Calls { get; } = [];
        private readonly Dictionary<string, Library> _libs = [];

        public Task<IEnumerable<Library>> GetAllAsync()
        {
            Calls.Add(new Call(nameof(GetAllAsync), null));
            return Task.FromResult(_libs.Values.AsEnumerable());
        }

        public Task<Library?> GetByRatingKeyAsync(string ratingKey)
        {
            Calls.Add(new Call(nameof(GetByRatingKeyAsync), ratingKey));
            _libs.TryGetValue(ratingKey, out var lib);
            return Task.FromResult(lib);
        }

        public Task UpsertAsync(Library library)
        {
            Calls.Add(new Call(nameof(UpsertAsync), library.RatingKey));
            _libs[library.RatingKey] = library;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string ratingKey)
        {
            Calls.Add(new Call(nameof(DeleteAsync), ratingKey));
            _libs.Remove(ratingKey);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingSeriesRepository : ISeriesRepository
    {
        public List<Call> Calls { get; } = [];
        private readonly Dictionary<string, Series> _series = [];

        public Task<(IEnumerable<Series> Items, int Total)> GetByLibraryAsync(string libraryRatingKey, int offset, int limit, string? query)
        {
            Calls.Add(new Call(nameof(GetByLibraryAsync), libraryRatingKey));
            return Task.FromResult((_series.Values.AsEnumerable(), _series.Count));
        }

        public Task<Series?> GetByRatingKeyAsync(string ratingKey)
        {
            Calls.Add(new Call(nameof(GetByRatingKeyAsync), ratingKey));
            _series.TryGetValue(ratingKey, out var s);
            return Task.FromResult(s);
        }

        public Task UpsertAsync(Series series)
        {
            Calls.Add(new Call(nameof(UpsertAsync), series.RatingKey));
            _series[series.RatingKey] = series;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string ratingKey)
        {
            Calls.Add(new Call(nameof(DeleteAsync), ratingKey));
            _series.Remove(ratingKey);
            return Task.CompletedTask;
        }

        public Task DeleteByLibraryAsync(string libraryRatingKey)
        {
            Calls.Add(new Call(nameof(DeleteByLibraryAsync), libraryRatingKey));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSettingsStore : IAppSettingsStore
    {
        public Task<AppSettingsSnapshot> GetAsync() => Task.FromResult(AppSettingsSnapshot.Defaults);
        public Task SaveAsync(AppSettingsSnapshot settings) => Task.CompletedTask;
    }

    private sealed class FakeScanJobStore : IScanJobStore
    {
        public void Add(ScanJob job) { }
        public ScanJob? Get(string jobId) => new ScanJob { JobId = jobId };
        public ScanJob? GetLatest() => null;
        public void AddLog(string jobId, string level, string message, string? suggestion = null) { }
    }

    private sealed class FakeMarkerDetector : IMarkerDetector
    {
        public IEnumerable<Marker> Detect(string filePath) => [];
    }

    private sealed class FakeMarkerRepository : IMarkerRepository
    {
        public Task<bool> ExistsAsync(string ratingKey, string kind) => Task.FromResult(false);
        public Task AddAsync(Marker marker) => Task.CompletedTask;
        public Task<IEnumerable<Marker>> GetByRatingKeyAsync(string ratingKey) => Task.FromResult(Enumerable.Empty<Marker>());
        public Task<IEnumerable<Marker>> GetAllAsync() => Task.FromResult(Enumerable.Empty<Marker>());
        public Task UpdateAsync(Marker marker) => Task.CompletedTask;
        public Task DeleteAsync(string ratingKey, string kind) => Task.CompletedTask;
    }

    private sealed class FakePathResolver : IPathResolver
    {
        public string Resolve(IReadOnlyList<string> mediaRoots, string filePath) => "";
    }

    [Fact]
    public async Task Orchestrator_runs_catalog_then_markers_in_order()
    {
        // Arrange
        var plex = new RecordingPlexClient();
        var libRepo = new RecordingLibraryRepository();
        var seriesRepo = new RecordingSeriesRepository();
        var settings = new FakeSettingsStore();
        var scanJobs = new FakeScanJobStore();
        var markerDetector = new FakeMarkerDetector();
        var markerRepo = new FakeMarkerRepository();
        var pathResolver = new FakePathResolver();

        var catalogSync = new CatalogSyncUseCase(plex, libRepo, seriesRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<CatalogSyncUseCase>.Instance);
        var markerScan = new LibraryScanUseCase(markerDetector, markerRepo, pathResolver);
        var markerSync = new EpisodeMarkerSyncUseCase(plex, markerScan, settings, Microsoft.Extensions.Logging.Abstractions.NullLogger<EpisodeMarkerSyncUseCase>.Instance);
        var orchestrator = new SynchronizationOrchestrator(catalogSync, markerSync, scanJobs, Microsoft.Extensions.Logging.Abstractions.NullLogger<SynchronizationOrchestrator>.Instance);

        // Act
        var result = await orchestrator.RunAsync("job-test", CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        // Verify order: GetSeriesAsync (catálogo) called before GetEpisodesAsync (marcadores)
        var idxGetSeries = plex.Calls.FindIndex(c => c.Method == nameof(IPlexApiClient.GetSeriesAsync));
        var idxGetEpisodes = plex.Calls.FindIndex(c => c.Method == nameof(IPlexApiClient.GetEpisodesAsync));
        Assert.True(idxGetSeries >= 0, "GetSeriesAsync deve ter sido chamado (catálogo)");
        Assert.True(idxGetEpisodes >= 0, "GetEpisodesAsync deve ter sido chamado (marcadores)");
        Assert.True(idxGetSeries < idxGetEpisodes, "Catálogo (GetSeriesAsync) deve executar antes dos marcadores (GetEpisodesAsync)");

        // Verify contagens esperadas
        Assert.Equal(1, result.Stage1LibrariesInserted);
        Assert.Equal(1, result.Stage1SeriesInserted);
        Assert.Equal(0, result.Stage2Processed);
        Assert.Equal(1, result.Stage2Skipped);
        Assert.Equal(0, result.Stage2Errors);
    }

    [Fact]
    public async Task Orchestrator_fails_fast_if_catalog_fails()
    {
        // Arrange: Plex falha em GetLibrariesAsync
        var plex = new RecordingPlexClient
        {
            // override GetLibrariesAsync para falhar
        };
        // Simpler: create a failing plex client
        var failingPlex = new FailingPlexClient();
        var libRepo = new RecordingLibraryRepository();
        var seriesRepo = new RecordingSeriesRepository();
        var settings = new FakeSettingsStore();
        var scanJobs = new FakeScanJobStore();
        var markerDetector = new FakeMarkerDetector();
        var markerRepo = new FakeMarkerRepository();
        var pathResolver = new FakePathResolver();

        var catalogSync = new CatalogSyncUseCase(failingPlex, libRepo, seriesRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<CatalogSyncUseCase>.Instance);
        var markerScan = new LibraryScanUseCase(markerDetector, markerRepo, pathResolver);
        var markerSync = new EpisodeMarkerSyncUseCase(failingPlex, markerScan, settings, Microsoft.Extensions.Logging.Abstractions.NullLogger<EpisodeMarkerSyncUseCase>.Instance);
        var orchestrator = new SynchronizationOrchestrator(catalogSync, markerSync, scanJobs, Microsoft.Extensions.Logging.Abstractions.NullLogger<SynchronizationOrchestrator>.Instance);

        // Act
        var result = await orchestrator.RunAsync("job-test", CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("conexão recusada", result.ErrorMessage);
        // GetEpisodesAsync must not be called after catalog failure
        Assert.DoesNotContain(failingPlex.Calls, c => c.Method == nameof(IPlexApiClient.GetEpisodesAsync));
    }

    private sealed class FailingPlexClient : IPlexApiClient
    {
        public List<Call> Calls { get; } = [];

        public Task<PlexLibrariesResult> GetLibrariesAsync()
        {
            Calls.Add(new Call(nameof(GetLibrariesAsync), null));
            return Task.FromResult(PlexLibrariesResult.FailedResult("conexão recusada"));
        }

        public Task<PlexEpisodesResult> GetEpisodesAsync(string sectionKey)
        {
            Calls.Add(new Call(nameof(GetEpisodesAsync), sectionKey));
            return Task.FromResult(PlexEpisodesResult.Empty);
        }

        public Task<PlexSeriesPage> GetSeriesAsync(string sectionKey, int containerStart, int containerSize, string? titleQuery = null)
        {
            Calls.Add(new Call(nameof(GetSeriesAsync), sectionKey));
            return Task.FromResult(new PlexSeriesPage([], 0));
        }

        public Task<PlexEpisodesResult> GetSeriesEpisodesAsync(string seriesKey) =>
            Task.FromResult(PlexEpisodesResult.Empty);

        public Task<PlexIdentityResult> GetIdentityAsync(string plexUrl, string plexToken) =>
            Task.FromResult(new PlexIdentityResult("machine", "Plex", "1.0"));
    }
}