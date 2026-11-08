using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlexAutoplay.Infrastructure;

public class PlexAutoSkipBackgroundService : BackgroundService
{
    private readonly ILogger<PlexAutoSkipBackgroundService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppSettingsStore _settings;
    private readonly IMarkerRepository _markerRepository;

    public PlexAutoSkipBackgroundService(ILogger<PlexAutoSkipBackgroundService> logger,
        IHttpClientFactory httpClientFactory, IAppSettingsStore settings, IMarkerRepository markerRepository)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _markerRepository = markerRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlexAutoSkipBackgroundService started - polling every 2s");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndSkipAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PollAndSkipAsync cycle");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }

    private async Task PollAndSkipAsync(CancellationToken ct)
    {
        var settings = await _settings.GetAsync();
        var plexUrl = settings.PlexUrl.TrimEnd('/');
        using var http = _httpClientFactory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, $"{plexUrl}/status/sessions");
        req.Headers.Add("X-Plex-Token", settings.PlexToken);
        var resp = await http.SendAsync(req, ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Plex /status/sessions returned {StatusCode}", resp.StatusCode);
            return;
        }

        var xml = await resp.Content.ReadAsStringAsync(ct);
        var session = ParsePlayingSession(xml);
        if (session is null) return;

        var markers = (await _markerRepository.GetByRatingKeyAsync(session.RatingKey.ToString()))
            .Select(m => new MarkerInfo(m.Kind, m.StartMs, m.EndMs))
            .ToList();

        foreach (var marker in markers)
        {
            if (session.PositionMs >= marker.StartMs && session.PositionMs <= marker.EndMs)
            {
                _logger.LogInformation("Auto-skip trigger: pos={Pos}ms in marker [{Start},{End}] {Kind} ratingKey={RK}",
                    session.PositionMs, marker.StartMs, marker.EndMs, marker.Kind, session.RatingKey);
                await SeekToEndOfMarkerAsync(session, marker, plexUrl, settings.PlexToken, ct);
                break;
            }
        }
    }

    private static PlexSessionInfo? ParsePlayingSession(string xml)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xml);
            var video = doc.Descendants("Video").FirstOrDefault(v =>
                v.Attribute("viewOffset") != null &&
                v.Attribute("ratingKey") != null &&
                v.Attribute("type")?.Value == "episode");

            if (video == null) return null;

            int viewOffset = int.Parse(video.Attribute("viewOffset")!.Value);
            int ratingKey = int.Parse(video.Attribute("ratingKey")!.Value);

            var player = video.Elements("Player").FirstOrDefault(p => p.Attribute("state")?.Value == "playing");
            if (player == null) return null;

            string clientIp = player.Attribute("address")?.Value ?? string.Empty;
            string machineId = player.Attribute("machineIdentifier")?.Value ?? string.Empty;
            string product = player.Attribute("product")?.Value ?? string.Empty;
            string platform = player.Attribute("platform")?.Value ?? string.Empty;
            string version = player.Attribute("version")?.Value ?? string.Empty;

            // Map known client products to their companion control ports (from PlexAutoSkip)
            int clientPort = GetClientPort(product);

            return new PlexSessionInfo
            {
                PositionMs = viewOffset,
                RatingKey = ratingKey,
                ClientIp = clientIp,
                ClientPort = clientPort,
                MachineIdentifier = machineId,
                Product = product,
                Platform = platform,
                Version = version
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ParsePlayingSession error: {ex.Message}");
            return null;
        }
    }

    private static int GetClientPort(string product)
    {
        return product switch
        {
            "Plex for Roku" => 8324,
            "Plex for Android (TV)" => 32500,
            "Plex for Android (Mobile)" => 32500,
            "Plex for iOS" => 32500,
            "Plex for Apple TV" => 32500,
            "Plex for Windows" => 32700,
            "Plex for Mac" => 32700,
            _ => 0 // Unknown client — will use PMS proxy
        };
    }

    private static bool IsBrokenClient(string product, string version)
    {
        // PlexAutoSkip: these clients removed "Advertise as Player" / Plex Companion functionality
        if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(version)) return false;
        var v = version.Split('-')[0]; // strip build suffix
        return product switch
        {
            "Plex Web" => VersionCompare(v, "4.83.2") >= 0,
            "Plex for Windows" => VersionCompare(v, "1.46.1") >= 0,
            "Plex for Mac" => VersionCompare(v, "1.46.1") >= 0,
            "Plex for Linux" => VersionCompare(v, "1.46.1") >= 0,
            _ => false
        };
    }

    private static int VersionCompare(string v1, string v2)
    {
        try
        {
            var a = new Version(v1);
            var b = new Version(v2);
            return a.CompareTo(b);
        }
        catch { return 0; }
    }

    private async Task SeekToEndOfMarkerAsync(PlexSessionInfo session, MarkerInfo marker, string plexUrl, string plexToken, CancellationToken ct)
    {
        string commandId = Guid.NewGuid().ToString("N")[..12];

        // Skip known-broken clients (no Plex Companion support) — don't spam errors
        if (IsBrokenClient(session.Product, session.Version))
        {
            _logger.LogInformation("Auto-skip: client {Product} {Version} lacks Plex Companion support, skipping seek (marker {Kind} ->{Ms}ms)",
                session.Product, session.Version, marker.Kind, marker.EndMs);
            return;
        }

        // 1) Direct client HTTP (preferred — works for Android, iOS, AppleTV, Roku, Windows/Mac desktop)
        if (!string.IsNullOrEmpty(session.ClientIp) && session.ClientPort > 0)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var seekUrl = $"http://{session.ClientIp}:{session.ClientPort}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId}&X-Plex-Token={Uri.EscapeDataString(plexToken)}";
                var response = await httpClient.GetAsync(seekUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Seek via direct client ok ({Product} {Ip}:{Port}, ->{Ms}ms)", session.Product, session.ClientIp, session.ClientPort, marker.EndMs);
                    return;
                }
                _logger.LogDebug("Direct client seek returned {StatusCode} ({Product} {Ip}:{Port})", response.StatusCode, session.Product, session.ClientIp, session.ClientPort);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Direct client seek failed ({Product} {Ip}:{Port})", session.Product, session.ClientIp, session.ClientPort);
            }
        }

        // 2) PMS proxy with X-Plex-Target-Client-Identifier (fallback for clients that proxy through server)
        if (!string.IsNullOrEmpty(session.MachineIdentifier))
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var proxyUrl = $"{plexUrl}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId}&type=video";
                var req = new HttpRequestMessage(HttpMethod.Get, proxyUrl);
                req.Headers.Add("X-Plex-Token", plexToken);
                req.Headers.Add("X-Plex-Target-Client-Identifier", session.MachineIdentifier);
                var response = await httpClient.SendAsync(req, ct);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Seek via PMS proxy ok (machineId={Mi}, ->{Ms}ms)", session.MachineIdentifier, marker.EndMs);
                    return;
                }
                _logger.LogDebug("PMS proxy seek returned {StatusCode} (machineId={Mi})", response.StatusCode, session.MachineIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "PMS proxy seek failed (machineId={Mi})", session.MachineIdentifier);
            }
        }

        _logger.LogWarning("No viable seek path for {Product} (ip={Ip}, port={Port}, machineId={Mi})",
            session.Product, session.ClientIp, session.ClientPort, session.MachineIdentifier);
    }
}

public class PlexSessionInfo
{
    public int PositionMs { get; set; }
    public int RatingKey { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public int ClientPort { get; set; }
    public string MachineIdentifier { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}