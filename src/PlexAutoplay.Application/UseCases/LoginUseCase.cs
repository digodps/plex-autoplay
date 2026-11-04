namespace PlexAutoplay.Application.UseCases;

public class LoginUseCase
{
    private readonly ITokenService _tokenService;
    private readonly string _adminPassword;

    public LoginUseCase(ITokenService tokenService, string adminPassword)
    {
        _tokenService = tokenService;
        _adminPassword = adminPassword;
    }

    public (bool Success, string? Token) TryGenerate(string password)
    {
        if (password == _adminPassword)
        {
            return (true, _tokenService.GenerateToken());
        }

        return (false, null);
    }
}
