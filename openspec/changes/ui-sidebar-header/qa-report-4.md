# QA Report 4 — ui-sidebar-header (sidebar flutuante)

## Veredito: **PASS**

Data: 2026-11-07
URL validada: `http://localhost:5174` (Vite dev server com proxy para o backend em `http://127.0.0.1:8080`)

## Método

- Build, lint e testes do frontend na worktree `feature/spec-ui-sidebar-header`.
- `openspec validate ui-sidebar-header --strict`.
- Playwright (Chromium) com login real (`admin`), navegação, tema, colapso e
  menus.
- Docker acceptance com `docker build --no-cache` e execução isolada.

## Comandos e resultados

| # | Comando | Resultado |
|---|---|---|
| 1 | `npm --prefix vue/plex-frontend run build` | PASS — `✓ built in 1.39s` |
| 2 | `npm --prefix vue/plex-frontend run lint` | PASS — exit 0 (apenas warnings de configuração `boundaries`) |
| 3 | `npm --prefix vue/plex-frontend run test:sidebar` | PASS — `# fail 0` |
| 4 | `npm --prefix vue/plex-frontend run test:breadcrumb` | PASS — `# fail 0` |
| 5 | `npm --prefix vue/plex-frontend run test:tokens` | PASS — `# fail 0` |
| 6 | `openspec validate ui-sidebar-header --strict` | PASS — `Change 'ui-sidebar-header' is valid` |
| 7 | `node /tmp/opencode/qa-float.mjs` | PASS — 15/15 checagens |

Saída do Playwright:

```json
{
  "ok": true,
  "base": "http://localhost:5174",
  "checks": [
    { "name": "sidebar top below header with >=8px gap", "pass": true, "evidence": { "sidebarTop": 57, "headerBottom": 49, "gap": 8 } },
    { "name": "sidebar left margin >=8px", "pass": true, "evidence": 8 },
    { "name": "sidebar right margin >=8px", "pass": true, "evidence": { "viewport": 1280, "right": 264, "margin": 1016 } },
    { "name": "sidebar bottom margin >=8px", "pass": true, "evidence": { "viewport": 800, "bottom": 792, "margin": 8 } },
    { "name": "sidebar rounded corners >0", "pass": true, "evidence": "8px" },
    { "name": "sidebar full border >0", "pass": true, "evidence": "1px" },
    { "name": "sidebar does not touch main content", "pass": true, "evidence": { "sidebarRight": 264, "mainLeft": 272, "gap": 8 } },
    { "name": "sidebar width 256 expanded", "pass": true, "evidence": 256 },
    { "name": "library items rendered", "pass": true, "evidence": { "count": 1 } },
    { "name": "library navigation works", "pass": true, "evidence": { "href": "/libraries/1/series", "url": "http://localhost:5174/libraries/1/series" } },
    { "name": "active item state", "pass": true, "evidence": { "count": 1 } },
    { "name": "theme toggle flips dark class", "pass": true, "evidence": { "beforeDark": false, "afterDark": true, "sidebarBgAfter": "rgb(33, 38, 45)" } },
    { "name": "sidebar surface distinct from body in dark", "pass": true, "evidence": { "sidebarBg": "rgb(33, 38, 45)", "bodyBg": "rgb(24, 24, 27)" } },
    { "name": "hamburger collapses sidebar", "pass": true, "evidence": { "beforeW": 256, "afterW": 64 } },
    { "name": "profile menu opens and closes", "pass": true, "evidence": { "logoutVisible": true, "logoutGone": true } }
  ]
}
```

## Evidências

- `/tmp/opencode/qa-sidebar-float.png` — sidebar flutuante expandida.
- `/tmp/opencode/qa-float.mjs` — script Playwright executado.

## Achados

Nenhum achado bloqueante nas checagens de UI, build, lint ou testes.

## Docker acceptance

- `docker build --no-cache -t plex-autoplay:ui-sidebar-header-float .` →
  **exit 0** (`/tmp/opencode/docker-build-float.log`).
- Container isolado `qa-float` em `http://localhost:18084` (imagem
  `plex-autoplay:ui-sidebar-header-float`, porta `18084:80`, dados copiados
  para `/tmp/opencode/qa-float-data`).
- Playwright contra o container: **15/15 PASS**, com as mesmas evidências
  acima (`/tmp/opencode/qa-float-docker.json`, screenshot
  `/tmp/opencode/qa-sidebar-float-docker.png`).

## Veredito final

**PASS** — build, lint, testes, `openspec validate --strict`, Playwright na
UI de desenvolvimento e Docker acceptance na imagem publicada.
