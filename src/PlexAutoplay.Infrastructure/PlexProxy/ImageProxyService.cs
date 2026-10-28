namespace PlexAutoplay.Infrastructure.PlexProxy;

using System.Net;
using System.Net.Http.Headers;
using PlexAutoplay.Domain.Ports;

public class ImageProxyService
{
    private readonly HttpClient _httpClient;
    private readonly string _plexToken;
    private readonly string _plexUrl;
    private readonly ICacheService _cacheService;

    public ImageProxyService(HttpClient httpClient, string plexToken, string plexUrl, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _plexToken = plexToken;
        _plexUrl = plexUrl.TrimEnd('/');
        _cacheService = cacheService;
    }

    public async Task<byte[]> GetImageAsync(string imagePath)
    {
        var cacheKey = $"image:{imagePath}";

        var cached = _cacheService.Get(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var url = $"{_plexUrl}{imagePath}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Plex-Token", _plexToken);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        _cacheService.Set(cacheKey, imageBytes);

        return imageBytes;
    }
}
