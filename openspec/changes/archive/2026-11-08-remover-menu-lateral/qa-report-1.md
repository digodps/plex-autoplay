# QA Report 1 — remover-menu-lateral

**Verdict: PASS** (all mandatory spec scenarios pass; 1 non-blocking UX gap reported)

- URL testada: `http://127.0.0.1:18086`
- Container: `plex-qa-remover-menu`
- Imagem: `plex-autoplay:remover-menu-lateral` (`sha256:1df21720bb65d3ae4f2664d9dd9bfc23db261b973951a13ac3bc8af87223b0b5`)
- Execução: 2026-11-08, Chromium via Playwright 1.65.0 (browser real, viewport 1280x900 e 390x844)
- Escopo: `specs/web-ui/spec.md`, `specs/navegacao-lateral/spec.md`, `design.md`, `tasks.md`
- Nenhum código de aplicação ou artefato de spec foi modificado. Scripts temporários em `/tmp/opencode/`.

## Build / validação estática

| Item | Resultado | Evidência |
| --- | --- | --- |
| `openspec validate remover-menu-lateral --strict` | PASS — `Change 'remover-menu-lateral' is valid` | saída do comando |
| Artefato servido `index-DEfIxDNH.js` | PASS — `HTTP 200`, 118295 bytes; CSS `index-Y0wfcyHf.css` referenciado | `/tmp/opencode/qa-index.html`, `curl -I` |
| Bundle sem resíduos de sidebar (`Recolher menu`, `Expandir menu`, `sidebar.collapsed`) | PASS — 0 ocorrências | `/tmp/opencode/bundle.js` |

## Cenários executados

1. **Shell sem menu lateral** — PASS. Em `/dashboard`, `/libraries`, `/libraries/1/series`, `/series/999/markers?...`, `/sync`, `/settings`: `aside` = 0, elementos `[class*=sidebar]`/`[data-sidebar]`/`[aria-label*=sidebar]` = 0. Top bar contém apenas 2 botões: `Tema escuro|Tema claro` e `Menu do usuário`; nenhum botão de expandir/recolher. Estrutura `header + main` confirmada (`header+main=1`).
2. **Tema** — PASS. Toggle visível e único. Light: `data-theme="light"`, header `rgb(255,255,255)`; clique → `data-theme="dark"`, classe `dark`, header `rgb(24,24,27)`, cor de texto muda; segundo clique → light restaurado (`rgb(255,255,255)`, `data-theme="light"`). `aria-label` acompanha o estado (`Tema claro` no dark, `Tema escuro` no light).
3. **Menu do usuário** — PASS parcial quanto a fechamento. Abre com `aria-expanded="true"`, itens `Configurações`, `Alterar senha`, `Sair` presentes. `Configurações` navega para `/settings`, URL confirma, dropdown fecha (`aria-expanded="false"`, zero links do menu). Re-clique fecha (`aria-expanded="false"`). **Clique fora não fecha** e `Escape` não fecha (ver Achado 1).
4. **Breadcrumb** — PASS. `/libraries`: `Início` (link `/dashboard`) + `Bibliotecas` com `aria-current="page"`. `/series/999/markers?libraryKey=1&libraryTitle=QA%20Library&seriesTitle=QA%20Series`: `Início / Bibliotecas / QA Library / QA Series`, com `Bibliotecas` → `/libraries` e `QA Library` → `/libraries/1/series`; item atual `QA Series` com `aria-current="page"`. Clique em `Bibliotecas` navega para `/libraries`.
5. **390px** — PASS. `documentElement.scrollWidth == clientWidth == 390` em `/dashboard`, `/libraries`, `/series/999/markers?...`, `/settings`; top bar sticky (top 0, altura 49px), toggle de tema e menu do usuário dentro do viewport (x+width ≈ 374) e menu abre. Breadcrumb trunca sem quebrar linha.
6. **Erros de página** — nenhum `pageerror` durante o fluxo.

## Achados

1. **Dropdown do usuário não fecha com clique fora nem com `Escape`** (não bloqueante).
   - Esperado: fechar ao clicar fora do menu / pressionar `Escape`.
   - Obtido: permanece aberto (`aria-expanded="true"`) ao clicar no conteúdo principal e ao pressionar `Escape`; fecha apenas por re-clique no botão ou após navegação.
   - Causa provável: `AppHeader.vue` só alterna `showProfile` no clique do botão; não há listener de documento/focusout.
   - Impacto de spec: nenhum. `web-ui` exige apenas que as ações continuem disponíveis e o toggle visível, e a instrução de QA aceita "fecha no clique fora **ou** no re-clique" — re-clique funciona. Recomendação: listener `pointerdown` no documento + `Escape` quando `showProfile`.
   - Evidência: `/tmp/opencode/qa-remove-menu-confirm.cjs` (saída `outside`/`escape`), `/tmp/opencode/qa-remover-menu-results.json`, `qa-evidence/03-user-menu-open.png`.
2. **Ambiente de QA sem bibliotecas/série reais → fallback genérico do breadcrumb** (informativo, comportamento previsto).
   - `/libraries/1/series` mostra `Biblioteca` e `/series/999/markers?libraryKey=1` mostra `Série` (sem `libraryTitle`/`seriesTitle` e sem catálogo carregado). Fallback previsto em `tasks.md` 1.4; com os query params do roteiro, a trilha completa é exibida corretamente.
   - Evidência: `/tmp/opencode/qa-remove-menu-confirm.cjs` (bloco `perRoute`).
3. **Checkboxes de `tasks.md` não marcados** (processo, não código).
   - Itens 1.1–2.3 seguem `[ ]` apesar de o comportamento estar implementado e validado. Atualizar antes do arquivamento da change.
   - Evidência: `openspec/changes/remover-menu-lateral/tasks.md`.
4. **Overflow interno de 5px no estado de erro de `/series/999/markers`** (informativo, não é regressão do shell).
   - Um `span.ml-2` com payload JSON bruto do erro 502 mede até x=395 dentro de `main`, mas `main` tem `overflow-x-hidden` e o documento permanece em 390px (`body/doc scrollWidth = 390`). Sem overflow horizontal de página.
   - Evidência: `/tmp/opencode/qa-overflow-probe.cjs`, `qa-evidence/06-breadcrumb-markers.png`.

## Screenshots

- `openspec/changes/remover-menu-lateral/qa-evidence/01-light-1280.png` — tema claro, shell sem sidebar
- `openspec/changes/remover-menu-lateral/qa-evidence/02-dark-1280.png` — tema escuro aplicado
- `openspec/changes/remover-menu-lateral/qa-evidence/03-user-menu-open.png` — dropdown com as 3 ações
- `openspec/changes/remover-menu-lateral/qa-evidence/04-mobile-390.png` — 390px sem overflow
- `openspec/changes/remover-menu-lateral/qa-evidence/05-mobile-390-menu-open.png` — menu utilizável em 390px
- `openspec/changes/remover-menu-lateral/qa-evidence/06-breadcrumb-markers.png` — trilha `Início / Bibliotecas / QA Library / QA Series`

## Nota de segredos

A senha de administração foi lida de `/home/hermes/plex-autoplay/.env` em tempo de execução pelo script e usada apenas no preenchimento do formulário de login. **Nenhum segredo foi escrito neste relatório, em prompts, em nomes de arquivos ou em evidências.**
