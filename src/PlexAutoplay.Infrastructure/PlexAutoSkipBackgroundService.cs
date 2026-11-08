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
        var resp = await http.GetAsync($"{plexUrl}/status/stream", ct);

        if (!resp.IsSuccessStatusCode) return;

        var xml = await resp.Content.ReadAsStringAsync(ct);
        var viewOffset = ExtractViewOffset(xml);
        var ratingKey = ExtractRatingKey(xml);

        if (viewOffset <= 0) return;

        var markers = (await _markerRepository.GetByRatingKeyAsync(ratingKey.ToString())).Select(m => new MarkerInfo(m.Kind, m.StartMs, m.EndMs)).ToList();

        foreach (var marker in markers)
        {
            if (viewOffset >= marker.StartMs && viewOffset <= marker.EndMs)
            {
                await SeekToEndOfMarkerAsync(new PlexSessionInfo
                {
                    PositionMs = viewOffset,
                    RatingKey = ratingKey,
                    ClientIp = ExtractClientIp(xml),
                    ClientPort = ExtractClientPort(xml),
                    MachineIdentifier = ExtractMachineIdentifier(xml)
                }, marker, plexUrl, settings.PlexToken, ct);

                break;
            }
        }
    }

    private static int ExtractViewOffset(string xml)
    {
        try
        {
            var start = xml.IndexOf("<ViewOffset>") + "<ViewOffset>".Length;
            var end = xml.IndexOf("</ViewOffset>", start);
            if (start > "<ViewOffset>".Length && end > start)
                return int.Parse(xml.Substring(start, end - start));
        }
        catch { }
        return 0;
    }

    private static int ExtractRatingKey(string xml)
    {
        try
        {
            var start = xml.IndexOf("<RatingKey>") + "<RatingKey>".Length;
            var end = xml.IndexOf("</RatingKey>", start);
            if (start > "<RatingKey>".Length && end > start)
                return int.Parse(xml.Substring(start, end - start));
        }
        catch { }
        return 0;
    }

    private static string ExtractClientIp(string xml)
    {
        try
        {
            var start = xml.IndexOf("<Client>") + "<Client>".Length;
            var end = xml.IndexOf("</Client>", start);
            if (start > "<Client>".Length && end > start)
                return xml.Substring(start, end - start);
        }
        catch { }
        return string.Empty;
    }

    private static int ExtractClientPort(string xml)
    {
        try
        {
            var start = xml.IndexOf("<Port>") + "<Port>".Length;
            var end = xml.IndexOf("</Port>", start);
            if (start > "<Port>".Length && end > start)
                return int.Parse(xml.Substring(start, end - start));
        }
        catch { }
        return 0;
    }

    private static string ExtractMachineIdentifier(string xml)
    {
        try
        {
            var start = xml.IndexOf("<MachineIdentifier>") + "<MachineIdentifier>".Length;
            var end = xml.IndexOf("</MachineIdentifier>", start);
            if (start > "<MachineIdentifier>".Length && end > start)
                return xml.Substring(start, end - start);
        }
        catch { }
        return string.Empty;
    }

    private async Task SeekToEndOfMarkerAsync(PlexSessionInfo session, MarkerInfo marker, string plexUrl, string plexToken, CancellationToken ct)
    {
        _logger.LogInformation(
            "Auto-skip: position={Pos}ms within marker [{Start}ms, {End}ms] ({Reason}) for ratingKey={RK}",
            session.PositionMs, marker.StartMs, marker.EndMs, marker.Kind, session.RatingKey);

        // Step 1: Try PMS proxy first — goes through Plex Server, which Samsung TV accepts
        if (!string.IsNullOrEmpty(session.MachineIdentifier))
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var commandId = Guid.NewGuid().ToString();
                var proxyUrl = $"{plexUrl}/players/{session.MachineIdentifier}/commands/seek?offset={marker.EndMs}&commandID={commandId}";

                var response = await httpClient.GetAsync(proxyUrl, ct);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Seek sent via PMS proxy for machineId={Mi}", session.MachineIdentifier);
                }
                else
                {
                    _logger.LogWarning("PMS proxy seek returned {StatusCode}", response.StatusCode);
                    // Step 2: Fallback to direct client HTTP if PMS proxy fails
                    if (!string.IsNullOrEmpty(session.ClientIp) && session.ClientPort > 0)
                    {
                        using var httpClient2 = _httpClientFactory.CreateClient();
                        var commandId2 = Guid.NewGuid().ToString();
                        var seekUrl = $"http://{session.ClientIp}:{session.ClientPort}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId2}&X-Plex-Token={plexToken}";

                        var response2 = await httpClient2.GetAsync(seekUrl, ct);
                        if (response2.IsSuccessStatusCode)
                            _logger.LogInformation("Seek command sent to client {Ip} for marker skip", session.ClientIp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send seek via PMS proxy");
                // Step 2: Fallback to direct client HTTP if PMS proxy throws
                if (!string.IsNullOrEmpty(session.ClientIp) && session.ClientPort > 0)
                {
                    try
                    {
                        using var httpClient2 = _httpClientFactory.CreateClient();
                        var commandId2 = Guid.NewGuid().ToString();
                        var seekUrl = $"http://{session.ClientIp}:{session.ClientPort}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId2}&X-Plex-Token={plexToken}";

                        var response2 = await httpClient2.GetAsync(seekUrl, ct);
                        if (response2.IsSuccessStatusCode)
                            _logger.LogInformation("Seek command sent to client {Ip} for marker skip", session.ClientIp);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Failed to send seek to client {Ip}", session.ClientIp);
                    }
                }
            }
        }
        // Step 2: If no MachineIdentifier, try direct client HTTP
        else if (!string.IsNullOrEmpty(session.ClientIp) && session.ClientPort > 0)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var commandId = Guid.NewGuid().ToString();
                var seekUrl = $"http://{session.ClientIp}:{session.ClientPort}/player/playback/seekTo?offset={marker.EndMs}&commandID={commandId}&X-Plex-Token={plexToken}";

                var response = await httpClient.GetAsync(seekUrl, ct);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Seek command sent to client {Ip} for marker skip", session.ClientIp);
                }
                else
                {
                    _logger.LogWarning("Seek command to client returned {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send seek to client {Ip}", session.ClientIp);
            }
        }
        // Step 3: No valid client info — log and skip
        else
        {
            _logger.LogWarning("No MachineIdentifier or ClientIp:ClientPort available for auto-skip seek");
        }
    }

    private async Task SendSeekToPmsProxyAsync(PlexSessionInfo session, long targetMs, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(session.MachineIdentifier)) return;

            var settings = await _settings.GetAsync();
            var plexUrl = settings.PlexUrl.TrimEnd('/');
            using var httpClient = _httpClientFactory.CreateClient();
            var commandId = Guid.NewGuid().ToString();
            var proxyUrl = $"{plexUrl}/players/{session.MachineIdentifier}/commands/seek?offset={targetMs}&commandID={commandId}";

            var response = await httpClient.GetAsync(proxyUrl, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("PMS proxy seek completed for machineId={Mi}", session.MachineIdentifier);
            }
            else
            {
                _logger.LogWarning("PMS proxy seek returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed PMS proxy seek");
        }
    }
}

public class PlexSessionInfo
{
    public int PositionMs { get; set; }
    public int RatingKey { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public int ClientPort { get; set; }
    public string MachineIdentifier { get; set; } = string.Empty;
}