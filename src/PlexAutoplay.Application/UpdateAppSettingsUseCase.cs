namespace PlexAutoplay.Application;

/// <summary>Valores parciais aceitos por PUT /api/settings.</summary>
public sealed record UpdateAppSettingsRequest(
    string? PlexUrl,
    string? PlexToken,
    string? MediaRoot,
    int? SyncIntervalMinutes,
    int? SyncCatalogIntervalMinutes);

public sealed record UpdateAppSettingsResult(bool Success, IReadOnlyList<string> Errors)
{
    public static UpdateAppSettingsResult Ok() => new(true, []);
    public static UpdateAppSettingsResult Invalid(params string[] errors) => new(false, errors);
}

/// <summary>
/// Valida e persiste configurações operacionais. Tudo é validado antes de
/// qualquer escrita; o token só muda quando enviado explicitamente.
/// </summary>
public sealed class UpdateAppSettingsUseCase
{
    private const int MinInterval = 1;
    private const int MaxInterval = 1440;
    private readonly IAppSettingsStore _store;

    public UpdateAppSettingsUseCase(IAppSettingsStore store)
    {
        _store = store;
    }

    public async Task<UpdateAppSettingsResult> ExecuteAsync(UpdateAppSettingsRequest request)
    {
        var errors = new List<string>();
        var current = await _store.GetAsync();

        var plexUrl = (request.PlexUrl ?? current.PlexUrl).Trim().TrimEnd('/');
        if (!Uri.TryCreate(plexUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add("plex_url deve ser uma URL absoluta http:// ou https://");
        }

        var mediaRoot = (request.MediaRoot ?? current.MediaRoot).Trim();
        if (!Path.IsPathRooted(mediaRoot) || !Directory.Exists(mediaRoot))
        {
            errors.Add("media_root deve ser um diretório absoluto existente");
        }

        var syncInterval = request.SyncIntervalMinutes ?? current.SyncIntervalMinutes;
        if (syncInterval is < MinInterval or > MaxInterval)
        {
            errors.Add($"sync_interval_minutes deve estar entre {MinInterval} e {MaxInterval}");
        }

        var catalogInterval = request.SyncCatalogIntervalMinutes ?? current.SyncCatalogIntervalMinutes;
        if (catalogInterval is < MinInterval or > MaxInterval)
        {
            errors.Add($"sync_catalog_interval_minutes deve estar entre {MinInterval} e {MaxInterval}");
        }

        if (errors.Count > 0)
        {
            return UpdateAppSettingsResult.Invalid([.. errors]);
        }

        var token = request.PlexToken is null ? current.PlexToken : request.PlexToken.Trim();
        await _store.SaveAsync(new AppSettingsSnapshot(
            plexUrl, token, mediaRoot, syncInterval, catalogInterval));

        return UpdateAppSettingsResult.Ok();
    }
}
