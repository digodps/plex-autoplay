using PlexAutoplay.Infrastructure;
using PlexAutoplay.Infrastructure.AutoSkip;
using System.Net;
using System.Net.Http;

namespace PlexAutoplay.UnitTests;

/// <summary>
/// Verifica detecção dinâmica de capacidade de seek de clientes Plex modernos,
/// substituindo a lista hardcoded de versões "quebradas".
/// </summary>
public class PlexModernClientDetectorTests
{
    private readonly PlexModernClientDetector _detector = new();

    [Fact]
    public void ModernWeb_Returns_ProxyOnly()
    {
        var s = _detector.Detect("Plex Web", "Windows", "4.90.0");
        Assert.Equal(ClientSeekCategory.ModernWeb, s.Category);
        Assert.False(s.SupportsDirectConnection);
        Assert.Equal(0, s.DirectPort);
        Assert.False(s.SupportsPmsProxy);
        Assert.False(s.ExpectsNonXmlResponse);
    }

    [Fact]
    public void ModernDesktop_Returns_ProxyOnly_WhenVersionAtLeastThreshold()
    {
        var s = _detector.Detect("Plex for Windows", "Windows", "1.46.1");
        Assert.Equal(ClientSeekCategory.ModernDesktop, s.Category);
        Assert.False(s.SupportsDirectConnection);
        Assert.Equal(0, s.DirectPort);
        Assert.False(s.SupportsPmsProxy);
    }

    [Fact]
    public void SmartTv_Returns_ProxyOnly_NonXml()
    {
        var s = _detector.Detect("Plex for Samsung", "Tizen", "1.0.0");
        Assert.Equal(ClientSeekCategory.SmartTV, s.Category);
        Assert.False(s.SupportsDirectConnection);
        Assert.Equal(0, s.DirectPort);
        Assert.True(s.SupportsPmsProxy);
        Assert.True(s.ExpectsNonXmlResponse);
    }

    [Fact]
    public void LegacyCompanion_Roku_Returns_Direct_8324()
    {
        var s = _detector.Detect("Plex for Roku", "Roku", "1.0.0");
        Assert.Equal(ClientSeekCategory.LegacyCompanion, s.Category);
        Assert.True(s.SupportsDirectConnection);
        Assert.Equal(8324, s.DirectPort);
        Assert.False(s.ExpectsNonXmlResponse);
    }

    [Fact]
    public void LegacyCompanion_AndroidTV_Returns_Direct_32500_NonXml()
    {
        var s = _detector.Detect("Plex for Android (TV)", "AndroidTV", "9.0.0");
        Assert.Equal(ClientSeekCategory.LegacyCompanion, s.Category);
        Assert.True(s.SupportsDirectConnection);
        Assert.Equal(32500, s.DirectPort);
        Assert.True(s.ExpectsNonXmlResponse);
    }

    [Fact]
    public void Unknown_Product_Falls_Back_To_Proxy()
    {
        var s = _detector.Detect("Plex for Xylophone", "XylophoneOS", "1.0.0");
        Assert.Equal(ClientSeekCategory.Unknown, s.Category);
        Assert.False(s.SupportsDirectConnection);
        Assert.Equal(0, s.DirectPort);
        Assert.True(s.SupportsPmsProxy);
    }

    [Fact]
    public void ModernWeb_Browser_Product_Maps_To_ModernWeb()
    {
        var s = _detector.Detect("Plex for Chrome", "Windows", "4.90.0");
        Assert.Equal(ClientSeekCategory.ModernWeb, s.Category);
        Assert.False(s.SupportsDirectConnection);
        Assert.False(s.SupportsPmsProxy);
    }
}

/// <summary>
/// Verifica tratamento de respostas não-XML de clients modernos (Samsung, Android, Plexamp).
/// </summary>
public class IsSeekSuccessTests
{
    [Fact]
    public async Task Ok_EmptyBody_Success()
    {
        using var content = new StringContent("");
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };
        Assert.True(await PlexAutoSkipBackgroundService.IsSeekSuccessAsync(resp));
    }

    [Fact]
    public async Task Ok_TextOk_Success()
    {
        using var content = new StringContent("OK");
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };
        Assert.True(await PlexAutoSkipBackgroundService.IsSeekSuccessAsync(resp));
    }

    [Fact]
    public async Task Ok_XmlPartial_Code200_Success()
    {
        using var content = new StringContent("<?xml version=\"1.0\"?><Response code=\"200\" status=\"OK\">");
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };
        Assert.True(await PlexAutoSkipBackgroundService.IsSeekSuccessAsync(resp));
    }

    [Fact]
    public async Task NotFound_Failure()
    {
        using var content = new StringContent("Not Found");
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = content };
        Assert.False(await PlexAutoSkipBackgroundService.IsSeekSuccessAsync(resp));
    }
}
