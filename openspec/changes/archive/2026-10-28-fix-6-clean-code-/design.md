# Design

## Context

Achado [6] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Classe com mais de 200 linhas (315) - possível violação do SRP (Responsabilidade Única)

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: src/PlexAutoplay.Web/Services/PlexApiClient.cs

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
