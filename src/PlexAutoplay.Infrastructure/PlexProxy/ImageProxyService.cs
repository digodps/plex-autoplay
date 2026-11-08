namespace PlexAutoplay.Infrastructure.PlexProxy;

using PlexAutoplay.Application;
using PlexAutoplay.Domain.Ports;

public class ImageProxyService : IImageProxyService
{
    private readonly HttpClient _httpClient;
    private readonly IAppSettingsStore _settings;
    private readonly ICacheService _cacheService;

    public ImageProxyService(HttpClient httpClient, IAppSettingsStore settings, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _settings = settings;
        _cacheService = cacheService;
    }

    public async Task<byte[]> GetImageAsync(string imagePath)
    {
        var settings = await _settings.GetAsync();
        var plexUrl = settings.PlexUrl.TrimEnd('/');
        var cacheKey = $"image:{plexUrl}:{imagePath}";

        var cached = _cacheService.Get(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var url = $"{plexUrl}{imagePath}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Plex-Token", settings.PlexToken);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        _cacheService.Set(cacheKey, imageBytes);

        return imageBytes;
    }
}
