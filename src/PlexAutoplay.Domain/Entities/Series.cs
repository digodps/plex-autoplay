namespace PlexAutoplay.Domain.Entities;

/// <summary>Série sincronizada do catálogo Plex.</summary>
public class Series
{
    public string RatingKey { get; set; } = string.Empty;
    public string LibraryRatingKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Thumb { get; set; } = string.Empty;
}
