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
}
