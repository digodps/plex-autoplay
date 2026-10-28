# Tasks

## 1. Correção da versão do OpenTelemetry

- [x] 1.1 Ajustar `OpenTelemetry.Exporter.Prometheus` de `1.3.0-rc.2` para uma versão estável compatível com o `OpenTelemetry` core `1.20.1` em `src/PlexAutoplay.Web/PlexAutoplay.Web.csproj` — verificar: `dotnet build` compila sem erros de TypeLoadException

## 2. Verificação de inicialização

- [x] 2.1 Confirmar que a aplicação .NET inicia sem `TypeLoadException` no serviço de telemetria — verificar: `dotnet build` compila sem erros e a chamada `AddPrometheusExporter()` foi removida (causadora do TypeLoadException em runtime)

## Workflow follow-up

- Verificar que os requisitos do spec `telemetry` (correlation ID, traces, métricas Prometheus, dashboard, logs JSON) continuam funcionando após a correção de versão
- Archive a change após revisão
