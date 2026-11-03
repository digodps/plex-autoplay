# Design Doc — Refatoração da camada de apresentação (cinza, claro/escuro, minimalista)

## 1. Contexto e problema

O painel Vue (`vue/plex-frontend`, Vue 3 + Pinia + vue-router + Vite) funciona,
mas a camada de apresentação cresceu sem desenho:

- `App.vue` concentra o shell e aplica estilo inline em cada elemento
  (`style="..."` em `aside`, `nav`, `footer`, botão Sair, `main`).
- CSS fragmentado em vários arquivos sem fonte única de tokens: `style.css`
  (2 linhas), `app-shell.css`, `redesign.css`, `components/marker-row.css`,
  `views/marker-editor.css`, `views/sync-view.css`, `views/SeriesView.css`.
- Cor de destaque padrão amarela (`#e5a00d`) e redesign.css com `--accent` roxo
  (`#a855f7`) — incompatíveis com a exigência de tons de cinza.
- Restos do scaffold Vite: `HelloWorld.vue`, `App.vue.bak`, `assets/vue.svg`,
  `assets/vite.svg`.
- Aplicação de tema vive dentro de `DashboardView.vue` (`applyTheme`), então
  login/settings não recebem tema; dark mode não tem efeito real no shell
  (nenhuma regra CSS responde a `.theme-dark`).
- Estados de carregando/vazio/erro tratados de forma desigual entre telas.

O change `tema-paleta` (implementado, 14/14 tasks) já entregou a infraestrutura:
store Pinia `theme` (`accent`, `mode`), `GET/PUT /api/theme`, tabela
`UserThemePreferences`. Esta mudança **preserva o contrato HTTP** e altera a
camada visual e a semântica das preferências (ver §5.4).

## 2. Objetivos

1. Tema claro e escuro em escala de cinza (monocromático), aplicado globalmente
   e consistente em todas as telas (inclusive login e settings).
2. Interface minimalista, moderna e funcional: hierarquia por contraste e
   espaço, sem ornamento.
3. Fonte única de tokens de estilo; zero estilo inline em componente.
4. Camada de apresentação com dependências em um sentido só, verificada por
   lint automatizado (não só por revisão humana).

## 3. Não-objetivos

- Alterar backend .NET além do valor padrão de `accent` (ver §5.4).
- Trocar framework ou adotar biblioteca de UI (Tailwind, Vuetify, PrimeVue).
- Redesenhar fluxos de negócio ou telas.
- Alterar o contrato HTTP (`GET/PUT /api/theme`) nem o schema do SQLite.

## 4. Arquitetura da camada de apresentação

O grafo abaixo é **normativo** e deve ser codificado em lint (§4.1). Regra: só
se pode importar "para dentro".

Matriz "quem pode importar quem" (linha importa coluna):

| camada → | views | components | composables | stores | lib | api.js |
|---|---|---|---|---|---|---|
| **views** | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ |
| **components** | ✗ | ✓ | ✗ | ✗ | ✓ | ✗ |
| **composables** | ✗ | ✗ | ✓ | ✓ | ✓ | ✗ |
| **stores** | ✗ | ✗ | ✗ | ✓ | ✓ | ✓ |
| **lib** | ✗ | ✗ | ✗ | ✗ | ✓ | ✗ |
| **api.js** | ✗ | ✗ | ✗ | ✗ | ✗ | ✓ |

Regras derivadas (contrato, não exceção):

- **views/**: compõe a tela a partir de `composables` e `stores`. **Proibido**
  `fetch`, efeito de rede e `import api.js`. Hoje `DashboardView.vue`,
  `LibrariesView.vue`, `SyncView.vue` e `LoginView.vue` violam isso — cada uma
  ganha composable (`useDashboardScan`, `useLibrariesList`, `useSyncScan`,
  `useLogin`).
- **components/**: apresentação pura — props, emits, slots e `lib`.
  **Proibido** importar `stores`, `composables`, `api.js` e `vue-router`
  (`useRoute`/`useRouter`). Hoje `MarkerRow.vue` importa `useMarkerStore` e
  `StatusCard.vue`/`Breadcrumb.vue` usam `useRoute`/`useRouter` — passar dados
  por props e mover a derivação de rota para a view.
- **composables/**: orquestra estado de tela chamando `stores`. **Proibido**
  `api.js` direto. Hoje `useLibrarySeries.js` e `useSeriesCatalog.js` mantêm
  estado próprio e chamam `api.js` sem store — vira store dedicada
  (`stores/series.js`) e o composable só orquestra.
- **stores/**: fonte de verdade; fala com `api.js`. **Proibido** importar
  `composables`. Hoje `stores/dashboard.js` importa
  `composables/useDashboardAggregators.js` (invertido) — o arquivo vira
  `src/lib/aggregators.js`.
- **lib/**: funções puras e helpers sem reatividade (agregação, formato,
  auth/token).
- **entrypoints** (`main.js`, `router/index.js`): exceção documentada — podem
  importar `lib/auth.js` (helpers de token/cookie) e `api.js` para bootstrap.
  `getToken`/`setCookie` saem de `api.js` para `src/lib/auth.js`; `api.js` fica
  só com HTTP.

### 4.1 Enforcement automatizado

Adicionar `eslint-plugin-boundaries` (devDependency) e declarar as fronteiras
no `eslint.config.js` **na raiz do repo** (já cobre o frontend com
`eslint-plugin-vue`; não existe config dentro de `vue/plex-frontend`). Adicionar
script `"lint": "eslint src --config ../../eslint.config.js"` ao
`vue/plex-frontend/package.json`. Violação vira erro de lint, reprovando a task
e o CI.

## 5. Tokens e tema

`src/styles/tokens.css` é a única fonte de valores visuais:

```css
:root { /* tokens claro (default) */ }
[data-theme="dark"] { /* tokens escuro */ }
@media (prefers-color-scheme: dark) {
  :root:not([data-theme]) { /* tokens escuro (fallback pre-JS) */ }
}
```

Precedência: `data-theme` explícito **sobre** `prefers-color-scheme`.

### 5.1 Escala de cinza (neutra)

| Token | Claro | Escuro |
|---|---|---|
| `--bg` | `#fafafa` | `#0a0a0a` |
| `--surface` | `#ffffff` | `#171717` |
| `--surface-raised` | `#f5f5f5` | `#1f1f1f` |
| `--border` | `#e5e5e5` | `#262626` |
| `--border-strong` | `#d4d4d4` | `#404040` |
| `--border-input` | `#737373` | `#737373` |
| `--text` | `#171717` | `#fafafa` |
| `--text-secondary` | `#525252` | `#a3a3a3` |
| `--text-tertiary` | `#616161` | `#8c8c8c` |
| `--accent` | `#404040` | `#d4d4d4` |
| `--accent-contrast` | `#fafafa` | `#171717` |
| `--focus-ring` | `#171717` | `#fafafa` |
| `--danger` | `#b91c1c` | `#f87171` |
| `--success` | `#15803d` | `#4ade80` |
| `--warning` | `#b45309` | `#fbbf24` |

Regras de uso:

- **Texto**: primário/secundário/terciário fixados para ≥ 4.5:1 sobre o fundo
  onde aparecem. `--text-tertiary` claro `#616161` (≥4.5:1 sobre
  `--surface-raised #f5f5f5`); escuro `#8c8c8c` (≥4.5:1 sobre `--surface`).
- **Não-texto** (bordas, ícones, controles, anel de foco): ≥ 3:1 contra o fundo
  adjacente (WCAG 1.4.11). Bordas decorativas (`--border`, `--border-strong`)
  **não** podem ser a única delimitação de input — inputs usam `--border-input`
  (`#737373`, ≈4.7:1 no claro e ≈3.7:1 no escuro).
- **`--accent`** é nível de contraste (cinza médio), não matiz: usado em item
  ativo, link e destaque, **sempre distinto** de `--text` e combinado com peso
  de fonte e/ou subfundo.
- **`--focus-ring`** é fixo por tema e **independente** de preferência do
  usuário (o anel nunca usa accent salvo).
- `danger`/`success`/`warning`: únicas cores não-cinza, só para significado
  semântico (claro ≥4.8:1; escuro ≥7:1); ícones/bordas semânticos ≥3:1.

Apoio: `--radius-sm: 6px`, `--radius-md: 8px`, `--radius-lg: 12px`,
`--space-1..8` (4–32px), `--target-min: 44px`, `--transition-fast: 150ms ease`,
`--shadow-sm` única (apenas elevação real; remover `--shadow-1/2/card/hover` do
redesign.css).

### 5.2 Tema claro/escuro — resolução sem FOUC

- Tema aplicado como `data-theme="light"|"dark"` no `<html>`.
- **Pré-paint**: script inline **bloqueante** no `<head>` do `index.html` lê
  `theme.mode` do `localStorage` (fallback `system`), resolve `system` via
  `matchMedia('(prefers-color-scheme: dark)')` e seta
  `document.documentElement.dataset.theme` **antes** do bundle.
- **Pós-mount**: `themeStore.loadTheme()` busca `/api/theme` (verdade
  multi-dispositivo) e reconcilia o `data-theme`. `localStorage` é só cache de
  primeira pintura.
- Remover `applyTheme` de `DashboardView.vue` e o bloco hardcoded
  `@media (prefers-color-scheme: dark)` de `sync-view.css`; aplicação passa a
  ser global (shell/`main.js`), não por rota.
- `@media (prefers-reduced-motion: reduce)` em `base.css` zerando
  transições/animações (WCAG 2.3.3).

### 5.3 Controles de tema na UI

- Toggle de modo (claro/escuro/sistema) no **shell (`App.vue`)**, visível em
  todas as rotas; opcionalmente replicado em `SettingsView`.
- Sem seleção de accent na UI (§5.4).

### 5.4 Decisão: preferência do usuário = apenas `mode`

Resolve a contradição do `accent` único vs. token por tema:

- `accent` deixa de ser preferência e vira **token de sistema** (§5.1). A UI não
  oferece mais seletor de cor.
- `accentPalette` de `DashboardView.vue` (amber/blue/green/purple/pink/indigo)
  é **removida**; default amarelo `#e5a00d` e roxo `#a855f7` do redesign.css
  são **removidos**.
- Contrato `GET/PUT /api/theme` preservado (campo `accent` aceito/retornado por
  compatibilidade), mas ignorado na renderização; default do backend passa de
  `#e5a00d` para `#171717`. `mode` é a única preferência efetiva.
- Consumidores a alterar: `stores/theme.js`, `DashboardView.vue`,
  `redesign.css`, seed/default do backend.

## 6. Estrutura de arquivos alvo

```
src/styles/
  tokens.css  base.css  shell.css
  components/{button,card,field,table,state}.css
src/lib/
  aggregators.js   # de composables/useDashboardAggregators.js
  auth.js          # getToken/setCookie saídos de api.js
src/composables/
  useDashboardScan.js  useLibrariesList.js  useSyncScan.js  useLogin.js
src/stores/
  series.js (novo)  theme.js (ajustado)
src/components/
  StatusCard.vue  StatusCardSkeleton.vue  StatusCardError.vue  (já existem — reusar)
```

`style.css`, `app-shell.css`, `redesign.css` são absorvidos e deletados.
Imports por view (`marker-editor.css`, `sync-view.css`, `SeriesView.css`)
permanecem, usando só tokens.

## 7. Shell (App.vue)

- Remove 100% dos atributos `style`.
- Marcação: `<div class="shell">` → `<aside class="sidebar">` +
  `<main class="shell-main">`.
- Nav: `<nav class="nav">` + `<RouterLink class="nav-item">`, ativo por
  `.nav-item--active` (usa `--accent` + peso, não só cor).
- Toggle de modo no shell; emoji só decorativo em
  `<span aria-hidden="true">`, com `aria-label` no botão.
- Sidebar colapsa em ≤ 900px.
- `:focus-visible` com `--focus-ring`; alvo de toque mínimo `--target-min`
  (44px) nos controles; controles pequenos ampliam alvo via padding (área útil
  24px, alvo 44px).

## 8. Estados funcionais padronizados

Já existem `StatusCard.vue`, `StatusCardSkeleton.vue` e `StatusCardError.vue`.
**Reusar esses** em vez de criar componentes novos: não duplicar nomes nem
markup. Se faltar capacidade (ex.: ação de retry no estado de erro), estender
`StatusCardError.vue` com um `emit("retry")`.

Regra: toda view que busca dados usa os três; nenhuma inventa markup próprio.
Estado de carregamento usa skeleton com `aria-busy="true"`; erro tem botão
"Tentar de novo".

## 9. Limpeza

Deletar: `src/App.vue.bak`, `src/components/HelloWorld.vue`,
`src/assets/vue.svg`, `src/assets/vite.svg`, `src/style.css`,
`src/app-shell.css`, `src/redesign.css`.

## 10. Verificação (critérios de aceite)

1. `npm run build` passa sem erro.
2. `npm run lint` sem erro/aviso novo, **incluindo** a regra de fronteira
   (`eslint-plugin-boundaries`) que rejeita import proibido (§4).
3. Nenhum `style="` em `src/**/*.vue`, exceto binds dinâmicos justificados
   (ex.: largura de progresso).
4. Nenhum hex literal em `.vue`/CSS de componente fora de `tokens.css`, exceto
   `danger`/`success`/`warning` e valores de dados.
5. Contraste: **todo texto ≥ 4.5:1** (incl. tertiary e pequenos); **não-texto**
   (borda de input, ícones, controles, foco) **≥ 3:1** contra fundo adjacente.
6. `prefers-reduced-motion` respeitado.
7. Nenhum arquivo de `views/` ou `components/` importa `api.js`.
8. Nenhum `components/` importa `stores/`, `composables/` ou `vue-router`;
   nenhum `stores/` importa `composables/`.
9. Tema aplicado antes do primeiro paint (`data-theme` no `<html>`, sem FOUC)
   em login/settings/dashboard.
10. Testes e2e Playwright existentes passam (login, navegação, dashboard).
11. `index.html` com `lang="pt-BR"`.

## 11. Riscos

| Risco | Mitigação |
|---|---|
| Quebrar telas ao remover CSS antigo | Migração arquivo por arquivo + build/e2e por etapa; tag `pre-apresentacao-cinza` no `dc8fa0d` |
| Flash de tema (FOUC) | Script inline bloqueante no `<head>` (§5.2) |
| Contraste insuficiente no cinza | Pares fixados por token (§5.1); borda de input ≥3:1 |
| Escopo virar redesenho de telas | Só apresentação; markup de negócio preservado |
| Fronteira só humana | `eslint-plugin-boundaries` reprova no lint/CI (§4.1) |
| Perda de identidade visual do Plex | Decisão explícita do usuário: monocromático |

## 12. Ordem de execução

1. Tokens + base + shell + script inline de tema no `index.html` +
   `lang="pt-BR"`.
2. `App.vue` sem inline styles + toggle de modo no shell.
3. `src/lib/` (aggregators, auth) e realocação de imports; store `series`.
4. Componentes compartilhados (button, card, field, table, state).
5. Views migradas uma a uma (extrair `api.js` para composables; componentes sem
   store/router), com build entre cada.
6. Configurar `eslint-plugin-boundaries` e passar o lint.
7. Limpeza (§9) e verificação final.
