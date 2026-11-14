# Tasks

## 1. Implement Double-check Intro vs Background Music

- [x] 1.1 Adicionar `AnalyzeSegmentLoudness(string filePath, long startMs, long endMs)` em `FfmpegMarkerDetector` usando ffmpeg `astats`. **Verify:** retorna RMS dBFS do segmento.
- [x] 1.2 Modificar `DetectIntroFromSilence` para validar loudness do segmento após silêncio: confirmado (≥ -20 dBFS), low confidence (-20 a -30), rejeitado (< -30). **Verify:** não cria intro quando rejected.
- [x] 1.3 Ler thresholds de configuração (`MarkerDetection:IntroLoudnessThresholdDb`, `IntroLowConfidenceThresholdDb`) com defaults. **Verify:** valores default funcionam sem appsettings.
- [x] 1.4 Preservar detecção por capítulos como ground truth (sem double-check). **Verify:** capítulo "intro"/"op" cria marcador direto.
- [x] 1.5 Adicionar logs (Info confirmado, Warning rejeitado) e métrica `intro_double_check_total{result}`. **Verify:** logs aparecem em cada caso.
- [x] 1.6 Criar unit tests para `AnalyzeSegmentLoudness` e thresholds. **Verify:** testes passam.
- [x] 1.7 Build: `dotnet build PlexAutoplay.slnx -c Release` passa. **Verify:** sem erros de compilação.
- [x] 1.8 `dotnet test` passa. **Verify:** sem regressões.
