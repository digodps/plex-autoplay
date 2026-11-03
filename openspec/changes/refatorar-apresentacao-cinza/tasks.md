# Tasks

## 1. Fundação visual — tokens, base, tema sem FOUC

- [x] 1.1 Criar `src/styles/tokens.css` com `:root` (claro), `[data-theme="dark"]`, fallback `@media (prefers-color-scheme: dark) :root:not([data-theme])`, e todos os tokens da tabela §5.1 (bg/surface/raised, border/border-strong/border-input, text/secondary/tertiary, accent/accent-contrast, focus-ring, danger/success/warning, radius, space, target-min, transition, shadow-sm) e verificar que os pares de contraste texto ≥4.5:1 e não-texto ≥3:1 estão corretos
- [x] 1.2 Criar `src/styles/base.css` com reset, tipografia, `:focus-visible` usando `--focus-ring`, `@media (prefers-reduced-motion: reduce)` e importar os dois no ponto de entrada
- [x] 1.3 Criar `src/styles/shell.css` com layout de sidebar/brand/nav/footer/main (absorvendo as regras reais de `app-shell.css`, incluindo `.app-shell` grid e colapso mobile) usando só tokens
- [x] 1.4 Adicionar script inline **bloqueante** no `<head>` do `index.html` que lê `theme.mode` do `localStorage`, resolve `system` via `matchMedia`, e seta `document.documentElement.dataset.theme` antes do bundle; trocar `lang="en"` por `lang="pt-BR"`
- [x] 1.5 Remover a aplicação de tema de `DashboardView.vue` (`applyTheme`, `toggleDarkMode`, `accentPalette`) e implementar aplicação global (`data-theme` resolvido em `stores/theme.js` + `main.js`), incluindo listener de `prefers-color-scheme` para `mode=system`; verificar que o tema aplica em login/settings/dashboard sem FOUC

## 2. Shell (App.vue) sem inline styles

- [x] 2.1 Remover todos os atributos `style="..."` de `App.vue` e substituir por classes (`shell`, `sidebar`, `brand`, `nav`, `nav-item`, `nav-item--active`, `shell-main`, footer, botão Sair) definidas em `shell.css`
- [x] 2.2 Adicionar toggle de modo claro/escuro/sistema no shell, com emoji decorativo em `<span aria-hidden="true">` e `aria-label` no botão; verificar visível em todas as rotas
- [x] 2.3 Garantir `:focus-visible` em todo interativo e alvo de toque mínimo `--target-min` (44px) nos controles do shell

## 3. Camada lib/ e realocação de imports

- [x] 3.1 Mover `composables/useDashboardAggregators.js` para `src/lib/aggregators.js` e atualizar os imports (inclusive `stores/dashboard.js`, que não pode mais importar de `composables/`)
- [x] 3.2 Extrair `getToken`/`setCookie` (e demais helpers de token/cookie) de `api.js` para `src/lib/auth.js` e atualizar `main.js` e `router/index.js` para importar de `lib/auth.js`
- [x] 3.3 Criar `stores/series.js` como fonte de verdade e refatorar `useLibrarySeries.js`/`useSeriesCatalog.js` para orquestrarem a store sem chamar `api.js` diretamente
- [x] 3.4 Extrair a lógica de rede das views para composables (`useDashboardScan`, `useLibrariesList`, `useSyncScan`, `useLogin`) e remover `import api.js` de `DashboardView.vue`, `LibrariesView.vue`, `SyncView.vue`, `LoginView.vue`
- [x] 3.5 Remover `useMarkerStore` de `MarkerRow.vue` (props/emits) e `useRoute`/`useRouter` de `StatusCard.vue`/`Breadcrumb.vue` (derivar na view e passar por prop); verificar que nenhum componente importa store/composable/vue-router

## 4. Componentes compartilhados e estados

- [x] 4.1 Criar/ajustar CSS de componentes (`button.css`, `card.css`, `field.css` com `--border-input`, `table.css`, `state.css`) usando só tokens
- [x] 4.2 Padronizar estados vazio/carregando/erro usando os `StatusCard`/`StatusCardSkeleton`/`StatusCardError` existentes em todas as views que buscam dados (skeleton com `aria-busy="true"`; erro com "Tentar de novo" via `emit("retry")`)
- [x] 4.3 Verificar `html lang="pt-BR"` e ausência de emoji como ícone de conteúdo fora de `aria-hidden`

## 5. Migração de views para tokens

- [x] 5.1 Converter todos os hex literais de `views/*.css` e demais CSS de componente para `var(--token)` (38 hex hoje em `sync-view.css`, `redesign.css`, `app-shell.css`, etc.), mantendo `danger`/`success`/`warning` como únicos valores literais permitidos
- [x] 5.2 Remover o bloco hardcoded `@media (prefers-color-scheme: dark)` de `sync-view.css` e os tokens de sombra/acento do `redesign.css`, migrando para tokens
- [x] 5.3 Verificar `npm run build` entre cada view migrada

## 6. Enforcement de fronteira (lint)

- [x] 6.1 Adicionar `eslint-plugin-boundaries` como devDependency do frontend e declarar as fronteiras da matriz §4 no `eslint.config.js` da raiz
- [x] 6.2 Adicionar script `"lint": "eslint src --config ../../eslint.config.js"` ao `vue/plex-frontend/package.json` e corrigir toda violação reportada
- [x] 6.3 Verificar `npm run lint` sem erro/aviso novo

## 7. Limpeza e verificação final

- [x] 7.1 Deletar `src/App.vue.bak`, `src/components/HelloWorld.vue`, `src/assets/vue.svg`, `src/assets/vite.svg`, `src/style.css`, `src/app-shell.css`, `src/redesign.css` (após migração)
- [x] 7.2 Verificar `npm run build` passa e `grep -rn 'style="' src/**/*.vue` sem resultados (exceto binds dinâmicos justificados)
- [x] 7.3 Rodar testes e2e Playwright existentes (`auth`, `container`, `images`, `libraries`, `markers`) e confirmar que passam
- [x] 7.4 Atualizar default de `accent` no backend de `#e5a00d` para `#171717`
