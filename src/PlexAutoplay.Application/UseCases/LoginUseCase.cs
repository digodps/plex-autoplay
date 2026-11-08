namespace PlexAutoplay.Application.UseCases;

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
}

public class LoginUseCase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthCredentialStore _credentials;
    private readonly IPasswordHasher _hasher;

    public LoginUseCase(
        ITokenService tokenService,
        IAuthCredentialStore credentials,
        IPasswordHasher hasher)
    {
        _tokenService = tokenService;
        _credentials = credentials;
        _hasher = hasher;
    }

    public async Task<LoginResult> TryGenerateAsync(string password)
    {
        var result = new LoginResult();
        
        var isConfigured = await _credentials.IsConfiguredAsync();
        if (!isConfigured)
        {
            return result;
        }
        
        var hash = await _credentials.GetHashAsync();
        if (hash is null)
        {
            return result;
        }
        
        if (_hasher.Verify(password, hash))
        {
            result.Success = true;
            result.Token = _tokenService.GenerateToken();
        }
        
        return result;
    }
}
