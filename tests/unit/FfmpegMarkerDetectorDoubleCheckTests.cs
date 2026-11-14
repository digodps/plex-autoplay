using System.Diagnostics;
using PlexAutoplay.Infrastructure.Detection;

public class FfmpegMarkerDetectorDoubleCheckTests
{
    [Fact]
    public void Constructor_DefaultsSet()
    {
        var detector = new FfmpegMarkerDetector();
        Assert.Equal(-20.0, detector.IntroLoudnessThresholdDb);
        Assert.Equal(-30.0, detector.IntroLowConfidenceThresholdDb);
    }

    [Fact]
    public void Constructor_CustomThresholdsSet()
    {
        var detector = new FfmpegMarkerDetector(-15.0, -25.0);
        Assert.Equal(-15.0, detector.IntroLoudnessThresholdDb);
        Assert.Equal(-25.0, detector.IntroLowConfidenceThresholdDb);
    }

    [Fact]
    public void AnalyzeSegmentLoudness_NoFfmpeg_ReturnsNegativeInfinity()
    {
        var detector = new FfmpegMarkerDetector();
        var loudness = detector.AnalyzeSegmentLoudness("nonexistent.mp4", 0, 90_000, null);
        Assert.Equal(double.NegativeInfinity, loudness);
    }

    [Fact]
    public void DetectIntroFromSilence_PairsConsecutiveGaps_NinetySecondsApart()
    {
        var detector = new FfmpegMarkerDetector();
        var silence = new List<SilenceSegment>
        {
            new(10_000, 12_000),   // cold open pause
            new(196_000, 198_000), // pre-OP silence (196-198s)
            new(286_000, 288_000), // post-OP silence (286-288s)
            new(400_000, 402_000), // later
        };
        var result = detector.DetectIntroFromSilence("dummy.mp4", silence, 500_000, null);
        Assert.NotNull(result);
        Assert.Equal(198_000, result.StartMs); // OP starts at end of pre-OP silence
        Assert.Equal(286_000, result.EndMs);   // OP ends at start of post-OP silence
    }

    [Fact]
    public void DetectIntroFromSilence_RejectsGapPair_OutsideDurationWindow()
    {
        var detector = new FfmpegMarkerDetector();
        var silence = new List<SilenceSegment>
        {
            new(72_000, 74_000),   // cold open internal silence
            new(196_000, 198_000), // pre-OP silence
            new(320_000, 322_000), // 122s after pre-OP → too long
        };
        var result = detector.DetectIntroFromSilence("dummy.mp4", silence, 500_000, null);
        Assert.Null(result); // 122s > 105s window
    }

    [Fact]
    public void DetectIntroFromSilence_RequiresTwoSilences_NotSingleGap()
    {
        var detector = new FfmpegMarkerDetector();
        var silence = new List<SilenceSegment>
        {
            new(196_000, 198_000), // only one silence gap
        };
        var result = detector.DetectIntroFromSilence("dummy.mp4", silence, 500_000, null);
        Assert.Null(result);
    }
}
