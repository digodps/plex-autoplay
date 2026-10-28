namespace PlexAutoplay.Infrastructure.Cache;

using Microsoft.Extensions.Caching.Memory;
using PlexAutoplay.Domain.Ports;

public class ImageCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public ImageCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public byte[]? Get(string key)
    {
        return _cache.TryGetValue(key, out byte[]? data) ? data : null;
    }

    public void Set(string key, byte[] data, TimeSpan? expiry = null)
    {
        var options = expiry.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry.Value }
            : new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

        _cache.Set(key, data, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
