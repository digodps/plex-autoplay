namespace PlexAutoplay.Application.UseCases;

public enum SetupStatus
{
    Success,
    InvalidToken,
    InvalidPassword,
    InvalidSettings,
    AlreadyConfigured
}

public sealed record CompleteSetupRequest(
    string? Token,
    string? NewPassword,
    string? PlexUrl,
    string? PlexToken,
    IReadOnlyList<string>? MediaRoots);

public sealed record CompleteSetupResult(SetupStatus Status, IReadOnlyList<string> Errors)
{
    public static CompleteSetupResult Ok() => new(SetupStatus.Success, []);
    public static CompleteSetupResult Fail(SetupStatus status, params string[] errors) => new(status, errors);
}

/// <summary>
/// Conclui o primeiro acesso: valida token de uso único, credenciais do Plex e
/// diretórios de mídia, persiste a configuração e só então cria a senha do operador.
/// A ordem garante que uma falha nunca deixe credencial criada sem configuração.
/// </summary>
public class CompleteSetupUseCase
{
    private static readonly TimeSpan PlexTimeout = TimeSpan.FromSeconds(10);

    private readonly IAuthCredentialStore _credentials;
    private readonly IPasswordHasher _hasher;
    private readonly ISetupTokenValidator _setupTokens;
    private readonly IAppSettingsStore _settings;
    private readonly IPlexApiClient _plexClient;

    public CompleteSetupUseCase(
        IAuthCredentialStore credentials,
        IPasswordHasher hasher,
        ISetupTokenValidator setupTokens,
        IAppSettingsStore settings,
        IPlexApiClient plexClient)
    {
        _credentials = credentials;
        _hasher = hasher;
        _setupTokens = setupTokens;
        _settings = settings;
        _plexClient = plexClient;
    }

    public async Task<CompleteSetupResult> ExecuteAsync(CompleteSetupRequest request)
    {
        if (await _credentials.IsConfiguredAsync())
        {
            return CompleteSetupResult.Fail(SetupStatus.AlreadyConfigured, "Configuração inicial já concluída");
        }

        if (!_setupTokens.TryValidate(request.Token ?? ""))
        {
            return CompleteSetupResult.Fail(SetupStatus.InvalidToken, "Token de setup inválido ou já utilizado");
        }

        if (!PasswordPolicy.IsValid(request.NewPassword))
        {
            return CompleteSetupResult.Fail(
                SetupStatus.InvalidPassword,
                $"A senha deve ter entre {PasswordPolicy.MinLength} e {PasswordPolicy.MaxLength} caracteres");
        }

        var (plexUrl, plexUrlError) = PlexUrlRules.Normalize(request.PlexUrl);
        var (mediaRoots, mediaRootErrors) = MediaRoots.Normalize(request.MediaRoots);

        var errors = new List<string>();
        if (plexUrlError is not null) errors.Add(plexUrlError);
        errors.AddRange(mediaRootErrors);
        if (string.IsNullOrWhiteSpace(request.PlexToken))
        {
            errors.Add("plex_token: informe o token de acesso do Plex");
        }

        if (errors.Count > 0)
        {
            return new CompleteSetupResult(SetupStatus.InvalidSettings, [.. errors]);
        }

        try
        {
            var identity = await _plexClient
                .GetIdentityAsync(plexUrl, request.PlexToken!.Trim())
                .WaitAsync(PlexTimeout);

            if (identity.Failed)
            {
                return CompleteSetupResult.Fail(
                    SetupStatus.InvalidSettings,
                    $"plex_url: não foi possível validar a conexão com o Plex ({identity.ErrorMessage})");
            }
        }
        catch (TimeoutException)
        {
            return CompleteSetupResult.Fail(
                SetupStatus.InvalidSettings,
                "plex_url: o Plex não respondeu em 10 segundos");
        }

        var current = await _settings.GetAsync();
        await _settings.SaveAsync(new AppSettingsSnapshot(
            plexUrl,
            request.PlexToken.Trim(),
            mediaRoots,
            current.SyncIntervalMinutes,
            current.SyncCatalogIntervalMinutes,
            current.AutoSkipAuthorized));

        var created = await _credentials.CreateAsync(_hasher.Hash(request.NewPassword!));
        if (!created)
        {
            return CompleteSetupResult.Fail(SetupStatus.AlreadyConfigured, "Configuração inicial já concluída");
        }

        _setupTokens.Invalidate();
        return CompleteSetupResult.Ok();
    }
}
