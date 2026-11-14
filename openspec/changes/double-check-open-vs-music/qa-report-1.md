# QA Report — Iteração 1

**Change:** double-check-open-vs-music
**Status:** ✅ PASS
**Worktree:** /home/hermes/plex-autoplay-double-check-open-vs-music
**Branch:** feature/spec-double-check-open-vs-music

## Critérios verificados

| # | Critério (spec) | Resultado | Evidência |
|---|-----------------|-----------|-----------|
| 1 | Double-check loudness RMS pós-silêncio | ✅ | `AnalyzeSegmentLoudness` em `FfmpegMarkerDetector.cs` via `ffmpeg astats` |
| 2 | Confirmado quando RMS ≥ threshold | ✅ | origin `detected`; cenário "Intro candidate confirmed as opening theme" |
| 3 | Rejeitado quando RMS < low-confidence | ✅ | `continue` no loop, sem criar intro; cenário "rejected as background music" |
| 4 | Ambíguo → `detected-low-confidence` | ✅ | ramo `low_confidence` |
| 5 | Thresholds configuráveis c/ defaults | ✅ | `appsettings.json` + env vars `MARKER_INTRO_*`; defaults -20/-30 |
| 6 | Capítulo como ground truth (sem double-check) | ✅ | `FindChapterMarker` inalterado; origin `detected-chapter` |
| 7 | Telemetria `intro_double_check_total{result}` | ✅ | `Counter<int>` no Meter `PlexAutoplay` |
| 8 | Build Release | ✅ | `dotnet build PlexAutoplay.slnx -c Release` → 0 erros |
| 9 | Testes | ✅ | 56/56 passam (`dotnet test` Release) |
| 10 | `openspec validate --strict` | ✅ | "Change 'double-check-open-vs-music' is valid" |

## Validação UI (Playwright)

**N/A** — mudança backend-only (detecção de marcadores). Nenhuma rota, componente ou menu foi alterado; nenhum impacto visual a validar. Tema/menu permanecem inalterados.

## Achados

Nenhum. Build gate passou de primeira; nenhuma iteração de correção necessária.

## Notas

- `ffmpeg`/`ffprobe` ausentes no host de teste → `AnalyzeSegmentLoudness` retorna `double.NegativeInfinity` (fallback "confirmed"), coberto por teste unitário. Em runtime de container o binário existe.
- Marcadores existentes não são afetados (skip-if-exists preservado).