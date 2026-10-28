# Proposal

## Why

Correção do achado [7] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/Services/PlexApiClient.cs

## Capabilities

### New Capabilities

- code-review-fix-7: Correção específica do achado [7] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/Services/PlexApiClient.cs
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: `new Meter(...)`/`CreateCounter` (singletons estáticos de telemetria) e `new HttpRequestMessage(...)` (BCL). Nenhuma dependência injetável; documentado e arquivado sem alteração de código.
