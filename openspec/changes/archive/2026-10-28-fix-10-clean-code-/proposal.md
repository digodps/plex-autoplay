# Proposal

## Why

Correção do achado [10] da revisão de código: Classe com mais de 200 linhas (212) - possível violação do SRP (Responsabilidade Única)

## What Changes

- Corrigir o problema identificado: Classe com mais de 200 linhas (212) - possível violação do SRP (Responsabilidade Única)
- Arquivos afetados: tests/e2e/tests/markers.spec.js

## Capabilities

### New Capabilities

- code-review-fix-10: Correção específica do achado [10] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: tests/e2e/tests/markers.spec.js
- Referências:   SOLID - S (Single Responsibility Principle)

## Resultado da aplicação

markers.spec.js dividido por responsabilidade: CRUD de API em markers.spec.js (106 linhas) e catálogo/editor de UI em tests/series-editor.spec.js (80 linhas); helpers loginByUi/bearerToken movidos para tests/utils/auth.js. E2E 23 passed + 2 skipped.
