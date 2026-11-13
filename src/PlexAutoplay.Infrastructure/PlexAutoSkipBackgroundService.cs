using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.AutoSkip;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlexAutoplay.Infrastructure;

public class PlexAutoSkipBackgroundService : BackgroundService
{
    private readonly ILogger<PlexAutoSkipBackgroundService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppSettingsStore _settings;
    private readonly IMarkerRepository _markerRepository;
    private readonly IClientSeekStrategyProvider _strategyProvider;
    private readonly ISkipMonitor _monitor;
    private readonly ISkipHistoryRepository _skipHistory;

    // Cooldown: evita loop de re-seek no mesmo marcador.
    // Key = "ratingKey|kind", Value = EndMs que pulamos.
    // Se position <= EndMs registrado, não re-seeka (já pulamos este marcador).
    private static readonly ConcurrentDictionary<string, long> _lastSkipEndMs = new();

    public PlexAutoSkipBackgroundService(
        ILogger<PlexAutoSkipBackgroundService> logger,
        IHttpClientFactory httpClientFactory,
        IAppSettingsStore settings,
        IMarkerRepository markerRepository,
        IClientSeekStrategyProvider strategyProvider,
        ISkipMonitor monitor,
        ISkipHistoryRepository skipHistory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _markerRepository = markerRepository;
        _strategyProvider = strategyProvider;
        _monitor = monitor;
        _skipHistory = skipHistory;
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
        if (session is null)
        {
            _logger.LogDebug("No playable video session found in Plex /status/sessions response");
            _monitor.UpdateSessions(System.Array.Empty<SessionStatus>());
            return;
        }

        var markers = (await _markerRepository.GetByRatingKeyAsync(session.RatingKey.ToString()))
            .Select(m => new MarkerInfo(m.Kind, m.StartMs, m.EndMs))
            .ToList();

        var upcoming = markers.Where(m => session.PositionMs < m.StartMs)
            .OrderBy(m => m.StartMs)
            .ToList();
        var target = upcoming.Count > 0
            ? new MarkerTarget(upcoming[0].Kind, upcoming[0].StartMs, upcoming[0].EndMs,
                Math.Max(0, upcoming[0].StartMs - session.PositionMs))
            : null;

        _monitor.UpdateSessions(new[]
        {
            new SessionStatus(
                session.MachineIdentifier,
                session.Product,
                session.Platform,
                session.RatingKey,
                session.PositionMs,
                session.DurationMs,
                "playing",
                target)
        });

        foreach (var marker in markers)
        {
            var cooldownKey = $"{session.RatingKey}|{marker.Kind}";
            var inMarker = session.PositionMs >= marker.StartMs && session.PositionMs <= marker.EndMs;

            if (!inMarker)
            {
                // Fora do marcador (passou do fim ou voltou antes do início) → nova passagem permitida
                _lastSkipEndMs.TryRemove(cooldownKey, out _);
                continue;
            }

            // Cooldown: já pulamos este marcador nesta passagem → não re-seeka
            if (_lastSkipEndMs.ContainsKey(cooldownKey))
            {
                continue;
            }

            _logger.LogInformation("Auto-skip trigger: pos={Pos}ms in marker [{Start},{End}] {Kind} ratingKey={RK}",
                session.PositionMs, marker.StartMs, marker.EndMs, marker.Kind, session.RatingKey);
            var success = await SeekToEndOfMarkerAsync(session, marker, plexUrl, settings.PlexToken, ct);
            if (success)
            {
                _lastSkipEndMs[cooldownKey] = marker.EndMs;
                var entry = new SkipHistoryEntry(
                    DateTime.UtcNow,
                    session.MachineIdentifier,
                    session.Product,
                    session.Platform,
                    session.RatingKey,
                    marker.Kind,
                    session.PositionMs,
                    marker.EndMs);
                _monitor.RecordSkip(entry);
                try
                {
                    await _skipHistory.AddAsync(entry);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao persistir histórico de skip ratingKey={RK}", session.RatingKey);
                }
                break; // apenas um skip por ciclo
            }
        }
    }

    private PlexSessionInfo? ParsePlayingSession(string xml)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xml);
            var video = doc.Descendants("Video").FirstOrDefault(v =>
                v.Attribute("viewOffset") != null &&
                v.Attribute("ratingKey") != null);

            if (video == null) return null;

            int viewOffset = int.Parse(video.Attribute("viewOffset")!.Value);
            int ratingKey = int.Parse(video.Attribute("ratingKey")!.Value);
            long durationMs = long.TryParse(video.Attribute("duration")?.Value, out var d) ? d : 0;

            var player = video.Elements("Player").FirstOrDefault(p => p.Attribute("state")?.Value == "playing");
            if (player == null) return null;

            return new PlexSessionInfo
            {
                PositionMs = viewOffset,
                DurationMs = durationMs,
                RatingKey = ratingKey,
                ClientIp = player.Attribute("address")?.Value ?? string.Empty,
                MachineIdentifier = player.Attribute("machineIdentifier")?.Value ?? string.Empty,
                Product = player.Attribute("product")?.Value ?? string.Empty,
                Platform = player.Attribute("platform")?.Value ?? string.Empty,
                Version = player.Attribute("version")?.Value ?? string.Empty,
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ParsePlayingSession error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verifica se uma resposta seek foi bem-sucedida. Trata respostas não-XML
    /// (corpo vazio, "OK" ou XML parcial com code="200"), comum em clientes modernos
    /// como Samsung, Android e Plexamp que não retornam XML completo em comandos OK.
    /// </summary>
    internal static async Task<bool> IsSeekSuccessAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (!response.IsSuccessStatusCode) return false;

        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrEmpty(body)) return true;

        var trimmed = body.Trim();
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return trimmed.Contains("code=\"200\"", StringComparison.OrdinalIgnoreCase)
                   || trimmed.Contains("status=\"OK\"", StringComparison.OrdinalIgnoreCase)
                   || trimmed.Contains("code=\"OK\"", StringComparison.OrdinalIgnoreCase);
        }

        return trimmed == "OK"
               || trimmed.Length > 4 && trimmed.StartsWith("OK;", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> SeekToEndOfMarkerAsync(PlexSessionInfo session, MarkerInfo marker, string plexUrl, string plexToken, CancellationToken ct)
    {
        string commandId = Guid.NewGuid().ToString("N")[..12];
        var strategy = _strategyProvider.Detect(session.Product, session.Platform, session.Version);

        // 1) Direto no cliente (LegacyCompanion com porta conhecida)
        if (strategy.SupportsDirectConnection
            && !string.IsNullOrEmpty(session.ClientIp)
            && strategy.DirectPort > 0)
        {
            _logger.LogDebug("Trying direct seek for {Product} ({Ip}:{Port})", session.Product, session.ClientIp, strategy.DirectPort);
            var success = await SeekDirectAsync(session, marker, plexToken, commandId, strategy, ct);
            if (success) return true;
        }

        // 2) PMS proxy (apenas para Smart TVs e demais clientes que anunciam Player)
        if (strategy.SupportsPmsProxy
            && !string.IsNullOrEmpty(session.MachineIdentifier))
        {
            _logger.LogDebug("Trying PMS proxy seek for {Product} (machineId={Mid})", session.Product, session.MachineIdentifier);
            var success = await SeekViaPmsProxyAsync(session, marker, plexUrl, plexToken, commandId, strategy, ct);
            if (success) return true;
        }

        // Cliente web/desktop moderno não anuncia Player → Plex API não controla
        if (!strategy.SupportsDirectConnection && !strategy.SupportsPmsProxy)
            _logger.LogInformation("Skip não suportado para {Product} — {Platform} ({Version}): cliente não expõe Player nem PMS proxy.", session.Product, session.Platform, session.Version);
        else
            _logger.LogWarning("All seek strategies failed for {Product} {Version}", session.Product, session.Version);

        return false;
    }

    private async Task<bool> SeekDirectAsync(PlexSessionInfo session, MarkerInfo marker, string plexToken, string commandId, ClientSeekStrategy strategy, CancellationToken ct)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var seekUrl = $"http://{session.ClientIp}:{strategy.DirectPort}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId}&X-Plex-Token={Uri.EscapeDataString(plexToken)}";
            var response = await httpClient.GetAsync(seekUrl, ct);

            if (await IsSeekSuccessAsync(response))
            {
                _logger.LogInformation("Seek via direct client ok ({Product} {Ip}:{Port}, ->{Ms}ms)", session.Product, session.ClientIp, strategy.DirectPort, marker.EndMs);
                return true;
            }

            _logger.LogDebug("Direct client seek returned {StatusCode} ({Product} {Ip}:{Port})", response.StatusCode, session.Product, session.ClientIp, strategy.DirectPort);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Direct client seek failed ({Product} {Ip}:{Port})", session.Product, session.ClientIp, strategy.DirectPort);
        }
        return false;
    }

    private async Task<bool> SeekViaPmsProxyAsync(PlexSessionInfo session, MarkerInfo marker, string plexUrl, string plexToken, string commandId, ClientSeekStrategy strategy, CancellationToken ct)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var seekUrl = $"{plexUrl}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId}&type=video";
            var req = new HttpRequestMessage(HttpMethod.Get, seekUrl);
            req.Headers.Add("X-Plex-Token", plexToken);
            req.Headers.Add("X-Plex-Target-Client-Identifier", session.MachineIdentifier);

            var response = await httpClient.SendAsync(req, ct);

            if (await IsSeekSuccessAsync(response))
            {
                _logger.LogInformation("Seek via PMS proxy ok ({Product} {Mid}, ->{Ms}ms)", session.Product, session.MachineIdentifier, marker.EndMs);
                return true;
            }

            _logger.LogDebug("PMS proxy seek returned {StatusCode} ({Product} {Mid})", response.StatusCode, session.Product, session.MachineIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PMS proxy seek failed ({Product} {Mid})", session.Product, session.MachineIdentifier);
        }
        return false;
    }
}

internal class PlexSessionInfo
{
    public int PositionMs { get; set; }
    public long DurationMs { get; set; }
    public int RatingKey { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public string MachineIdentifier { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}