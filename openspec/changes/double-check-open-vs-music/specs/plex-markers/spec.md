# Double-check Intro vs Background Music Specification

## Purpose
Add a double-check mechanism to distinguish between a true opening theme (intro/OP) and background music/cold open music when detecting intro markers via silence detection. This addresses false positives where silence followed by music is incorrectly labeled as an intro when it's actually background music in a cold open.

## MODIFIED Requirements

### Requirement: Detect intro markers from audio silence
O sistema SHALL analisar um arquivo de mídia usando detecção de silêncio do ffprobe e identificar segmentos contíguos de silêncio no início do episódio como candidatos a marcadores de intro. Após identificar um candidato via silêncio, o sistema SHALL executar uma validação secundária (double-check) analisando a loudness (RMS) do segmento de áudio seguinte ao silêncio para diferenciar um tema de abertura (opening theme) de música de fundo (background music).

#### Scenario: Intro detected at episode start
- **WHEN** ffprobe finds silence from 0ms to approximately 90000ms in a series episode
- **THEN** the system creates a marker of kind `intro` with `start_ms=0` and `end_ms` equal to the silence end

#### Scenario: No silence at episode start
- **WHEN** ffprobe finds no significant silence in the first 120 seconds of audio
- **THEN** the system does not create an intro marker for that episode

#### Scenario: Intro detectado no início do episódio
- **WHEN** ffprobe encontra silêncio de 0ms a aproximadamente 90000ms em um episódio de série
- **THEN** o sistema cria um marcador do tipo intro com start_ms=0 e end_ms igual ao fim do silêncio

#### Scenario: Intro candidate rejected as background music
- **WHEN** silence detection finds a candidate intro (silence gap 20s-5min followed by ≥60s audio)
- **AND** the audio segment following the silence has low loudness (RMS below the low-confidence threshold) indicating background music under speech
- **THEN** the system rejects the intro candidate and does not create an intro marker
- **AND** records the rejection in telemetry (`intro_double_check_total{result="rejected"}`)

#### Scenario: Intro candidate confirmed as opening theme
- **WHEN** silence detection finds a candidate intro
- **AND** the audio segment following the silence has high loudness (RMS at or above the confirm threshold) indicating a full-mix theme song
- **THEN** the system confirms the intro marker with origin `detected`

#### Scenario: Intro candidate with ambiguous audio
- **WHEN** silence detection finds a candidate intro
- **AND** the audio segment loudness is between the low-confidence and confirm thresholds
- **THEN** the system creates the intro marker with origin `detected-low-confidence`

## ADDED Requirements

### Requirement: Configurable intro loudness threshold
O sistema SHALL permitir configurar os thresholds de loudness (RMS) usados no double-check via configuração (`MarkerDetection:IntroLoudnessThresholdDb` e `MarkerDetection:IntroLowConfidenceThresholdDb`), com valores padrão conservadores de -20 dBFS (confirmado) e -30 dBFS (low confidence).

#### Scenario: Custom loudness threshold via config
- **WHEN** appsettings define `MarkerDetection:IntroLoudnessThresholdDb` (ex: -18dB) e `MarkerDetection:IntroLowConfidenceThresholdDb` (ex: -28dB)
- **THEN** o double-check usa esses valores ao invés dos defaults

#### Scenario: Default thresholds without configuration
- **WHEN** nenhuma configuração de `MarkerDetection` é fornecida
- **THEN** o double-check usa -20 dBFS (confirmado) e -30 dBFS (low confidence)

### Requirement: Preserve chapter-based detection as ground truth
O sistema SHALL manter a detecção baseada em capítulos (chapter names: intro, op, opening, abertura) como ground truth absoluta — o double-check de loudness NÃO se aplica quando há capítulo nomeado.

#### Scenario: Chapter intro bypasses double-check
- **WHEN** o arquivo possui capítulo com título "intro", "op", "opening" ou "abertura"
- **THEN** o marcador intro é criado diretamente sem double-check de áudio
- **AND** origin = "detected-chapter"