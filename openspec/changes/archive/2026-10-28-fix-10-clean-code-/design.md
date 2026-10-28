# Design

## Context

Achado [10] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Classe com mais de 200 linhas (212) - possível violação do SRP (Responsabilidade Única)

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: tests/e2e/tests/markers.spec.js

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
