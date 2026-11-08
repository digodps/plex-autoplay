namespace PlexAutoplay.Infrastructure.PlexProxy;

using PlexAutoplay.Application;

/// <summary>
/// Resolve URL e token do Plex no banco a cada requisição, permitindo troca
/// em runtime sem reiniciar o container. Não usa DefaultRequestHeaders.
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
        var settings = await _settings.GetAsync();

        if (request.RequestUri is { IsAbsoluteUri: false })
        {
            var baseUri = new Uri(settings.PlexUrl.TrimEnd('/') + "/", UriKind.Absolute);
            request.RequestUri = new Uri(baseUri, request.RequestUri);
        }

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
