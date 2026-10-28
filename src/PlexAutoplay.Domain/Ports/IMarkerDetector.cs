namespace PlexAutoplay.Domain.Ports;

using PlexAutoplay.Domain.Entities;

public interface IMarkerDetector
{
    IEnumerable<Marker> Detect(string filePath);
}
