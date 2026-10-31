# Design

## Context

Achado [1] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: dotnet-format detectou diferenças de formatação em arquivos .cs

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs, src/PlexAutoplay.Web/Program.cs

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
