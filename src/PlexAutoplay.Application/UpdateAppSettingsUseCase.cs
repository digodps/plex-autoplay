namespace PlexAutoplay.Application;

/// <summary>Valores parciais aceitos por PUT /api/settings.</summary>
public sealed record UpdateAppSettingsRequest(
    string? PlexUrl,
    string? PlexToken,
    IReadOnlyList<string>? MediaRoots,
    int? SyncIntervalMinutes);

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

        var (plexUrl, plexUrlError) = PlexUrlRules.Normalize(request.PlexUrl ?? current.PlexUrl);
        if (plexUrlError is not null)
        {
            errors.Add(plexUrlError);
        }

        var mediaRoots = current.MediaRoots;
        if (request.MediaRoots is not null)
        {
            var (roots, rootsErrors) = MediaRoots.Normalize(request.MediaRoots);
            errors.AddRange(rootsErrors);
            mediaRoots = roots;
        }

        var syncInterval = request.SyncIntervalMinutes ?? current.SyncIntervalMinutes;
        if (syncInterval is < MinInterval or > MaxInterval)
        {
            errors.Add($"sync_interval_minutes deve estar entre {MinInterval} e {MaxInterval}");
        }

        if (errors.Count > 0)
        {
            return UpdateAppSettingsResult.Invalid([.. errors]);
        }

        var token = request.PlexToken is null ? current.PlexToken : request.PlexToken.Trim();
        await _store.SaveAsync(new AppSettingsSnapshot(plexUrl, token, mediaRoots, syncInterval));

        return UpdateAppSettingsResult.Ok();
    }
}