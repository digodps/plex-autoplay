# Design

## Context

O spec `telemetry` já define os requisitos de observabilidade (correlation ID, traces distribuídos, métricas Prometheus, dashboard, logs estruturados). A aplicação falha ao iniciar com `TypeLoadException` no `System.ServiceProviderExtensions` do OpenTelemetry, causado por incompatibilidade entre `OpenTelemetry.Exporter.Prometheus` v1.3.0-rc.2 e `OpenTelemetry` core v1.20.1. Veja `proposal.md` para o porquê.

## Goals / Non-Goals

**Goals:**
- Corrigir a incompatibilidade de versão para que a aplicação inicie sem erro
- Manter todos os requisitos do spec `telemetry` funcionando

**Non-Goals:**
- Alterar qualquer requisito de comportamento do spec `telemetry`
- Adicionar novos exportadores ou destinos de telemetria

## Decisions

### Ajustar versão do OpenTelemetry.Exporter.Prometheus
Substituir `1.3.0-rc.2` por uma versão estável compatível com o OpenTelemetry core `1.20.1`. Alternativa considerada: atualizar todos os pacotes OpenTelemetry para a mesma versão mais recente estável. Rejeitada por risco de introduzir outras incompatibilidades; o alinhamento com a versão core já usada (1.20.1) é mais conservador.

## Risks / Trade-offs

[Versão estável do Prometheus exporter pode não existir para 1.20.1] → Se não houver versão estável compatível, pode ser necessário remover temporariamente o Prometheus exporter e usar apenas Console e OTLP. Mitigação: testar `dotnet build` após a alteração.
