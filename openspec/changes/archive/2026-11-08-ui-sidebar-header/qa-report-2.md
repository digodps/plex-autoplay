# QA Report 2 — ui-sidebar-header

## Veredito: **PASS**

Data: 2026-11-07  
URL validada: http://localhost:8091  
App: container `qa-breadcrumb` (imagem `plex-autoplay:ui-sidebar-header`)  
Método: verificação independente do código, gates de build/lint/teste, `openspec validate --strict` e Playwright (Chromium) contra a UI real. Nenhum código de produto alterado.

> Este relatório substitui a rodada anterior (FAIL em `http://localhost:8090`), cuja imagem/container estava desatualizada. A imagem atual `plex-autoplay:ui-sidebar-header` e o container `qa-breadcrumb` servem o bundle com `buildBreadcrumb`.

## Escopo validado

- `/libraries/<key>/series` → header `Início > <nome da biblioteca>`
- `/series/<key>/markers` → header `Início > <biblioteca> > <série>`
- Apenas `Início` fixo; demais níveis derivados de rota + bibliotecas carregadas
- Trilha única no header, sem breadcrumb duplicado no conteúdo

## Comandos e resultados

| # | Comando | Resultado |
|---|---|---|
| 1 | `cd vue/plex-frontend && npm run build` | PASS — `✓ 55 modules transformed`, `✓ built in 2.34s` |
| 2 | `npm run lint` | PASS — exit 0 (apenas warnings de configuração `[boundaries]`; nenhum erro) |
| 3 | `npm run test:breadcrumb` | PASS — `# tests 7 / # pass 7 / # fail 0` |
| 4 | `npm run test:tokens` | PASS — `# tests 8 / # pass 8 / # fail 0` |
| 5 | `npm run test:sidebar` | PASS — `# fail 0` |
| 6 | `openspec validate ui-sidebar-header --strict` | PASS — `Change 'ui-sidebar-header' is valid` (exit 0) |
| 7 | `node /tmp/opencode/qa-breadcrumb.mjs` | PASS — exit 0 (JSON abaixo) |

Saída do Playwright:

```json
{
  "ok": true,
  "base": "http://localhost:8091",
  "library": "Animes",
  "crumbs": ["Início /", "Animes"],
  "shots": [
    "qa-series-breadcrumb",
    "qa-sidebar",
    "qa-theme-toggle",
    "qa-markers-breadcrumb"
  ]
}
```

## Checagens de UI (Playwright)

| # | Checagem | Resultado |
|---|---|---|
| 1 | `/libraries/<key>/series` renderiza `Início / <biblioteca>` | PASS — `["Início /", "Animes"]` |
| 2 | `Início` linka para `/dashboard` | PASS — `href="/dashboard"` |
| 3 | Item atual marcado `aria-current="page"` com o título da biblioteca | PASS |
| 4 | Único `<nav aria-label="Breadcrumb">` na rota de séries | PASS — count `1` |
| 5 | `/series/<key>/markers` renderiza `Início > <biblioteca> > <série>` | PASS — `Início / Animes / Aoashi` |
| 6 | Único `<nav aria-label="Breadcrumb">` na rota de marcadores | PASS — count `1` |
| 7 | Sidebar abaixo do header (sem sobreposição) | PASS — `aside.y >= header.y + header.height` |
| 8 | Sidebar apenas com bibliotecas da API (sem "Menu"/"Bibliotecas" estáticos) | PASS |
| 9 | Item ativo único na sidebar (`aria-current="page"`) | PASS — `Animes` |
| 10 | Colapso pelo hamburger + persistência `sidebar.collapsed.v2` | PASS — largura `<=64px`, reload mantém |
| 11 | Toggle de tema claro/escuro aplica `.dark` | PASS |
| 12 | Menu de perfil abre/fecha | PASS |
| 13 | Sem erros de console/pageerror na navegação validada | PASS |

## Evidências

Screenshots atualizados (2026-11-07 18:22–18:23) em `/tmp/opencode/`:

- `/tmp/opencode/qa-series-breadcrumb.png` — `Início / Animes`, trilha única
- `/tmp/opencode/qa-sidebar.png` — sidebar expandida, bibliotecas
- `/tmp/opencode/qa-theme-toggle.png` — tema alternado
- `/tmp/opencode/qa-markers-breadcrumb.png` — `Início / Animes / Aoashi`, trilha única

Script: `/tmp/opencode/qa-breadcrumb.mjs`

## Ausência de legados

- `BreadcrumbDefault`: **nenhuma referência** em `vue/plex-frontend/src`, `tests` ou `dist` (`grep` vazio).
- `stores/ui.js`: **nenhuma referência** em `src`/`tests`; diretório `src/stores` contém apenas `auth.js`, `dashboard.js`, `markers.js`, `sidebar.js`, `theme.js`.
- Container `qa-breadcrumb`: `/app/wwwroot/assets` contém `DefaultLayout-BLEO43SD.js` com a lógica de breadcrumb; `BreadcrumbDefault` count = `0`.

## Achados

Nenhum achado bloqueante. Sem desvios ao escopo solicitado.
