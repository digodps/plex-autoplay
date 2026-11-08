namespace PlexAutoplay.Infrastructure.PlexProxy;

using PlexAutoplay.Application;

/// <summary>
/// Resolve o token do Plex no banco a cada requisição, permitindo troca em
/// runtime sem reiniciar o container. A URL absoluta é responsabilidade do
/// <see cref="PlexApiClient"/>, que conhece o path prefix de PLEX_URL.
/// </summary>
public sealed class PlexRequestHandler : DelegatingHandler
{
    private readonly IAppSettingsStore _settings;

    public PlexRequestHandler(IAppSettingsStore settings)
    {
        _settings = settings;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is not { IsAbsoluteUri: true })
        {
            throw new InvalidOperationException(
                "PlexApiClient deve montar URL absoluta a partir de PLEX_URL antes de enviar a requisição.");
        }

        var settings = await _settings.GetAsync();

        if (!request.Headers.Contains("X-Plex-Token") && !string.IsNullOrEmpty(settings.PlexToken))
        {
            request.Headers.Add("X-Plex-Token", settings.PlexToken);
        }

        if (!request.Headers.Accept.Any())
        {
            request.Headers.Accept.ParseAdd("application/json");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
