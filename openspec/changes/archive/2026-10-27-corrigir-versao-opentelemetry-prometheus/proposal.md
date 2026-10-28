# Proposal

## Why

A aplicação .NET falha ao iniciar com `System.TypeLoadException: Could not load type 'System.ServiceProviderExtensions' from assembly 'OpenTelemetry, Version=1.0.0.0'`. Isso ocorre porque o pacote `OpenTelemetry.Exporter.Prometheus` na versão `1.3.0-rc.2` é um release candidate incompatível com o `OpenTelemetry` core na versão `1.20.1`, causando quebra no carregamento do serviço de telemetria no startup.

## What Changes

- Ajustar a versão do pacote `OpenTelemetry.Exporter.Prometheus` no `PlexAutoplay.Web.csproj` para uma versão compatível com o `OpenTelemetry` core `1.20.1`
- Não há alteração de comportamento especificado no spec `telemetry` — apenas correção de dependência

## Capabilities

### New Capabilities

- (nenhum)

### Modified Capabilities

- (nenhum — correção de dependência apenas, não altera requisitos de comportamento do spec `telemetry`)

## Impact

- Arquivo afetado: `src/PlexAutoplay.Web/PlexAutoplay.Web.csproj`
- Nenhuma API pública é alterada
- Nenhum comportamento de telemetria especificado é modificado
