# Design: Double-check Intro vs Background Music

## Approach

Extend `FfmpegMarkerDetector.DetectIntroFromSilence` to perform a second audio analysis pass on the candidate intro segment to measure loudness (RMS) and distinguish opening theme (loud, full mix) from background music (quiet, ducked under speech).

## Architecture

### 1. New Configuration
Add to `appsettings.json`:
```json
{
  "MarkerDetection": {
    "IntroLoudnessThresholdDb": -20.0,
    "IntroLowConfidenceThresholdDb": -30.0
  }
}
```

### 2. Modified `FfmpegMarkerDetector`

#### New method: `AnalyzeSegmentLoudness(string filePath, long startMs, long endMs)`
Uses `ffmpeg -af astats=metadata=1:reset=1` to get RMS level of a specific time segment. Returns average RMS in dBFS.

#### Modified `DetectIntroFromSilence`
After finding a candidate gap:
1. Get the audio segment after the silence: `[gap.EndMs, gap.EndMs + FallbackIntroLengthMs]`
2. Call `AnalyzeSegmentLoudness` on that segment
3. Compare RMS to thresholds:
   - `>= IntroLoudnessThresholdDb` → CONFIRMED (origin "detected")
   - `>= IntroLowConfidenceThresholdDb` → LOW CONFIDENCE (origin "detected-low-confidence")  
   - `< IntroLowConfidenceThresholdDb` → REJECTED (return null, log warning)

### 3. Preserve Chapter Detection
`FindChapterMarker` remains unchanged — chapters are ground truth, no double-check.

### 4. Telemetry/Logging
- Add `ActivitySource` tags: `intro.loudness_db`, `intro.double_check_result`
- Add counter metric: `intro_double_check_total{result}`

## Implementation Details

### Loudness Analysis via ffmpeg
```bash
ffmpeg -hide_banner -nostats -nostdin -ss {start}s -t {duration}s -i "file" -vn -sn -dn -af "astats=metadata=1:reset=1" -f null -
```
Parse `RMS level dB` from stderr output.

### Threshold Rationale (Defaults)
- `-20 dBFS`: Typical full-mix theme song loudness
- `-30 dBFS`: Background music under speech is typically -30 to -40 dBFS
- Configurable for different content types

## Files to Modify

1. `src/PlexAutoplay.Infrastructure/Detection/FfmpegMarkerDetector.cs` — core logic
2. `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs` — config binding
3. `src/PlexAutoplay.Application/AppSettings.cs` — settings class (if exists)
4. `appsettings.json` / `appsettings.Development.json` — defaults

## Tests

1. Unit: `DetectIntroFromSilence` with mocked loudness → confirms/rejects correctly
2. Integration: Real file with known intro vs cold open music
3. Config: Threshold override via appsettings works