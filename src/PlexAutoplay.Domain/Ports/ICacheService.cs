namespace PlexAutoplay.Domain.Ports;

public interface ICacheService
{
    byte[]? Get(string key);
    void Set(string key, byte[] data, TimeSpan? expiry = null);
    void Remove(string key);
}
