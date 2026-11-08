# Design

## Context

`SeriesView.vue` renderiza `<button class="group ...">` com `<img>` de capa em contêiner `overflow-hidden rounded-lg`. Já existe `group` e `hover:border-primary`; a miniatura não reage.

## Decisions

### 1. CSS puro no card existente

Lista literal de classes (fonte de verdade):

- `<button>` (card, já tem `group`): adicionar `focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900`; remover `dark:hover:text-primary` (sem par claro, sem efeito nos filhos).
- wrapper da miniatura: `relative mb-3 aspect-[2/3] w-full overflow-hidden rounded-lg bg-gray-100 dark:bg-gray-700`.
- wrapper interno comum aos dois ramos (img e placeholder): `h-full w-full transition-transform duration-300 motion-safe:group-hover:scale-105 motion-safe:group-focus-visible:scale-105 motion-reduce:transition-none`.
- overlay (irmão do wrapper interno, sobre img/placeholder): `pointer-events-none absolute inset-0 bg-black/0 transition-colors group-hover:bg-black/25 group-focus-visible:bg-black/25 motion-reduce:transition-none` com `aria-hidden="true"`.
- `<img>`: `h-full w-full object-cover` (sem transição própria).

Sem JS, sem estado novo. Overlay é obrigatório para manter feedback quando `prefers-reduced-motion: reduce` desativa o zoom.

### 2. Tokens

Usar cores já presentes no tema (`gray`, `primary`, preto translúcido). Overlay fica sobre a imagem, não altera contraste do texto do card.

## Goals / Non-Goals

**Goals:** feedback imediato no hover e foco por teclado; sem layout shift; funcionar em light/dark.

**Non-Goals:** animação de entrada, tilt/3D, mudança no grid, novos componentes.
