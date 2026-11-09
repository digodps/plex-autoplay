# QA Report 2 — redesign-breadcrumb (pós-merge)

## Veredito

**PASS** — 36/36 asserts, 0 erros JS.

## Contexto

- QA independente executado após merge em `origin/main`.
- O `origin/main` removeu o menu lateral (sidebar). Nenhum assert de sidebar foi executado nem é aplicável; a validação cobre apenas header, breadcrumb, tema, menu do usuário e layout mobile.
- Nenhum código-fonte ou task foi alterado por este QA.

## URL acessada

- Build de produção servido por `npm --prefix vue/plex-frontend run preview -- --port 8092 --strictPort`
- Base: `http://localhost:8092`
- Rotas validadas: `/dashboard`, `/libraries`, `/libraries/1/series`, `/series/42/markers`, `/settings`

## Comandos executados

```bash
cd /home/hermes/plex-autoplay-redesign-breadcrumb
npm --prefix vue/plex-frontend run preview -- --port 8092 --strictPort   # background
node /tmp/opencode/qa-breadcrumb-postmerge.mjs
```

Preview encerrado ao final (porta 8092 sem processo escutando).

## Implementação do teste

- Playwright real (`chromium` de `/home/hermes/plex-autoplay-redesign-breadcrumb/node_modules/playwright/index.mjs`), headless.
- Cookie de sessão: `auth_token=qa`.
- `page.route("**/api/**")` com mocks determinísticos:
  - `GET /api/auth/setup/status` → `{ "setupRequired": false }`
  - `GET /api/status` → `{ "status": "ok" }`
  - `GET /api/theme` → `{ "mode": "light" }`; `PUT /api/theme` → 200 (mock mantém o modo salvo para simular persistência real do backend)
  - `GET /api/plex/libraries` → `[{"key":"1","title":"Animes"},{"key":"2","title":"Filmes"}]`
  - `GET /api/series/1` → `{ "items":[{"ratingKey":"42","title":"Aosh","summary":"QA"}], "totalSize":1 }`
  - `GET /api/markers` → `[]`; `GET /api/series/0/42/episodes` → `[]`
  - demais `/api/**` → 200 `{}`
- Viewport desktop `1366x900`; viewport mobile `390x844`.

## Resultado dos checks

**36/36 asserts passaram; 0 erros JS de página.**

| Check | Resultado |
|---|---|
| a) `/dashboard`: trilha só `Início`, atual, 0 setas | PASS |
| b) `/libraries`: `Início > Bibliotecas`, 1 seta, `Bibliotecas` atual, sem separador `/`, `Início` href `/dashboard` | PASS |
| c) `/libraries/1/series`: `Início > Bibliotecas > Animes`, 2 setas, hrefs `/dashboard` e `/libraries`, `Animes` atual | PASS |
| d) clique no card `Aosh`: `/series/42/markers?...`, trilha `Início > Bibliotecas > Animes > Aosh`, 3 setas, `Animes` href `/libraries/1/series`, `Aosh` atual | PASS |
| e) clicar `Animes` → `/libraries/1/series`; `Bibliotecas` → `/libraries`; `Início` → `/dashboard` | PASS |
| f) tema alterna light/dark, persiste em `localStorage["theme.mode"]`, séries e marcadores recebem o tema | PASS |
| g) menu do usuário abre, navega para `/settings`, fecha | PASS |
| h) mobile 390x844: 4 rótulos na ordem, nav com rolagem horizontal (`scrollWidth 259 > clientWidth 233`), sem overflow da página (`scrollWidth 390 = innerWidth 390`) | PASS |
| i) sem erros JS | PASS |

## Tema — confirmação explícita

- Inicial: `data-theme="light"`, `class="dark"` ausente, `localStorage["theme.mode"]="light"`.
- Toggle: `data-theme="dark"`, `class="dark"` presente, `localStorage["theme.mode"]="dark"`.
- Reload em `/libraries/1/series` e navegação para `/series/42/markers`: ambos mantiveram `data-theme="dark"`, `class="dark"` e header com classes `dark:*`.
- Toggle de volta: `data-theme="light"`, `localStorage["theme.mode"]="light"`.

## Menu do usuário — confirmação explícita

- Botão `Menu do usuário` abriu o dropdown com link `Configurações`.
- Clique navegou para `/settings`.
- Dropdown fechou (link removido do DOM).

## Sidebar — não aplicável

O `origin/main` removeu o menu lateral. Não existe `aside.sidebar` no build validado; os checks de sidebar do QA Report 1 não se aplicam a este estado do código.

## Achados

Nenhum. Veredito PASS.

## Screenshots

- `/tmp/opencode/qa-shots/01-dashboard.png`
- `/tmp/opencode/qa-shots/02-libraries.png`
- `/tmp/opencode/qa-shots/03-series.png`
- `/tmp/opencode/qa-shots/04-markers.png`
- `/tmp/opencode/qa-shots/05-series-dark.png`
- `/tmp/opencode/qa-shots/06-markers-dark.png`
- `/tmp/opencode/qa-shots/07-settings.png`
- `/tmp/opencode/qa-shots/08-mobile-markers.png`

## Script

- `/tmp/opencode/qa-breadcrumb-postmerge.mjs`
