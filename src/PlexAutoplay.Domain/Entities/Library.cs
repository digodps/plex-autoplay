namespace PlexAutoplay.Domain.Entities;

/// <summary>Biblioteca de séries sincronizada do Plex.</summary>
public class Library
{
    public string RatingKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
}
