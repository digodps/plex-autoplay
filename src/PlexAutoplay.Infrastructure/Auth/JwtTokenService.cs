using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PlexAutoplay.Application;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlexAutoplay.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly string _secret;
    private readonly SymmetricSecurityKey _key;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(string secret, ILogger<JwtTokenService> logger)
    {
        _secret = secret;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        _logger = logger;
    }

    public string GenerateToken()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "plex-autoplay",
            audience: "plex-autoplay",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "plex-autoplay",
                ValidAudience = "plex-autoplay",
                IssuerSigningKey = _key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return true;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token JWT inválido");
            return false;
        }
    }
}
