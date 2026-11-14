# EGM — Goldfish Review (A/S/C): library-card-series-mosaic-hover

Revisão prévia dos requisitos e design, dentro da worktree isolada
`feature/spec-library-card-shape-magic-hover`.

## Revisor A — Arquitetura, dependências e integração

| # | Achado | Classificação | Ação |
|---|--------|---------------|------|
| A1 | Duas views `LibrariesView.vue` (`views/` e `views/Libraries/`); o router usa `views/Libraries/LibrariesView.vue`. Risco de editar o arquivo errado. | Aceito | Integração feita **apenas** em `views/Libraries/LibrariesView.vue`. O arquivo `views/LibrariesView.vue` permanece intocado (não é roteado). |
| A2 | Endpoint `/api/series/{key}` já existe e retorna `thumb` já reescrito para `/api/images/...`. Não criar backend novo. | Aceito | Reuso do endpoint existente, `pageSize=6`. |
| A3 | N chamadas HTTP por card (uma por biblioteca) podem saturar. | Parcial | Cache por `libraryKey` em `Map` no módulo; evita refetch. Sem backend novo. |
| A4 | Imagens do proxy Plex exigem o `thumb` não nulo. | Aceito | Filtro `.filter(s => s.thumb)` antes de montar o mosaico. |
| A5 | `AbortController` não estava nos globals do ESLint. | Aceito | Adicionado em `eslint.config.js`. |

## Revisor S — SOLID e coesão

| # | Achado | Classificação | Ação |
|---|--------|---------------|------|
| S1 | Lógica de fetch + animação no componente viola SRP. | Aceito | Extraído `useLibraryMosaic.js` (dados/animação) e `LibraryMosaic.vue` (apresentação). |
| S2 | Componente recebe `series` pré-carregado mas nunca usa. | Rejeitado | Mantido como prop opcional para extensão futura; documentado como `ponytail:` (sem custo atual). |
| S3 | Acoplamento direto a `AppIcon` no fallback. | Aceito | Reuso do componente existente em vez de duplicar SVG. |

## Revisor C — Clean Code, escopo e testabilidade

| # | Achado | Classificação | Ação |
|---|--------|---------------|------|
| C1 | Nomes claros (`posters`, `isHovered`, `startHover`, `stopHover`). | Aceito | Mantido. |
| C2 | Testar shuffle sem acoplar a timers. | Aceito | Teste Playwright compara ordem de `background-image` antes/depois do hover. |
| C3 | Respeitar `prefers-reduced-motion`. | Aceito | Guarda no composable + `@media` no CSS. |
| C4 | Fallback neutro quando sem séries. | Aceito | `.mosaic-fallback` com `color: var(--text-tertiary)`. |

## Achados aceitos que alteraram requisito

- A1 (view correta) e A4 (filtro de thumb) foram incorporados antes de implementar.
- Nenhuma mudança de escopo; `openspec instructions apply` reexecutado após o commit.
