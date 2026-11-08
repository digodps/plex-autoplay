namespace PlexAutoplay.Infrastructure.Auth;

using PlexAutoplay.Application;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Token de setup de uso único mantido em memória. O composition root escreve
/// o valor no log do container; Invalidar ocorre no primeiro setup bem-sucedido.
/// </summary>
public class SetupTokenService : ISetupTokenValidator
{
    private readonly object _gate = new();
    private string? _token;

    /// <summary>
    /// Gera o token apenas uma vez por processo. Usa TEST_MASTER_PASSWORD do ambiente
    /// quando presente (ambiente de teste, evita re-setup); usa SETUP_TOKEN do ambiente
    /// quando presente (hook de testes); caso contrário, 32 bytes aleatórios.
    /// </summary>
    public string EnsureGenerated()
    {
        lock (_gate)
        {
            if (_token is null)
            {
                var masterPassword = Environment.GetEnvironmentVariable("TEST_MASTER_PASSWORD");
                var setupToken = Environment.GetEnvironmentVariable("SETUP_TOKEN");

                if (!string.IsNullOrWhiteSpace(masterPassword) && string.IsNullOrWhiteSpace(setupToken))
                {
                    // Ambiente de teste: reusa a senha mestra como token de setup.
                    _token = masterPassword;
                }
                else
                {
                    var configured = string.IsNullOrWhiteSpace(setupToken) ? null : setupToken;
                    _token = string.IsNullOrWhiteSpace(configured)
                        ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                            .Replace('+', '-')
                            .Replace('/', '_')
                            .TrimEnd('=')
                        : configured;
                }
            }

            return _token;
        }
    }

    public bool TryValidate(string token)
    {
        lock (_gate)
        {
            if (_token is null || string.IsNullOrEmpty(token))
            {
                return false;
            }

            var expected = Encoding.UTF8.GetBytes(_token);
            var provided = Encoding.UTF8.GetBytes(token);
            return expected.Length == provided.Length &&
                   CryptographicOperations.FixedTimeEquals(expected, provided);
        }
    }

    public void Invalidate()
    {
        lock (_gate)
        {
            _token = null;
        }
    }
}
