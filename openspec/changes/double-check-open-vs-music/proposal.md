# Proposal

## Why

O detector de marcadores `FfmpegMarkerDetector` detecta intro por detecção de silêncio: um silêncio seguido de ≥60s de áudio é rotulado como abertura (intro). Isso gera falsos positivos quando o áudio após o silêncio é música de fundo de um cold open, não uma tema de abertura (opening theme). São necessários um double-check de características de áudio para identificar se realmente é uma open ou apenas música no episódio.

## What Changes

- Adicionar double-check de loudness (RMS) do segmento de áudio após o silêncio candidato a intro.
- Se loudness alta → tema de abertura confirmado (`origin = "detected"`).
- Se loudness baixa → rejeitado como música de fundo, sem criar marcador intro.
- Se loudness ambígua → marcador com `origin = "detected-low-confidence"`.
- Manter detecção baseada em capítulos como ground truth (não há double-check).
- Tornar thresholds configuráveis via `appsettings.json`.
- Adicionar telemetria/logs de resultado do double-check.

## Capabilities

### New Capabilities
- `plex-markers-double-check`: validação secundária de áudio para diferenciar abertura vs música de fundo na detecção de intro.

### Modified Capabilities
- `plex-markers`: `FfmpegMarkerDetector` ganha double-check de loudness; resultados mais precisos, menos falsos positivos de intro.

## Impact

**Backend (`PlexAutoplay.Infrastructure/Detection/FfmpegMarkerDetector.cs`):**
- `DetectIntroFromSilence` passa a validar loudness do segmento pós-silêncio.
- Novo método `AnalyzeSegmentLoudness` via ffmpeg `astats`.

**Config (`appsettings.json` / settings):**
- `MarkerDetection:IntroLoudnessThresholdDb` (default -20 dBFS)
- `MarkerDetection:IntroLowConfidenceThresholdDb` (default -30 dBFS)

**Telemetry:**
- Counter `intro_double_check_total{result}` com rótulos confirmed/rejected/low_confidence.
- Activity tags `intro.loudness_db`, `intro.double_check_result`.

**Tests:**
- Unit tests para `DetectIntroFromSilence` com loudness mockada.
- Testes de configuração (override de threshold).
