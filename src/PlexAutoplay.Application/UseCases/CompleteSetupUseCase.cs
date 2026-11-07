namespace PlexAutoplay.Application.UseCases;

public enum CompleteSetupResult
{
    Success,
    InvalidToken,
    InvalidPassword,
    AlreadyConfigured
}

/// <summary>
/// Conclui o primeiro acesso: valida o token de uso único e cria a senha do operador.
/// </summary>
public class CompleteSetupUseCase
{
    private readonly IAuthCredentialStore _credentials;
    private readonly IPasswordHasher _hasher;
    private readonly ISetupTokenValidator _setupTokens;

    public CompleteSetupUseCase(
        IAuthCredentialStore credentials,
        IPasswordHasher hasher,
        ISetupTokenValidator setupTokens)
    {
        _credentials = credentials;
        _hasher = hasher;
        _setupTokens = setupTokens;
    }

    public async Task<CompleteSetupResult> ExecuteAsync(string? token, string? newPassword)
    {
        if (await _credentials.IsConfiguredAsync())
        {
            return CompleteSetupResult.AlreadyConfigured;
        }

        if (!_setupTokens.TryValidate(token ?? ""))
        {
            return CompleteSetupResult.InvalidToken;
        }

        if (!PasswordPolicy.IsValid(newPassword))
        {
            return CompleteSetupResult.InvalidPassword;
        }

        var created = await _credentials.CreateAsync(_hasher.Hash(newPassword!));
        if (!created)
        {
            return CompleteSetupResult.AlreadyConfigured;
        }

        _setupTokens.Invalidate();
        return CompleteSetupResult.Success;
    }
}
