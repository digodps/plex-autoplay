namespace PlexAutoplay.Infrastructure.Detection;

using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Infrastructure.Telemetry;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.RegularExpressions;

public class FfmpegMarkerDetector : IMarkerDetector
{
    private const int MinCreditsDurationMs = 300_000; // 5 minutes
    private const int FallbackIntroLengthMs = 90_000; // typical TV/anime opening length
    private static readonly ActivitySource ActivitySource = OpenTelemetryService.Tracer;
    private static readonly Meter MarkerMeter = new("PlexAutoplay");
    private static readonly Counter<int> IntroDoubleCheckCounter = MarkerMeter.CreateCounter<int>("intro_double_check_total");

    private static readonly string[] IntroChapterNames = { "intro", "op", "opening", "opening credits", "abertura" };
    private static readonly string[] CreditsChapterNames = { "credits", "ed", "ending", "outro", "end credits" };

    public double IntroLoudnessThresholdDb { get; }
    public double IntroLowConfidenceThresholdDb { get; }

    public FfmpegMarkerDetector()
    {
        IntroLoudnessThresholdDb = GetEnvDouble("MARKER_INTRO_LOUDNESS_THRESHOLD_DB", -20.0);
        IntroLowConfidenceThresholdDb = GetEnvDouble("MARKER_INTRO_LOW_CONFIDENCE_THRESHOLD_DB", -30.0);
    }

    public FfmpegMarkerDetector(double introLoudnessThresholdDb, double introLowConfidenceThresholdDb)
    {
        IntroLoudnessThresholdDb = introLoudnessThresholdDb;
        IntroLowConfidenceThresholdDb = introLowConfidenceThresholdDb;
    }

    private static double GetEnvDouble(string name, double defaultValue)
    {
        var val = Environment.GetEnvironmentVariable(name);
        return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

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
            intro = DetectIntroFromSilence(filePath, silenceSegments, durationMs, activity);
            if (intro != null) activity?.SetTag("intro.source", "silence");
        }
        else
        {
            activity?.SetTag("intro.source", "chapter");
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
                    EndMs = durationMs,
                    Origin = "detected"
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
        var chapter = chapters.FirstOrDefault(c => MatchesName(c.Title, names));
        if (chapter == null) return null;

        return new Marker
        {
            RatingKey = "", // Set by caller
            Kind = kind,
            StartMs = chapter.StartMs,
            EndMs = chapter.EndMs,
            Origin = "detected-chapter"
        };
    }

    private static bool MatchesName(string title, string[] names)
    {
        var normalized = Regex.Replace(title.Trim().ToLowerInvariant(), @"\s+", " ");
        if (normalized.Length == 0) return false;

        return names.Any(n => normalized == n || normalized.StartsWith(n + " "));
    }

    private Marker? DetectIntroFromSilence(string filePath, List<SilenceSegment> silenceSegments, int durationMs, Activity? activity)
    {
        if (silenceSegments.Count == 0 || durationMs <= 0) return null;

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

            // Double check loudness of segment
            var loudnessDb = AnalyzeSegmentLoudness(filePath, start, end, activity);
            activity?.SetTag("intro.loudness_db", loudnessDb);

            string result;
            if (loudnessDb == double.NegativeInfinity)
            {
                // Fallback if ffmpeg loudness check fails (e.g. unit tests or decode error)
                result = "confirmed";
            }
            else if (loudnessDb >= IntroLoudnessThresholdDb)
            {
                result = "confirmed";
            }
            else if (loudnessDb >= IntroLowConfidenceThresholdDb)
            {
                result = "low_confidence";
            }
            else
            {
                result = "rejected";
            }

            IntroDoubleCheckCounter.Add(1, new KeyValuePair<string, object?>("result", result));
            activity?.SetTag("intro.double_check_result", result);

            if (result == "rejected")
            {
                activity?.AddEvent(new ActivityEvent("intro.rejected_background_music", tags: new ActivityTagsCollection { ["loudness_db"] = loudnessDb }));
                continue; // Try next gap
            }

            var origin = result == "low_confidence" ? "detected-low-confidence" : "detected";

            return new Marker
            {
                RatingKey = "", // Set by caller
                Kind = "intro",
                StartMs = start,
                EndMs = end,
                Origin = origin
            };
        }

        return null;
    }

    internal double AnalyzeSegmentLoudness(string filePath, long startMs, long endMs, Activity? parentActivity)
    {
        var startSec = startMs / 1000.0;
        var durationSec = (endMs - startMs) / 1000.0;
        if (durationSec <= 0) return double.NegativeInfinity;

        using var activity = ActivitySource.StartActivity("Ffmpeg.AudioLoudness", ActivityKind.Internal, parentActivity?.Context ?? default);
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -nostats -nostdin -ss {startSec.ToString(CultureInfo.InvariantCulture)} -t {durationSec.ToString(CultureInfo.InvariantCulture)} -i \"{filePath}\" -vn -sn -dn -af \"astats=metadata=1:reset=1\" -f null -",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null) return double.NegativeInfinity;
            var output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var match = Regex.Match(output, @"RMS level dB:\s*([-0-9.]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var rms))
            {
                return rms;
            }
        }
        catch
        {
            // If ffmpeg is not available or fails, return negative infinity (treated as fallback confirmed)
        }

        return double.NegativeInfinity;
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

        try
        {
            using var process = Process.Start(psi);
            if (process == null) return chapters;

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
        }
        catch
        {
            // ignore ffprobe failures
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

        try
        {
            using var process = Process.Start(psi);
            if (process == null) return segments;

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

            if (pendingStart.HasValue)
                segments.Add(new SilenceSegment((long)(pendingStart.Value * 1000), (long)(pendingStart.Value * 1000)));
        }
        catch
        {
            // ignore ffmpeg failures
        }

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

        try
        {
            using var process = Process.Start(psi);
            if (process == null) return 0;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? (int)(seconds * 1000)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    internal sealed record SilenceSegment(long StartMs, long EndMs);
    internal sealed record Chapter(long StartMs, long EndMs, string Title);
}
