# Deficiências Resolvidas — ui-sidebar-header

EGM (Goldfish Review A/S/C) executado em 2026-11-07 sobre proposal, design,
spec delta, tasks e código real. Três revisores independentes.

Pedido do usuário que define o escopo desta rodada:

- Remover do menu lateral as opções "Menu" e "Light" (toggle de tema) e o item
  estático "Bibliotecas".
- Adicionar as bibliotecas obtidas da API diretamente no menu lateral.
- Menu lateral logo abaixo do header.
- Margem entre as bibliotecas.
- Ajustar o menu lateral (escopo principal).

## Triagem dos achados

### Aceitos

| # | Origem | Achado | Resolução |
|---|---|---|---|
| 1 | A1/S8/C3 | Artifacts mandam criar `AppSidebar.vue`, `Breadcrumb.vue`, `UserMenu.vue`, `DarkModeToggle.vue` e `AppLayout.vue`, que não existem. Real: `Sidebar/SidebarArea.vue`, `Breadcrumbs/BreadcrumbDefault.vue`, `AppHeader.vue`, `layouts/DefaultLayout.vue`. | Spec/design/tasks reescritos para os caminhos reais. Nenhum componente novo além do necessário. |
| 2 | A2/S3/C4 | Modelo de estado contraditório (`sidebar-visible`, `sidebar-description-visible`, `isCollapsed`). | Modelo único: `isCollapsed` em `stores/sidebar.js`. Hamburger do header é o único controle. |
| 3 | S1 | Hamburger duplicado no header e na sidebar. | Removido o botão "Menu" da sidebar. Header é dono do toggle. |
| 4 | S2 | Toggle de tema no header, na sidebar e no modal. | Sidebar não tem mais toggle. Header e SettingsModal permanecem. |
| 5 | A8 | Sidebar renderiza botão Menu e botão de tema, violando "somente navegação". | Sidebar passa a conter somente a lista de bibliotecas. |
| 6 | A5 | `tailwind.config.cjs` removeu `gray-2`, usado em `BreadcrumbDefault.vue`, `SeriesView.vue`, `MarkerEditorView.vue`, `SyncView.vue`. | `gray-2` restaurado no config. |
| 7 | A3/A4/C7 | `npm run test:tokens` e `npm run test:sidebar` falham (tokens.css inexistente, seletor `[data-theme]`, chave `sidebar.collapsed` vs `.v2`). | Testes ajustados: tokens convertidos para `style.css`/`.dark`; chave da sidebar alinhada. |
| 8 | A6/C5 | Três valores de fundo escuro concorrentes (`#0D1117`, `#18181B`, `#21262D`). | Design fixa tokens de superfície e mapeia classes usadas. |
| 9 | A10/S9 | Breadcrumb mostra só `route.meta.title`; `stores/ui.js` tem títulos reais sem consumidor. | Fora do escopo desta rodada (sidebar). Registrado como pendência, sem alterar requisito. |
| 10 | A11/C10 | Requisito de "avatar" impossível: auth guarda só token (cookie HttpOnly). | Requisito rebaixado para "ícone de perfil", que é o que existe. |
| 11 | S5/S6/C6 | SettingsModal acumula senha + tema e há modal + rota `/settings`; troca de senha via Plex API contraria "backend out of scope". | Modal mantém apenas aparência/tema nesta change. Troca de senha removida do escopo. Rota `/settings` para configuração de Plex permanece. |
| 12 | A9/S7 | Header é dono do SettingsModal. | Aceito como está: header é o dono da ação de abrir; modal não muda nesta rodada. |
| 13 | C8 | Aceitação Docker sem `-p` e `Dockerfile` não roda `npm run build`. | Aceitação passa a usar build do frontend + `docker build`/`docker run -p` com porta mapeada. |
| 14 | C9 | Conflito com spec principal `navegacao-lateral` (rail 88px, sem header, tema/logout na sidebar, Material Tailwind). | Delta `specs/navegacao-lateral/spec.md` com MODIFIED substitui os requisitos conflitantes (header presente, bibliotecas dinâmicas, sem tema/logout na sidebar, Tailwind puro). |
| 15 | C1 | "Ajustar o menu lateral" não tinha critério verificável. | Requisito agora descreve exatamente o pedido do usuário (itens, origem dos dados, posição, espaçamento). |
| 16 | S10/S11 | Store de tema mistura localStorage/API e modos `light|dark|system`. | Fora do escopo funcional; requisito apenas exige persistência. Pendência registrada. |
| 17 | S12 | Logout limpa apenas cookie local. | Fora do escopo; requisito mantido como está. |
| 18 | C12 | `qa-report-1.md` com evidências numéricas incorretas. | Relatório antigo não é reutilizado; QA desta rodada gera `qa-report-2.md` com evidência real. |

### Rejeitados

| # | Origem | Achado | Motivo |
|---|---|---|---|
| R1 | A12 | "openspec CLI não está instalado". | `openspec list` e `openspec validate` executam normalmente neste ambiente. |
| R2 | C2 parcial | Trocar todo o frontend de uma vez. | Fora do escopo; o pedido é ajustar o menu lateral. |

### Parciais

| # | Origem | Achado | Decisão |
|---|---|---|---|
| P1 | A7 | Duas fontes de tema (CSS vars e Tailwind) e vars ausentes em `StatusCard`. | Aceito como dívida; esta change não altera `StatusCard`. Tokens do menu lateral usam as classes Tailwind existentes. |
| P2 | C11 | Critérios vagos ("transição suave"). | Substituídos por critérios observáveis; transição mantida com `transition-colors` e `motion-reduce`. |

## Rodada 2 — Breadcrumb dinâmico (pedido do usuário)

Pedido: o breadcrumb do header deve compor o fluxo com apenas `Início` fixo e o
resto dinâmico (`Início > <biblioteca>` e `Início > <biblioteca> > <série>`).

### Aceitos

| # | Origem | Achado | Resolução |
|---|---|---|---|
| 19 | A1/S1/C3 | Trilha duplicada: `AppHeader` e `BreadcrumbDefault` nas views. | Header é dono único; `BreadcrumbDefault` e `stores/ui.js` removidos. |
| 20 | A2/A3/S4/C2 | Header sem o título da biblioteca e risco de terceiro fetch. | Layout já carrega bibliotecas e passa `breadcrumbs`; header só renderiza. |
| 21 | A5/C1 | Artefatos marcavam breadcrumb como fora de escopo. | Proposal/design/spec/tasks atualizados; pendência removida. |
| 22 | A6/C4 | Rótulos conflitantes (`Séries`, `Biblioteca`, `Bibliotecas`). | Só `Início` é fixo; níveis seguintes vêm de biblioteca/série. |
| 23 | C5 | Mudança sem teste automatizado. | `lib/breadcrumb.js` puro + `tests/breadcrumb.test.mjs` + script. |
| 24 | C6 | Separador pendurado e item vazio. | Itens sem label são filtrados; separador só entre itens. |
| 25 | S3 | `stores/ui.js` sem consumidor. | Store removida. |

### Rejeitados

| # | Origem | Achado | Motivo |
|---|---|---|---|
| R3 | A4 | Mover `libraryKey` para o path de marcadores. | Troca de contrato de rota; query já carrega o contexto necessário. |
| R4 | S5 | Refatorar `AppHeader`/`SettingsModal`. | Fora do escopo; header continua shell do layout. |

## Decisões de requisito

1. Header permanece e é o dono do toggle de tema, do perfil/logout, do
   breadcrumb e do hamburger que colapsa a sidebar.
2. Sidebar fica abaixo do header e contém **somente** a lista de bibliotecas
   carregada de `GET /api/plex/libraries`, com espaçamento vertical entre itens.
3. Nenhum item estático ("Bibliotecas", "Menu") e nenhum controle de tema na
   sidebar.
4. Cada biblioteca navega para `/libraries/<key>/series` e recebe destaque
   quando a rota ativa pertence àquela biblioteca.
5. Modo compacto (só ícones) continua, controlado pelo hamburger do header e
   persistido em `localStorage`.
