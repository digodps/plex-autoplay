using System;

namespace PlexAutoplay.Infrastructure.AutoSkip;

/// <summary>
/// Classifica um cliente Plex a partir dos atributos expostos em <c>/status/sessions</c>
/// e define a estratégia de seek apropriada. Substitui a lista hardcoded de versões "quebradas".
/// </summary>
public class PlexModernClientDetector : IClientSeekStrategyProvider
{
    // Plex removeu "Advertise as Player" / Plex Companion a partir destas versões.
    private const string WebCompanionRemovedVersion = "4.83.2";
    private const string DesktopCompanionRemovedVersion = "1.46.1";

    private static readonly string[] SmartTvMarkers =
        ["Samsung", "LG", "Vizio", "Hisense", "Tizen", "webOS"];

    /// <summary>
    /// Detecta a estratégia de seek para o cliente informado.
    /// </summary>
    public ClientSeekStrategy Detect(string product, string platform, string version)
    {
        product ??= string.Empty;
        platform ??= string.Empty;
        version ??= string.Empty;

        if (IsPlexWeb(product))
            return new ClientSeekStrategy(ClientSeekCategory.ModernWeb, false, 0, false, false);

        if (IsSmartTv(product, platform))
            return new ClientSeekStrategy(ClientSeekCategory.SmartTV, false, 0, true, true);

        if (IsModernDesktop(product) && VersionAtLeast(version, DesktopCompanionRemovedVersion))
            return new ClientSeekStrategy(ClientSeekCategory.ModernDesktop, false, 0, false, false);

        if (LegacyCompanionPort(product, version, out int port, out bool nonXml))
            return new ClientSeekStrategy(ClientSeekCategory.LegacyCompanion, true, port, true, nonXml);

        return new ClientSeekStrategy(ClientSeekCategory.Unknown, false, 0, true, false);
    }

    private static bool IsPlexWeb(string product) =>
        product == "Plex Web"
        || product.Contains("Chrome", StringComparison.OrdinalIgnoreCase)
        || product.Contains("Firefox", StringComparison.OrdinalIgnoreCase)
        || product.Contains("Safari", StringComparison.OrdinalIgnoreCase)
        || product.Contains("Edge", StringComparison.OrdinalIgnoreCase);

    private static bool IsSmartTv(string product, string platform)
    {
        foreach (var marker in SmartTvMarkers)
        {
            if (product.Contains(marker, StringComparison.OrdinalIgnoreCase)
                || platform.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool IsModernDesktop(string product) =>
        product is "Plex for Windows" or "Plex for Mac" or "Plex for Linux";

    /// <summary>
    /// Mapeia clientes que ainda suportam companion/direto. Retorna <c>false</c> quando não é legado.
    /// </summary>
    private static bool LegacyCompanionPort(string product, string version, out int port, out bool nonXml)
    {
        nonXml = false;
        port = product switch
        {
            "Plex for Roku" => 8324,
            "Plex for Android (TV)" => 32500,
            "Plex for Android (Mobile)" => 32500,
            "Plex for iOS" => 32500,
            "Plex for Apple TV" => 32500,
            // Desktop antigo (antes de removerem o companion) ainda escuta na 32700.
            "Plex for Windows" => 32700,
            "Plex for Mac" => 32700,
            "Plex for Linux" => 32700,
            _ => 0
        };

        if (port == 0) return false;

        // Android e Samsung retornam resposta não-XML em comandos bem-sucedidos.
        nonXml = product is "Plex for Android (TV)" or "Plex for Android (Mobile)";
        return true;
    }

    /// <summary>Compara versões no formato <c>major.minor.patch</c> (sufixo de build ignorado).</summary>
    private static bool VersionAtLeast(string version, string minimum)
    {
        var normalized = version.Split('-')[0].Trim();
        try
        {
            return Version.Parse(normalized) >= Version.Parse(minimum);
        }
        catch
        {
            return false;
        }
    }
}