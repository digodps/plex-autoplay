# QA Report 1 — remover-inicio-breadcrumb

## Veredito

**PASS** — 42/42 asserts Playwright, 12/12 testes unitários, `openspec validate --strict` válido.

## Contexto

- QA independente no worktree `/home/hermes/plex-autoplay-remover-inicio-breadcrumb`, branch `feature/spec-remover-inicio-breadcrumb`.
- Nenhum código de produção, artefato de spec ou `tasks.md` foi alterado. Apenas o script de QA e este relatório foram criados.
- Backend real indisponível: `page.route("**/api/**")` com mocks determinísticos e cookie `auth_token=qa`.
- Sidebar: não existe no layout atual (`DefaultLayout.vue` só tem header + main), portanto o item 8 de sidebar é N/A.

## URL acessada

- `http://localhost:8092` (Vite preview do build de produção, porta fixa).
- Rotas: `/libraries`, `/libraries/1/series`, `/dashboard`, `/series/42/markers?libraryKey=1&libraryTitle=Animes&seriesTitle=Aosh`, `/settings`.

## Comandos executados

```bash
node /tmp/opencode/qa-remover-inicio-breadcrumb.mjs
npm --prefix vue/plex-frontend run test:breadcrumb
openspec validate remover-inicio-breadcrumb --strict
```

Resultados:

- Script Playwright: `RESULTADO: 42/42 asserts — PASS`
- `test:breadcrumb`: `# tests 12 # pass 12 # fail 0`
- `openspec validate`: `Change 'remover-inicio-breadcrumb' is valid`

## Asserts e resultados

| # | Assert | Resultado |
|---|---|---|
| 1a | `/libraries`: único item `Bibliotecas` | PASS |
| 1b | `/libraries`: `Bibliotecas` é `<span aria-current="page">` | PASS |
| 1c | `/libraries`: texto da trilha exatamente `Bibliotecas` (sem `Início`) | PASS |
| 1d | `/libraries`: exatamente 1 chevron | PASS |
| 1e | `/libraries`: chevron é o primeiro filho do item | PASS |
| 1f | `/libraries`: sem texto `/` nem `Início` | PASS |
| 2a | `/libraries/1/series`: texto `Bibliotecas Animes` | PASS |
| 2b | `/libraries/1/series`: 2 itens na ordem | PASS |
| 2c | `/libraries/1/series`: `Bibliotecas` é link com `href="/libraries"` | PASS |
| 2d | `/libraries/1/series`: `Animes` é `<span aria-current="page">` | PASS |
| 2e | `/libraries/1/series`: 2 chevrons, um antes de cada item | PASS |
| 2f | `/libraries/1/series`: sem `/` nem `Início` | PASS |
| 3a | Cabeçalho: leitura `Plex Autoplay` → `Bibliotecas` → `Animes` | PASS |
| 3b | Cabeçalho: chevron antes de cada item da trilha | PASS |
| 3c | Cabeçalho: sem texto `/` | PASS |
| 4a | `/dashboard`: `nav[aria-label="Breadcrumb"]` ausente | PASS |
| 4b | `/dashboard`: nenhum chevron órfão no header | PASS |
| 4c | `/dashboard`: logo com `aria-label` contendo `Plex Autoplay` | PASS |
| 4d | `/dashboard`: logo com `href="/"` | PASS |
| 5a | Clique em `Bibliotecas` a partir de `/libraries/1/series` leva a `/libraries` | PASS |
| 5b | Item atual atualiza para `Bibliotecas` | PASS |
| 6a | `/series/42/markers`: texto `Bibliotecas Animes Aosh` | PASS |
| 6b | Marcadores: 3 níveis, `Aosh` com `aria-current="page"` | PASS |
| 6c | Marcadores: `Animes` é link com `href="/libraries/1/series"` | PASS |
| 6d | Marcadores: 3 chevrons | PASS |
| 7a | Tema: estado inicial `light`, sem classe `dark` | PASS |
| 7b | Toggle aplica `data-theme="dark"`, classe `dark`, `localStorage["theme.mode"]="dark"` | PASS |
| 7c | Header com cor de fundo diferente (`light` vs `dark`) | PASS |
| 7d | Breadcrumb com cor de texto diferente (`light` vs `dark`) | PASS |
| 7e | Tema `dark` persiste após reload | PASS |
| 7f | Toggle reverte para `light` (dataset, classe e localStorage) | PASS |
| 8a | Menu do usuário fechado inicialmente (`aria-expanded="false"`) | PASS |
| 8b | Menu abre (`aria-expanded="true"`) | PASS |
| 8c | Link `Configurações` visível no dropdown | PASS |
| 8d | Menu fecha após navegar | PASS |
| 8e | Dropdown removido do DOM após navegar | PASS |
| 8f | Navegação para `/settings` | PASS |
| 8g | Trilha de `/settings` mostra `Ajustes` com `aria-current="page"` | PASS |
| 8h | Menu reabre pelo gatilho | PASS |
| 8i | Menu fecha ao clicar no gatilho | PASS |
| 8j | Sidebar inexistente no layout (N/A) | PASS |
| 9a | Sem erros relevantes de console/pageerror | PASS |

## Evidências capturadas

Textos/atributos observados:

- `/libraries`: `["Bibliotecas"]` — `span aria-current="page"`.
- `/libraries/1/series`: `[{"text":"Bibliotecas","tag":"A","href":"/libraries"},{"text":"Animes","tag":"SPAN","aria-current":"page"}]`.
- Header: tokens `["Plex Autoplay","Bibliotecas","Animes"]`, 2 chevrons, sem `/`.
- `/series/42/markers`: `Bibliotecas Animes Aosh`, 3 chevrons, `Animes` link `/libraries/1/series`.
- Tema: `light` → `dark` → reload `dark` → `light`; `localStorage["theme.mode"]` acompanha.
- `/dashboard`: `nav[aria-label="Breadcrumb"]` count `0`, chevrons `0`, logo `aria-label="Plex Autoplay — início"`, `href="/"`.

Screenshots:

- `/tmp/opencode/qa-remover-inicio-01-libraries.png`
- `/tmp/opencode/qa-remover-inicio-02-series.png`
- `/tmp/opencode/qa-remover-inicio-03-dashboard.png`
- `/tmp/opencode/qa-remover-inicio-04-markers.png`
- `/tmp/opencode/qa-remover-inicio-05-theme-light.png`
- `/tmp/opencode/qa-remover-inicio-06-theme-dark.png`
- `/tmp/opencode/qa-remover-inicio-07-settings.png`

## Achados

1. **`tasks.md` não reflete o trabalho concluído.** As caixas 1.1, 1.2, 2.1, 3.1, 3.2, 3.3 e 3.4 continuam `[ ]`, embora código, testes, build e validação estejam presentes e passando. QA não editou o arquivo por instrução. Ação esperada: marcar as caixas em `openspec/changes/remover-inicio-breadcrumb/tasks.md`.
2. **Fechamento do menu do usuário ao clicar fora não existe.** O dropdown só fecha ao navegar, ao clicar no gatilho ou ao escolher uma ação. O requisito aceita "após navegar" e o comportamento foi validado; sem falha. Se "clicar fora" virar requisito, ajustar `vue/plex-frontend/src/components/AppHeader.vue`.
3. **Sidebar não aplicável.** O layout atual (`vue/plex-frontend/src/components/layouts/DefaultLayout.vue`) não possui menu lateral; asserts de sidebar foram registrados como N/A.
