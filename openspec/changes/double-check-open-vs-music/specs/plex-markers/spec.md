# Double-check Intro vs Background Music Detection Specification

## Purpose
Add a double-check mechanism to distinguish between a true opening theme (intro/OP) and background music/cold open music when detecting intro markers via silence detection. This addresses false positives where silence followed by music is incorrectly labeled as an intro when it's actually background music in a cold open.

## Requirements

### Requirement: Intro candidate validation via audio characteristic analysis
O sistema SHALL, após detectar um candidato de intro via detecção de silêncio, executar uma validação secundária (double-check) analisando as características de áudio do segmento seguinte ao silêncio para diferenciar um tema de abertura (opening theme) de música de fundo (background music).

#### Scenario: Intro candidate rejected as background music
- **WHEN** silence detection finds a candidate intro (silence gap 20s-5min followed by ≥60s audio)
- **AND** the audio segment following the silence has low loudness (RMS below threshold) indicating background music under speech
- **THEN** system rejects the intro candidate, does not create intro marker
- **AND** logs rejection reason for debugging

#### Scenario: Intro candidate confirmed as opening theme
- **WHEN** silence detection finds a candidate intro
- **AND** the audio segment following the silence has high loudness (RMS above threshold) indicating full-mix theme song
- **THEN** system confirms the intro marker with origin "detected"
- **AND** proceeds to create marker as normal

#### Scenario: Intro candidate with ambiguous audio
- **WHEN** silence detection finds a candidate intro
- **AND** the audio segment loudness is in ambiguous range
- **THEN** system creates intro marker with origin "detected-low-confidence"
- **AND** flags for potential manual review

### Requirement: Configurable loudness threshold
O sistema SHALL permitir configurar o threshold de loudness (RMS) para o double-check via configuração (ex: variável de ambiente ou appsettings), com valor padrão conservador.

#### Scenario: Custom loudness threshold via config
- **WHEN** appsettings define `MarkerDetection:IntroLoudnessThresholdDb` (ex: -20dB)
- **THEN** double-check usa esse valor ao invés do default

### Requirement: Preserve chapter-based detection as ground truth
O sistema SHALL manter a detecção baseada em capítulos (chapter names: intro, op, opening, abertura) como ground truth absoluta — o double-check NÃO se aplica quando há capítulo nomeado.

#### Scenario: Chapter intro bypasses double-check
- **WHEN** arquivo possui capítulo com título "intro", "op", "opening" ou "abertura"
- **THEN** marcador intro é criado diretamente sem double-check de áudio
- **AND** origin = "detected-chapter"

## Non-Functional Requirements

### Performance
- O double-check não deve adicionar mais que 2-3 segundos por episódio
- Reutiliza o processo ffmpeg existente (não spawnar processo extra se possível)

### Observability
- Log nível Info quando intro confirmado
- Log nível Warning quando intro rejeitado por low confidence
- Métrica: `intro_double_check_total{result="confirmed|rejected|low_confidence"}`

### Backwards Compatibility
- Marcadores existentes não são afetados
- Origin "detected" continua para intro confirmados
- Novo origin "detected-low-confidence" para casos ambíguos