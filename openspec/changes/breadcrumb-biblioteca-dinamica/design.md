# Design

## Context

O breadcrumb é gerado por `buildBreadcrumb` em `src/lib/breadcrumb.js`. Na rota `series-markers`, a última trilha é construída em `text(query.seriesTitle) || "Biblioteca"`, que usa o título da série quando disponível e o nome fixo "Biblioteca" como fallback. Isso ignora o nome da biblioteca mesmo quando ele está disponível.

## Goals / Non-Goals

**Goals:**
- Última migalha da rota `series-markers` exiba o nome dinâmico da biblioteca (ex.: "Animes") quando disponível.
- Manter o nome fixo "Biblioteca" como último recurso quando não há biblioteca nem título.

**Non-Goals:**
- Alterar `buildBiblioteca` assinatura, outras rotas, sidebar, AppHeader ou roteador.
- Alterar a rota `series` (já exibe `lib` dinamicamente).

## Decisions

1. **Mudar uma linha em `src/lib/breadcrumb.js`**: `text(query.seriesTitle) || "Biblioteca"` → `text(query.seriesTitle) || lib || "Biblioteca"`. Rationale: mínima alteração; `lib` já está calculado acima (nome da biblioteca) e só precisa ser usado como fallback antes do fixo.
