namespace PlexAutoplay.Application.UseCases;

public class LoginUseCase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthCredentialStore _credentials;
    private readonly IPasswordHasher _hasher;

    public LoginUseCase(ITokenService tokenService, IAuthCredentialStore credentials, IPasswordHasher hasher)
    {
        _tokenService = tokenService;
        _credentials = credentials;
        _hasher = hasher;
    }

    public async Task<(bool Success, string? Token)> TryGenerateAsync(string password)
    {
        var hash = await _credentials.GetHashAsync();
        if (hash is null || !_hasher.Verify(password, hash))
        {
            return (false, null);
        }

        return (true, _tokenService.GenerateToken());
    }
}
