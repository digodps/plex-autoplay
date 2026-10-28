# Proposal

## Why

O sistema `plex-autoplay` foi reescrito em .NET 10 com DDD/Clean Architecture, Vue.js SPA e Docker, mas não possui telemetria de ponta a ponta que permita rastrear requisições desde o frontend Vue até o banco SQLite e os serviços externos (Plex, ffprobe). Sem métricas, traces e logs estruturados correlacionados, é impossível diagnosticar problemas de desempenho, identificar gargalos de scan de bibliotecas ou entender o comportamento real da aplicação em produção.

## What Changes

- Adicionar telemetria distribuída (OpenTelemetry) ao ASP.NET Core Web API com traces e métricas
- Instrumentar HTTP client para chamadas ao Plex Media Server com propagação de contexto de trace
- Adicionar métricas de banco de dados (SQLite/Dapper) e de detecção de marcadores (ffprobe)
- Integrar logs estruturados com correlation ID para rastrear requisições do frontend Vue até o backend
- Exibir dashboard básico de métricas (tempo de resposta, taxa de erro, duração de scans) na aplicação
- Configurar exportador de telemetria (console para dev, OTLP para produção) via variáveis de ambiente

## Capabilities

### New Capabilities

- `telemetry`: Observabilidade de ponta a ponta com traces distribuídos, métricas e logs estruturados correlacionados via correlation ID, cobrindo frontend Vue, API .NET, banco SQLite e chamadas externas ao Plex.

### Modified Capabilities

- Nenhum (os specs existentes de plex-* continuam inalterados)

## Impact

- Novas dependências NuGet: OpenTelemetry, OpenTelemetry.Exporter.Console, OpenTelemetry.Exporter.Otlp, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.AspNetCore
- Alterações em `PlexAutoplay.Web` (Program.cs, middleware de correlation ID)
- Alterações em `PlexAutoplay.Infrastructure` (instrumentação de repositórios Dapper e chamadas HTTP ao Plex)
- Alterações em `PlexAutoplay.Application` (propagação de correlation ID nos use cases)
- Variáveis de ambiente adicionais: OTEL_EXPORTER, OTEL_ENDPOINT, OTEL_SERVICE_NAME
- Não altera os specs existentes (plex-auth, plex-auto-skip, plex-images, plex-libraries, plex-markers, plex-sync)
