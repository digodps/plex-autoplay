# Proposal

## Why

Correção do achado [13] da revisão de código: Classe com mais de 200 linhas (403) - possível violação do SRP (Responsabilidade Única)

## What Changes

- Corrigir o problema identificado: Classe com mais de 200 linhas (403) - possível violação do SRP (Responsabilidade Única)
- Arquivos afetados: vue/plex-frontend/src/views/MarkerEditorView.vue

## Capabilities

### New Capabilities

- code-review-fix-13: Correção específica do achado [13] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: vue/plex-frontend/src/views/MarkerEditorView.vue
- Referências:   SOLID - S (Single Responsibility Principle)

## Resultado da aplicação

MarkerEditorView.vue (445 → 91 linhas): estilo extraído para views/marker-editor.css e a linha de marcador extraída para components/MarkerRow.vue (110 linhas, estilo em components/marker-row.css). Classes CSS/HTML preservadas; E2E de UI verdes (23 passed).
