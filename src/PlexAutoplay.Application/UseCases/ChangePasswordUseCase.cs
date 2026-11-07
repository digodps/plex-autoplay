namespace PlexAutoplay.Application.UseCases;

public enum ChangePasswordResult
{
    Success,
    InvalidPassword,
    WrongCurrentPassword,
    NotConfigured
}

/// <summary>
/// Troca a senha do operador autenticado após validar a senha atual.
/// </summary>
public class ChangePasswordUseCase
{
    private readonly IAuthCredentialStore _credentials;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordUseCase(IAuthCredentialStore credentials, IPasswordHasher hasher)
    {
        _credentials = credentials;
        _hasher = hasher;
    }

    public async Task<ChangePasswordResult> ExecuteAsync(string? currentPassword, string? newPassword)
    {
        if (!PasswordPolicy.IsValid(newPassword))
        {
            return ChangePasswordResult.InvalidPassword;
        }

        var hash = await _credentials.GetHashAsync();
        if (hash is null)
        {
            return ChangePasswordResult.NotConfigured;
        }

        if (!_hasher.Verify(currentPassword ?? "", hash))
        {
            return ChangePasswordResult.WrongCurrentPassword;
        }

        var updated = await _credentials.UpdateAsync(_hasher.Hash(newPassword!));
        return updated ? ChangePasswordResult.Success : ChangePasswordResult.NotConfigured;
    }
}
