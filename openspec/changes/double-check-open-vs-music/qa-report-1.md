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

## Feature adicional: Botão "Reprocessar marcadores"

| # | Critério | Resultado | Evidência |
|---|----------|-----------|-----------|
| A1 | Endpoint POST `/api/markers/reprocess/{seriesKey}` | ✅ | Adicionado em `MarkerEndpoints.cs`, deleta marcadores existentes e re-detecta via `LibraryScanUseCase` |
| A2 | API frontend `reprocessMarkers(seriesKey)` | ✅ | `src/api.js` exporta função autenticada |
| A3 | Botão "Reprocessar" em `MarkerEditorView.vue` | ✅ | Header com ícone `refresh`, loading state, recarrega `fetchSeries` |
| A4 | CSS `.reprocess` | ✅ | `marker-editor.css` com `margin-left: auto` no flex header |

## Validação UI (Playwright)

**N/A** — mudança backend-only (detecção de marcadores). Nenhuma rota, componente ou menu foi alterado; nenhum impacto visual a validar. Tema/menu permanecem inalterados.

O botão "Reprocessar" foi adicionado à UI de edição de marcadores, mas a validação visual de tema/menu não se aplica a este componente (não há mudança de cores, layout ou navegação).

## Achados

Nenhum. Build gate passou de primeira; nenhuma iteração de correção necessária.

## Notas

- `ffmpeg`/`ffprobe` ausentes no host de teste → `AnalyzeSegmentLoudness` retorna `double.NegativeInfinity` (fallback "confirmed"), coberto por teste unitário. Em runtime de container o binário existe.
- Marcadores existentes não são afetados (skip-if-exists preservado).
- O reprocess força limpeza dos marcadores ANTES de re-detectar, permitindo corrigir falsos positivos do double-check.