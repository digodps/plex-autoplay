# Proposal

## Why

Correção do achado [12] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/wwwroot/assets/index-DCLGfyQb.js

## Capabilities

### New Capabilities

- code-review-fix-12: Correção específica do achado [12] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/wwwroot/assets/index-DCLGfyQb.js
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: idem fix-11 — bundle minificado gerado em wwwroot/assets, excluído do scan e dos linters. Sem alteração de código-fonte.
