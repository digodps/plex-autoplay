# Deficiências resolvidas (EGM Goldfish Review A/S/C)

## Revisor A — Arquitetura, dependências e integração
- Achado 1: Double-check encapsulado em `FfmpegMarkerDetector`, mantendo a porta `IMarkerDetector` intacta. Sem impacto em controladores HTTP ou repositórios. **ACEITO.**
- Achado 2: Nenhuma nova dependência externa adicionada (usa ffmpeg `astats` já disponível). **ACEITO.**

## Revisor S — SOLID e coesão
- Achado 1: Responsabilidade de análise de loudness adicionada de forma coesa em `FfmpegMarkerDetector`. **ACEITO.**
- Achado 2: Preservação estrita de capítulos como ground truth absoluto. **ACEITO.**

## Revisor C — Clean Code, escopo e testabilidade
- Achado 1: Thresholds configuráveis com valores default seguros. **ACEITO.**
- Achado 2: Adicionados testes unitários para cobrir confirmação, rejeição por low confidence e fallback. **ACEITO.**
