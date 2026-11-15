namespace PlexAutoplay.Infrastructure.Persistence.Repositories;

using Dapper;
using Microsoft.Data.Sqlite;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using System.Data.Common;

/// <summary>Repositório SQLite para progresso de episódios de série.</summary>
public class SqliteSeriesEpisodeRepository : ISeriesEpisodeRepository
{
    private readonly string _connectionString;

    public SqliteSeriesEpisodeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<SeriesEpisode?> GetByRatingKeyAsync(string ratingKey)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<SeriesEpisode>(
            @"SELECT RatingKey, SeriesRatingKey, SeasonNumber, EpisodeNumber, Title, Summary, Thumb, DurationSeconds, Watched, WatchProgress, AddedAt, UpdatedAt
              FROM SeriesEpisode 
              WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey });
    }

    public async Task<IEnumerable<SeriesEpisode>> GetBySeriesRatingKeyAsync(string seriesRatingKey)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<SeriesEpisode>(
            @"SELECT RatingKey, SeriesRatingKey, SeasonNumber, EpisodeNumber, Title, Summary, Thumb, DurationSeconds, Watched, WatchProgress, AddedAt, UpdatedAt
              FROM SeriesEpisode 
              WHERE SeriesRatingKey = @SeriesRatingKey
              ORDER BY SeasonNumber, EpisodeNumber",
            new { SeriesRatingKey = seriesRatingKey });
    }

    public async Task UpsertAsync(SeriesEpisode episode)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO SeriesEpisode (RatingKey, SeriesRatingKey, SeasonNumber, EpisodeNumber, Title, Summary, Thumb, DurationSeconds, Watched, WatchProgress, AddedAt, UpdatedAt)
              VALUES (@RatingKey, @SeriesRatingKey, @SeasonNumber, @EpisodeNumber, @Title, @Summary, @Thumb, @DurationSeconds, @Watched, @WatchProgress, @AddedAt, @UpdatedAt)
              ON CONFLICT(RatingKey) DO UPDATE SET
                SeriesRatingKey = excluded.SeriesRatingKey,
                SeasonNumber = excluded.SeasonNumber,
                EpisodeNumber = excluded.EpisodeNumber,
                Title = excluded.Title,
                Summary = excluded.Summary,
                Thumb = excluded.Thumb,
                DurationSeconds = excluded.DurationSeconds,
                Watched = excluded.Watched,
                WatchProgress = excluded.WatchProgress,
                UpdatedAt = datetime('now')",
            episode);
    }

    public async Task UpdateWatchProgressAsync(string ratingKey, bool watched, int watchProgress)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE SeriesEpisode 
              SET Watched = @Watched, WatchProgress = @WatchProgress, UpdatedAt = datetime('now')
              WHERE RatingKey = @RatingKey",
            new { RatingKey = ratingKey, Watched = watched, WatchProgress = watchProgress });
    }

    public async Task DeleteBySeriesRatingKeyAsync(string seriesRatingKey)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM SeriesEpisode WHERE SeriesRatingKey = @SeriesRatingKey",
            new { SeriesRatingKey = seriesRatingKey });
    }
}