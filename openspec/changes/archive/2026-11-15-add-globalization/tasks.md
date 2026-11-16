# Tasks: Globalization (i18n)

## Fase 1: Setup e Configuração Base

- [ ] **T1.1** Instalar `vue-i18n@11` (compatível com Vue 3.5+)
  ```bash
  cd vue/plex-frontend && npm install vue-i18n@11
  ```

- [ ] **T1.2** Criar estrutura de pastas `src/i18n/locales/`
- [ ] **T1.3** Criar `src/i18n/utils.js` com:
  - Mapeamento `navigator.language` → códigos suportados
  - Função `detectLocale()`
  - Função `loadLocaleMessages(code)` para lazy loading

- [ ] **T1.4** Criar `src/i18n/index.js`:
  - Configurar `createI18n` com legacy: false (Composition API)
  - Locale padrão: `pt-BR`
  - Fallback: `pt-BR`
  - Carregar `pt-BR` eager, `en` e `es` lazy
  - Exportar `i18n` instance

- [ ] **T1.5** Atualizar `src/main.js`:
  - Importar `i18n` de `@/i18n`
  - `app.use(i18n)`

## Fase 2: Store de Locale

- [ ] **T2.1** Criar `src/stores/locale.js` (Pinia):
  - State: `locale`, `availableLocales`
  - Actions: `setLocale(code)`, `init()`
  - Persistência no `localStorage` (chave: `app.locale`)
  - Sincronizar com `i18n.global.locale.value`
  - Atualizar `document.documentElement.lang`

- [ ] **T2.2** Chamar `useLocaleStore().init()` no `main.js` após `app.use(i18n)`

## Fase 3: Componente LanguageSelector

- [ ] **T3.1** Criar `src/components/LanguageSelector.vue`:
  - Dropdown com botão trigger (bandeira + nome)
  - Lista de opções com bandeiras
  - A11y: ARIA attributes, navegação por teclado
  - Fechar ao clicar fora / Escape
  - Usar `useLocaleStore()`

- [ ] **T3.2** Adicionar `LanguageSelector` no `AppHeader.vue`:
  - Importar componente
  - Posicionar entre breadcrumb e botão de tema
  - Responsivo (mobile: full-width dropdown)

## Fase 4: Arquivos de Tradução (Locales)

- [ ] **T4.1** Criar `src/i18n/locales/pt-BR.json` (completo, ~300+ chaves)
  - Namespaces: nav, auth, dashboard, libraries, series, markers, logs, sync, settings, sessions, common, errors, aria, languageSelector

- [ ] **T4.2** Criar `src/i18n/locales/en.json` (tradução completa)
- [ ] **T4.3** Criar `src/i18n/locales/es.json` (tradução completa)

## Fase 5: Refatoração de Views e Componentes

> **Estratégia**: Substituir strings hardcoded por `t('chave')` ou `{{ $t('chave') }}` nos templates.
> Usar Composition API: `const { t } = useI18n()` em `<script setup>`.

### 5.1 Páginas (Views)

- [ ] **T5.1** `src/views/Authentication/SigninView.vue`
- [ ] **T5.2** `src/views/Authentication/SetupView.vue`
- [ ] **T5.3** `src/views/Authentication/ChangePasswordView.vue`
- [ ] **T5.4** `src/views/Dashboard/DashboardView.vue`
- [ ] **T5.5** `src/views/Libraries/LibrariesView.vue`
- [ ] **T5.6** `src/views/Series/SeriesView.vue`
- [ ] **T5.7** `src/views/SeriesDetail/SeriesPage.vue`
- [ ] **T5.8** `src/views/Markers/MarkerEditorView.vue`
- [ ] **T5.9** `src/views/Logs/LogsView.vue`
- [ ] **T5.10** `src/views/SyncView.vue`
- [ ] **T5.11** `src/views/Pages/SettingsView.vue`
- [ ] **T5.12** `src/views/Sessions/SessionsView.vue`

### 5.2 Componentes Comuns

- [ ] **T5.13** `src/components/AppHeader.vue` (breadcrumb labels, aria-labels, user menu)
- [ ] **T5.14** `src/components/AppLogo.vue` (se houver texto)
- [ ] **T5.15** `src/components/StatusCard.vue`
- [ ] **T5.16** `src/components/LibraryMosaic.vue`
- [ ] **T5.17** `src/components/GenreCarousel.vue`
- [ ] **T5.18** `src/components/MarkerRow.vue`
- [ ] **T5.19** `src/components/PasswordModal.vue`
- [ ] **T5.20** `src/components/CardDataStats.vue`
- [ ] **T5.21** `src/components/Logs/LogFilterBar.vue`
- [ ] **T5.22** `src/components/Logs/LogCarouselView.vue`
- [ ] **T5.23** `src/components/ActionBar.vue`
- [ ] **T5.24** `src/components/HelloWorld.vue`
- [ ] **T5.25** `src/components/StatusCardSkeleton.vue`
- [ ] **T5.26** `src/components/Auths/DefaultAuthCard.vue`
- [ ] **T5.27** `src/components/DataStats/DataStatsOne.vue`

### 5.3 Stores (mensagens de erro, toasts)

- [ ] **T5.28** `src/stores/auth.js`
- [ ] **T5.29** `src/stores/dashboard.js`
- [ ] **T5.30** `src/stores/markers.js`
- [ ] **T5.31** `src/stores/ui.js`
- [ ] **T5.32** `src/stores/series.js`
- [ ] **T5.33** `src/stores/logs.js`

### 5.4 Composables

- [ ] **T5.34** `src/composables/useSeriesCatalog.js`

## Fase 6: Router e Meta Titles

- [ ] **T6.1** Atualizar `src/router/index.js`:
  - Substituir `meta.title` string por `meta.titleKey` (chave i18n)
  - Exemplo: `{ title: 'Dashboard' }` → `{ titleKey: 'nav.dashboard' }`
  - No `beforeEach` guard: `document.title = t(to.meta.titleKey) || 'Plex Autoplay'`
  - Atualizar `document.documentElement.lang` no guard

## Fase 7: Validação e Build

- [ ] **T7.1** Executar `npm run lint` em `vue/plex-frontend`
- [ ] **T7.2** Executar `npm run build` em `vue/plex-frontend`
- [ ] **T7.3** Build completo do projeto: `./scripts/build.sh`
- [ ] **T7.4** Verificar se não há strings hardcoded em pt-BR nos templates (grep por texto português)

## Fase 8: QA UI (Playwright)

- [ ] **T8.1** Testar troca de idioma (pt-BR → en → es → pt-BR)
- [ ] **T8.2** Verificar persistência ao recarregar página
- [ ] **T8.3** Verificar `lang` attribute no `<html>`
- [ ] **T8.4** Verificar tema light/dark ainda funciona
- [ ] **T8.5** Verificar menus (breadcrumb, user menu, language dropdown) funcionando
- [ ] **T8.6** Navegar por todas as rotas e confirmar traduções
- [ ] **T8.7** Acessibilidade: navegação por teclado no seletor de idioma