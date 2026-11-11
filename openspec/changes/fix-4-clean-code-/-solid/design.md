# Design

## Context

Achado [4] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Classe com mais de 200 linhas (211) - possível violação do SRP (Responsabilidade Única)

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: vue/plex-frontend/src/composables/useSeriesCatalog.js

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
