namespace PlexAutoplay.Domain.Ports;

/// <summary>Linha de log de varredura.</summary>
public sealed record ScanLogEntry(
    DateTime Timestamp,
    string Message,
    string Level,
    string? Suggestion,
    string? JobId = null,
    string? JobStatus = null);

/// <summary>Representa job de varredura.</summary>
public class ScanJob
{
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "queued";
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ScanLogEntry> Logs { get; } = [];
}