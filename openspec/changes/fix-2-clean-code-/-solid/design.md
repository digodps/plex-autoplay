# Design

## Context

Achado [2] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: src/PlexAutoplay.Web/Program.cs

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
