# Proposal

## Why

Correção do achado [3] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs

## Capabilities

### New Capabilities

- code-review-fix-3: Correção específica do achado [3] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: os `new` são DTOs anônimos de resposta (`Results.Ok(new { ... })`) e `new StreamReader(Request.Body)` (BCL, código de bindings HTTP). Nada de dependências injetáveis; documentado e arquivado sem alteração de código.
