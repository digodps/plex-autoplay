# Proposal

## Why

Correção do achado [4] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs

## Capabilities

### New Capabilities

- code-review-fix-4: Correção específica do achado [4] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: `new` apenas em DTOs anônimos de resposta e `Results.*`; sem dependências concretas de serviço. Documentado e arquivado sem alteração de código.
