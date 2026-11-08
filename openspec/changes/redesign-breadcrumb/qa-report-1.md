# QA Report 1 — redesign-breadcrumb

## Veredito

**PASS**

## URL acessada

- Build de produção servido por `npm --prefix vue/plex-frontend run preview -- --port 8092 --strictPort`
- Base: `http://localhost:8092`
- Rotas validadas: `/dashboard`, `/libraries`, `/libraries/1/series`, `/series/42/markers`, `/settings`

## Comandos executados

```bash
cd /home/hermes/plex-autoplay-redesign-breadcrumb
npm --prefix vue/plex-frontend run preview -- --port 8092 --strictPort   # background
node /tmp/opencode/qa-breadcrumb-redesign.mjs
```

O preview foi encerrado ao final (porta 8092 sem processo escutando).

## Implementação do teste

- Playwright real (`chromium` de `/home/hermes/plex-autoplay-redesign-breadcrumb/node_modules/playwright/index.mjs`), headless.
- Cookie de sessão: `auth_token=qa` para `http://localhost:8092`.
- `context.route("**/api/**")` com mocks determinísticos: setup/status, status, theme (GET light / PUT dark), bibliotecas (`Animes`, `Filmes`), séries da biblioteca `1` (`Aosh`, ratingKey `42`), markers `[]`, episódios `[]`, fallback `{}`.
- Viewport desktop `1280x900`; viewport mobile `390x844`.
- Total de chamadas de API mockadas: 57.

## Resultado dos checks

**34/34 asserts passaram; 0 erros JS de página/console.**

| Check | Resultado |
|---|---|
| a) `/dashboard`: 1 item `Início`, atual, 0 setas | PASS |
| b) `/libraries`: `Início`, `Bibliotecas`, atual, 1 seta, sem `/` como separador | PASS |
| c) `/libraries/1/series`: `Início`, `Bibliotecas`, `Animes`, 2 setas, hrefs corretos, `Animes` atual, sem item extra | PASS |
| d) card `Aosh`: `/series/42/markers?...`, 4 itens, 3 setas, `Animes` href `/libraries/1/series`, `Aosh` atual | PASS |
| e) navegação da trilha: `Animes` → `/libraries/1/series`, `Bibliotecas` → `/libraries`, `Início` → `/dashboard` | PASS |
| f) tema alterna `data-theme` e `localStorage["theme.mode"]`; tema propagado para séries e marcadores | PASS |
| g) menu do usuário abre/navega `/settings`/fecha; `aria-expanded` da sidebar alterna; biblioteca ativa com `aria-current="page"` | PASS |
| h) mobile 390x844: 4 rótulos na ordem, nav com rolagem horizontal, header íntegro, sem overflow horizontal | PASS |
| i) sem erros JS de página | PASS |

## Tema — confirmação explícita

- Estado inicial em `/libraries/1/series`: `data-theme="light"`, `localStorage["theme.mode"]="light"`.
- Clique no botão de tema do header: `data-theme="dark"`, `localStorage["theme.mode"]="dark"`.
- Navegação para `/series/42/markers`: página recebeu `data-theme="dark"`, `class="dark"` e `localStorage["theme.mode"]="dark"`.
- Retorno para claro pelo header validado antes dos testes de menu.

## Menus — confirmação explícita

- Menu do usuário abriu (`aria-expanded="true"`) e exibiu o link `Configurações`.
- Clique em `Configurações` navegou para `/settings` e o menu fechou (link removido do DOM).
- Botão de sidebar alternou de `aria-label="Recolher menu"` / `aria-expanded="true"` para `aria-label="Expandir menu"` / `aria-expanded="false"`, e foi restaurado.
- Item da sidebar da biblioteca ativa (`/libraries/1/series`, `Animes`) tem `aria-current="page"`.

## Docker acceptance

- Build limpo: `docker build --no-cache -t plex-autoplay:redesign-breadcrumb .` — PASS.
- Container isolado: `docker run -d --name qa-breadcrumb-img -p 8093:80 -v /tmp/opencode/qa-breadcrumb-data:/data -e WEB_PORT=80 -e DISABLE_HTTP=false -e SQLITE_CONNECTION_STRING='Data Source=/data/app.db' -e APP_CONFIG_PATH=/data/app-config.json plex-autoplay:redesign-breadcrumb`.
- `GET http://127.0.0.1:8093/settings` — HTTP 200.
- Bundle servido pelo container: `index-Dz7vdPeb.js`, igual ao build validado no preview.
- Observação: execução sem volume `/data` terminou com exit 139 na inicialização do schema; com o volume persistente do compose o container sobe normalmente. O compose já monta `./data:/data`.

## Achados

Nenhum. Veredito PASS.

## Screenshots

- `/tmp/opencode/qa-breadcrumb-dashboard.png`
- `/tmp/opencode/qa-breadcrumb-libraries.png`
- `/tmp/opencode/qa-breadcrumb-series.png`
- `/tmp/opencode/qa-breadcrumb-markers.png`
- `/tmp/opencode/qa-breadcrumb-dark-series.png`
- `/tmp/opencode/qa-breadcrumb-dark-markers.png`
- `/tmp/opencode/qa-user-menu-open.png`
- `/tmp/opencode/qa-breadcrumb-mobile-markers.png`

## Script

- `/tmp/opencode/qa-breadcrumb-redesign.mjs`
