using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace PlexAutoplay.Web.Configuration;

/// <summary>
/// Resolve o certificado TLS do servidor a partir de variáveis de ambiente.
/// Prioridade: CERT_BASE64 > CERT_PATH > certificado auto-assinado.
/// Encerra o processo em caso de certificado inválido ou expirado.
/// </summary>
public static class ServerCertificateProvider
{
    public static X509Certificate2 GetServerCertificate()
    {
        var certBase64 = Environment.GetEnvironmentVariable("CERT_BASE64");
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
        var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD");

        X509Certificate2? serverCert = null;

        if (!string.IsNullOrEmpty(certBase64))
        {
            serverCert = LoadFromBase64(certBase64, certPassword);
        }
        else if (!string.IsNullOrEmpty(certPath))
        {
            if (!File.Exists(certPath))
            {
                Log.Fatal("CERT_PATH aponta para arquivo inexistente: {CertPath}", certPath);
                Environment.Exit(1);
            }

            serverCert = LoadFromFile(certPath, certPassword);
        }

        if (serverCert != null && serverCert.NotAfter < DateTimeOffset.Now)
        {
            Log.Fatal("Certificado fornecido está expirado: {NotAfter}", serverCert.NotAfter);
            Environment.Exit(1);
        }

        if (serverCert == null)
        {
            Log.Information("Nenhum certificado configurado via CERT_PATH ou CERT_BASE64. Gerando certificado auto-assinado...");
            serverCert = GenerateSelfSignedCertificate();
            Log.Information("Certificado auto-assinado gerado: {Subject} (expire: {NotAfter})", serverCert.Subject, serverCert.NotAfter);
        }

        return serverCert;
    }

    private static X509Certificate2 LoadFromBase64(string certBase64, string? certPassword)
    {
        try
        {
            var certBytes = Convert.FromBase64String(certBase64);
            var cert = string.IsNullOrEmpty(certPassword)
                ? new X509Certificate2(certBytes)
                : new X509Certificate2(certBytes, certPassword);
            Log.Information("Certificado carregado de CERT_BASE64: {Subject} (expire: {NotAfter})", cert.Subject, cert.NotAfter);
            return cert;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "CERT_BASE64 inválido: não é um PFX válido ou não pôde ser decodificado");
            Environment.Exit(1);
            throw; // unreachable; satisfaz o compilador
        }
    }

    private static X509Certificate2 LoadFromFile(string certPath, string? certPassword)
    {
        try
        {
            var cert = string.IsNullOrEmpty(certPassword)
                ? new X509Certificate2(certPath)
                : new X509Certificate2(certPath, certPassword);
            Log.Information("Certificado carregado de CERT_PATH: {Subject} (expire: {NotAfter})", cert.Subject, cert.NotAfter);
            return cert;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "CERT_PATH inválido: não é um PFX válido: {CertPath}", certPath);
            Environment.Exit(1);
            throw; // unreachable; satisfaz o compilador
        }
    }

    private static X509Certificate2 GenerateSelfSignedCertificate()
    {
        using var ecdsa = ECDsa.Create(ECCurve.CreateFromValue("1.2.840.10045.3.1.7")); // nistP256 / P-256
        var request = new CertificateRequest(new X500DistinguishedName("cn=plex-autoplay"), ecdsa, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // Server Authentication
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddIpAddress(System.Net.IPAddress.Loopback);
        request.CertificateExtensions.Add(sanBuilder.Build());

        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        return new X509Certificate2(cert);
    }
}
