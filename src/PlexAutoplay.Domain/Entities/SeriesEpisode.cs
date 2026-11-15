namespace PlexAutoplay.Domain.Entities;

/// <summary>Episódio de série com progresso de visualização local.</summary>
public class SeriesEpisode
{
    public string RatingKey { get; set; } = string.Empty;
    public string SeriesRatingKey { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Thumb { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public bool Watched { get; set; }
    public int WatchProgress { get; set; } // 0-100
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}