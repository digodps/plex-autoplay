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
    private const int FallbackIntroLengthMs = 90_000; // typical TV/anime opening length
    private static readonly ActivitySource ActivitySource = OpenTelemetryService.Tracer;

    private static readonly string[] IntroChapterNames = { "intro", "op", "opening", "opening credits", "abertura" };
    private static readonly string[] CreditsChapterNames = { "credits", "ed", "ending", "outro", "end credits" };

    public IEnumerable<Marker> Detect(string filePath)
    {
        using var activity = ActivitySource.StartActivity("Ffmpeg.DetectMarkers");
        activity?.SetTag("file.path", filePath);
        activity?.SetTag("detector.type", "ffprobe+chapters");

        var markers = new List<Marker>();
        var chapters = GetChapters(filePath, activity);

        var intro = FindChapterMarker(chapters, IntroChapterNames, "intro");
        var credits = FindChapterMarker(chapters, CreditsChapterNames, "credits");

        // Audio analysis is only needed where the container has no usable chapter metadata.
        var silenceSegments = intro == null || credits == null
            ? RunSilenceDetect(filePath, activity)
            : new List<SilenceSegment>();
        var durationMs = intro == null || credits == null ? GetDurationMs(filePath, activity) : 0;

        if (intro == null)
        {
            intro = DetectIntroFromSilence(silenceSegments, durationMs);
            if (intro != null) activity?.SetTag("intro.source", "silence");
        }

        if (credits == null && durationMs > MinCreditsDurationMs)
        {
            // Credits: last silence inside the final 15% of the file.
            var creditsThreshold = durationMs * 0.85;
            var creditsSegment = silenceSegments.LastOrDefault(s => s.StartMs >= creditsThreshold);
            if (creditsSegment != null)
            {
                credits = new Marker
                {
                    RatingKey = "", // Set by caller
                    Kind = "credits",
                    StartMs = creditsSegment.StartMs,
                    EndMs = durationMs
                };
                activity?.SetTag("credits.source", "silence");
            }
        }

        if (intro != null) markers.Add(intro);
        if (credits != null) markers.Add(credits);

        activity?.SetTag("markers.detected", markers.Count);
        return markers;
    }

    private static Marker? FindChapterMarker(List<Chapter> chapters, string[] names, string kind)
    {
        // Skipped: duration sanity checks. Tagged chapters are treated as ground truth.
        var chapter = chapters.FirstOrDefault(c => MatchesName(c.Title, names));
        if (chapter == null) return null;

        return new Marker
        {
            RatingKey = "", // Set by caller
            Kind = kind,
            StartMs = chapter.StartMs,
            EndMs = chapter.EndMs
        };
    }

    private static bool MatchesName(string title, string[] names)
    {
        var normalized = Regex.Replace(title.Trim().ToLowerInvariant(), @"\s+", " ");
        if (normalized.Length == 0) return false;

        // "OP", "Intro", "Opening", "Opening Credits", "ED", "Ending", "Credits"
        return names.Any(n => normalized == n || normalized.StartsWith(n + " "));
    }

    private static Marker? DetectIntroFromSilence(List<SilenceSegment> silenceSegments, int durationMs)
    {
        if (silenceSegments.Count == 0 || durationMs <= 0) return null;

        // ponytail: without chapters the opening is guessed as the first >=1s silence gap
        // between 20s and 5min that is followed by >=60s of continuous audio (the OP song).
        // Upgrade path: fingerprint audio across episodes of the same series (needs >1 episode).
        var gaps = silenceSegments
            .Where(s => s.EndMs - s.StartMs >= 1000 && s.StartMs >= 20_000 && s.StartMs <= 300_000)
            .OrderBy(s => s.StartMs)
            .ToList();

        foreach (var gap in gaps)
        {
            var nextStart = gaps.FirstOrDefault(g => g.StartMs > gap.StartMs)?.StartMs ?? durationMs;
            if (nextStart - gap.EndMs < 60_000) continue;

            var start = gap.StartMs;
            var end = Math.Min(start + FallbackIntroLengthMs, durationMs - 120_000);
            if (end - start < 30_000) continue;

            return new Marker
            {
                RatingKey = "", // Set by caller
                Kind = "intro",
                StartMs = start,
                EndMs = end
            };
        }

        return null;
    }

    private List<Chapter> GetChapters(string filePath, Activity? parentActivity)
    {
        var chapters = new List<Chapter>();
        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_chapters -of csv=p=0 \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return chapters;

        // csv=p=0 -> chapter_id,time_base,start,start_time,end,end_time,title
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) return chapters;

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var fields = line.Trim().Split(',');
            if (fields.Length < 7) continue;
            if (!double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var start)) continue;
            if (!double.TryParse(fields[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var end)) continue;

            var title = string.Join(',', fields.Skip(6));
            chapters.Add(new Chapter((long)(start * 1000), (long)(end * 1000), title));
        }

        return chapters;
    }

    private List<SilenceSegment> RunSilenceDetect(string filePath, Activity? parentActivity)
    {
        var segments = new List<SilenceSegment>();
        using var activity = ActivitySource.StartActivity("Ffmpeg.SilenceDetect", ActivityKind.Internal, parentActivity?.Context ?? default);

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

        return segments;
    }

    private int GetDurationMs(string filePath, Activity? parentActivity)
    {
        using var activity = ActivitySource.StartActivity("Ffmpeg.GetDuration", ActivityKind.Internal, parentActivity?.Context ?? default);

        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of csv=p=0 \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return 0;

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? (int)(seconds * 1000)
            : 0;
    }

    private sealed record SilenceSegment(long StartMs, long EndMs);
    private sealed record Chapter(long StartMs, long EndMs, string Title);
}
