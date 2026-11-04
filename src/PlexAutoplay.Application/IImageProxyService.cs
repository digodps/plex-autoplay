namespace PlexAutoplay.Application;

/// <summary>
/// Porta para o proxy de imagens do Plex (com cache).
/// </summary>
public interface IImageProxyService
{
    Task<byte[]> GetImageAsync(string imagePath);
}
