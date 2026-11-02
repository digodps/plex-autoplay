namespace PlexAutoplay.Infrastructure.Detection;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Telemetry;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

public class FfmpegMarkerDetector : IMarkerDetector
{
    private const int MinCreditsDurationMs = 300_000; // 5 minutes
    private static readonly ActivitySource ActivitySource = OpenTelemetryService.Tracer;

    public IEnumerable<Marker> Detect(string filePath)
    {
        using var activity = ActivitySource.StartActivity("Ffmpeg.DetectMarkers");
        activity?.SetTag("file.path", filePath);
        activity?.SetTag("detector.type", "ffprobe");

        var markers = new List<Marker>();

        var silenceSegments = RunSilenceDetect(filePath, activity);
        if (silenceSegments.Count == 0)
        {
            return markers;
        }

        // Get file duration for credits detection
        var durationMs = GetDurationMs(filePath, activity);

        // Intro: silence at the very start (from 0ms)
        var introSegment = silenceSegments.FirstOrDefault(s => s.StartMs <= 1000);
        if (introSegment != null && introSegment.EndMs > 0)
        {
            markers.Add(new Marker
            {
                RatingKey = "", // Set by caller
                Kind = "intro",
                StartMs = 0,
                EndMs = introSegment.EndMs
            });
        }

        // Credits: silence near the end (last 15% of duration)
        if (durationMs > MinCreditsDurationMs)
        {
            var creditsThreshold = durationMs * 0.85;
            var creditsSegment = silenceSegments.LastOrDefault(s => s.StartMs >= creditsThreshold);
            if (creditsSegment != null)
            {
                markers.Add(new Marker
                {
                    RatingKey = "", // Set by caller
                    Kind = "credits",
                    StartMs = creditsSegment.StartMs,
                    EndMs = durationMs
                });
            }
        }

        activity?.SetTag("markers.detected", markers.Count);
        return markers;
    }

    private List<SilenceSegment> RunSilenceDetect(string filePath, Activity? parentActivity)
    {
        var segments = new List<SilenceSegment>();

        using var activity = parentActivity != null
            ? ActivitySource.StartActivity("Ffmpeg.SilenceDetect", ActivityKind.Internal, parentActivity.Context)
            : ActivitySource.StartActivity("Ffmpeg.SilenceDetect", ActivityKind.Internal);
        activity?.SetTag("file.path", filePath);
        activity?.SetTag("detection.type", "silence");

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -nostats -nostdin -i \"{filePath}\" -vn -sn -dn -af silencedetect=noise=-30dB:d=1 -f null -",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return segments;

        // ffmpeg reports silence_start/silence_end pairs on stderr while decoding audio.
        var output = process.StandardError.ReadToEnd();
        process.WaitForExit();

        double? pendingStart = null;
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var match = Regex.Match(line, @"silence_(start|end):\s*([0-9.]+)");
            if (!match.Success) continue;
            if (!double.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)) continue;

            if (match.Groups[1].Value == "start")
            {
                pendingStart = seconds;
            }
            else if (pendingStart.HasValue)
            {
                segments.Add(new SilenceSegment((long)(pendingStart.Value * 1000), (long)(seconds * 1000)));
                pendingStart = null;
            }
        }

        // Trailing silence never emits silence_end: the stream ends first.
        if (pendingStart.HasValue)
            segments.Add(new SilenceSegment((long)(pendingStart.Value * 1000), (long)(pendingStart.Value * 1000)));

        activity?.SetTag("segments.found", segments.Count);
        return segments;
    }

    private long GetDurationMs(string filePath, Activity? parentActivity)
    {
        using var activity = parentActivity != null
            ? ActivitySource.StartActivity("Ffmpeg.GetDuration", ActivityKind.Internal, parentActivity.Context)
            : ActivitySource.StartActivity("Ffmpeg.GetDuration", ActivityKind.Internal);
        activity?.SetTag("file.path", filePath);

        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return 0;

        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        if (double.TryParse(output, out var durationSeconds))
        {
            var durationMs = (long)(durationSeconds * 1000);
            activity?.SetTag("duration.ms", durationMs);
            return durationMs;
        }

        return 0;
    }

    private record SilenceSegment(long StartMs, long EndMs);
}
