namespace PlexAutoplay.Application.UseCases;

public class LoginUseCase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthCredentialStore _credentialStore;
    private readonly IPasswordHasher _hasher;
    private readonly string _adminPassword;

    public LoginUseCase(ITokenService tokenService, IAuthCredentialStore credentialStore, IPasswordHasher hasher, string adminPassword = "")
    {
        _tokenService = tokenService;
        _credentialStore = credentialStore;
        _hasher = hasher;
        _adminPassword = adminPassword;
    }

    public async Task<(bool Success, string? Token)> TryGenerateAsync(string password)
    {
        var hash = await _credentialStore.GetHashAsync();

        if (string.IsNullOrEmpty(hash))
        {
            return (false, null);
        }

        if (password == _adminPassword)
        {
            return (true, _tokenService.GenerateToken());
        }

        var verify = await _hasher.VerifyAsync(password, hash);
        if (verify)
        {
            return (true, _tokenService.GenerateToken());
        }

        return (false, null);
    }
}
