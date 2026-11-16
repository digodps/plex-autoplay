# Tasks

## 1. Setup & Asset Copy

- [ ] 1.1 Copiar `design-taste-frontend/assets/` → `vue/plex-frontend/public/assets/` — verify: `public/assets/system.css`, `public/assets/system.js`, `public/assets/layout/`, `public/assets/ui/`, `public/assets/motion/`, `public/assets/diagrams/`, `public/assets/moments/`, `public/assets/scenes/` existem
- [ ] 1.2 Atualizar `index.html` com meta tags Taste: `<html data-type="software">`, `<meta name="site-kind" content="software">`, `<meta name="page-type" content="software">`, link `system.css`, script `system.js` — verify: meta tags presentes, CSS/JS carregam sem 404
- [ ] 1.3 Rodar `deal.mjs` e `start.mjs` para gerar paleta e layout recipe — verify: `.taste-skill/` criado com `palette.css`, `layout-recipe.json`, `history.json`
- [ ] 1.4 Rodar `icons.mjs scan` para gerar ícones reais — verify: `public/assets/icons/plex.svg` (laranja), flags, ui icons presentes

## 2. Design Tokens & Base Styles

- [ ] 2.1 Remover Tailwind config customizado (`tailwind.config.cjs`) — verify: arquivo removido ou substituído por minimal config apenas para utilities não-cobertas pelo Taste
- [ ] 2.2 Remover CSS custom properties duplicadas em componentes (AppHeader, DashboardView, etc.) — verify: zero `--accent`, `--radius`, `--speed`, `--ease`, `--shadow-*` definidos em `<style scoped>`
- [ ] 2.3 Garantir `system.css` tokens são a única fonte: `--accent`, `--radius`, `--speed`, `--ease`, `--fontStack-*`, `--h1`..`--h6`, `--p-h`, `--wall`, `--rail`, `--device`, `--surface`, `--text*`, `--border`, `--accent-bg`, `--danger`, `--success`, `--warning` — verify: `:root` e `[data-theme="dark"]` têm todos tokens
- [ ] 2.4 Configurar `system.js` para theme toggle (light/dark/system) e persistência — verify: `themeStore.cycleMode()` usa `system.js` API, persiste em localStorage, respeita `prefers-color-scheme`

## 3. Component Migration — Header & Navigation

- [ ] 3.1 Substituir `AppHeader.vue` por Taste `nav-cards` recipe (`assets/layout/nav-cards.html`) — verify: header tem logo (wordmark), nav cards para seções principais, theme toggle, user menu, language selector
- [ ] 3.2 Substituir `Breadcrumb.vue` por Taste breadcrumb pattern (se aplicável) ou remover se nav-cards cobre — verify: breadcrumb não duplica nav, ou usa `.breadcrumb` classes do system.css
- [ ] 3.3 User menu: migrar para Taste dropdown pattern (close-type) — verify: abre/fecha click outside, ESC fecha, focus trap, aria-expanded correto
- [ ] 3.4 Language selector: migrar para Taste select pattern — verify: abre/fecha, seleção troca locale, flag icons reais, estado ativo destacado
- [ ] 3.5 Theme toggle: usar `system.js` API, ícones sun/moon reais — verify: ciclo light→dark→system, persiste, ícones de `public/assets/icons/`

## 4. View Migration — Dashboard

- [ ] 4.1 Refatorar `DashboardView.vue` para hero type-led + section métricas (Taste software pattern) — verify: hero 1 screen (h1 "Início", subtitle, primary action "Iniciar Escaneamento" pill button), section métricas 1 screen (h2 "Métricas do Sistema", grid 2x2 cards)
- [ ] 4.2 Substituir `StatusCard.vue` por Taste `feature-card` ou `panel-fragment` — verify: mesmo card style, card loop 3s (pulsing diagram para service/sync), hover lift, focus ring
- [ ] 4.3 Substituir `ActionBar.vue` por Taste button pair (primary pill + secondary tonal) — verify: "Iniciar Escaneamento" primary, "Ver Log Completo" secondary, mesmo shape
- [ ] 4.4 Theme controls no hero: mover para header (já em 3.1), remover do Dashboard — verify: header tem theme toggle + accent palette se aplicável
- [ ] 4.5 Dealt moment: metric-cards pulsing diagram (hub/lanes) na section métricas — verify: `data-diagram="hub"` ou similar, pulsa 3s, pause hover/reduced-motion

## 5. View Migration — Libraries

- [ ] 5.1 Refatorar `LibrariesView.vue` para hero photograph-led + section library-grid — verify: hero 1 screen (photograph server room, h1 "Bibliotecas", subtitle), section 1-2 screens (h2 "Suas Bibliotecas", grid auto-fill minmax(280px,1fr))
- [ ] 5.2 Library cards: Taste `feature-card` recipe — verify: icon tv (real Plex icon?), h3 title, "Ver séries" link (verb-object), hover lift 2px, equal edges
- [ ] 5.3 Empty state dentro da section (não separado) — verify: se 0 bibliotecas, mensagem + illustration dentro da section grid

## 6. View Migration — Series

- [ ] 6.1 Refatorar `SeriesView.vue` para hero photograph-led + section series-list — verify: hero 1 screen (tv collage photo, h1 = library title), section 1-2 screens (h2 "Séries")
- [ ] 6.2 Series list: cards ou list items com Taste pattern — verify: thumbnail, title, episode count, "Ver detalhes" action
- [ ] 6.3 Catálogo não sincronizado: mensagem + botão "Forçar sincronização" dentro da section — verify: reutiliza endpoint existente, loading state, success/error feedback, recarrega lista
- [ ] 6.4 Dealt moment: nenhum (software mas não product page)

## 7. View Migration — SeriesDetail (SeriesPage)

- [ ] 7.1 Refatorar `SeriesPage.vue` para hero product (app-frame) + sections episódios + marcadores — verify: hero 1-2 screens (app-frame com player UI screenshot/video), h1 = series name
- [ ] 7.2 Section episódios: EpisodeList migrado para Taste — verify: h2 "Episódios", cards ou list, EpisodeItem cards com hover lift
- [ ] 7.3 Section marcadores: MarkerEditor migrado — verify: h2 "Marcadores de Intro/Créditos", app-frame para editor
- [ ] 7.4 Dealt moment: picture-develops (film) na section episódios — verify: `assets/moments/_demo/in-page-service.html` adaptado

## 8. View Migration — Settings

- [ ] 8.1 Refatorar `SettingsView.vue` para hero type-led + sections config — verify: hero 1 screen (h1 "Configurações"), sections: Geral, Plex, Sincronização, Avançado (cada 1 screen)
- [ ] 8.2 Form fields: Taste `starter.html` form pattern — verify: labels, inputs, selects, switches com system.css tokens, focus ring, error states
- [ ] 8.3 Dealt moment: pinned-chapters na section principal — verify: chapters fixos no scroll, progress indicator

## 9. View Migration — Logs

- [ ] 9.1 Refatorar `LogsView.vue` para hero type-led + section log-carousel/accordion — verify: hero 1 screen (h1 "Logs"), section 1-2 screens (h2 "Logs de Processamento")
- [ ] 9.2 Log carousel/accordion: Taste `photo-chapters` moment — verify: `assets/moments/_demo/in-page.html` (photo-chapters) adaptado para logs, navegação teclado, auto-advance pause hover
- [ ] 9.3 Dealt moment: photo-chapters (log-carousel) — verify: momento próprio section, pause reduced-motion

## 10. View Migration — MarkerEditor

- [ ] 10.1 Refatorar `MarkerEditorView.vue` para hero product (app-frame) + section editor — verify: hero 1-2 screens (app-frame editor UI), h1 "Editor de Marcadores"
- [ ] 10.2 Section editor: Taste panel fragments — verify: h2 "Marcadores", timeline, inputs HH:MM:SS, preview player
- [ ] 10.3 Dealt moment: picture-develops — verify: film strip developing animation

## 11. View Migration — Sync

- [ ] 11.1 Refatorar `SyncView.vue` para hero type-led + section sync status — verify: hero 1 screen (h1 "Sincronização"), section 1 screen (h2 "Status da Sincronização")
- [ ] 11.2 Botões: pill primary "Sincronizar Agora", tonal secondary "Ver Log" — verify: mesmo button pair
- [ ] 11.3 Progress/feedback: Taste loading states — verify: spinner, progress bar, success/error toast

## 12. Composables & Stores Migration

- [ ] 12.1 `useThemeStore`: migrar para `system.js` API — verify: `cycleMode()`, `setMode()`, `accent` property, persistência, SSR-safe
- [ ] 12.2 `useUIStore`: consolidar menu states (profile, language) — verify: single source of truth, Taste dropdown patterns
- [ ] 12.3 `useDashboardCards`, `useSeriesCatalog`, etc.: remover estilos, manter apenas lógica — verify: zero CSS em composables, retornam dados puros
- [ ] 12.4 `apiFetch`, stores: sem alterações de API, apenas consumo — verify: endpoints existentes funcionam

## 13. Icon System Integration

- [ ] 13.1 Substituir todos `AppIcon` por `<img src="/assets/icons/{name}.svg" data-icon="{name}" class="icon" />` — verify: zero `AppIcon` imports, zero `lucide-vue-next`, zero `@heroicons/vue`
- [ ] 13.2 CSS `.icon` utility: `width: 1em; height: 1em; display: inline-block; vertical-align: middle;` — verify: ícones escalam com font-size, colors via `currentColor` ou SVG fill nativo
- [ ] 13.3 Plex icon: laranja oficial (#E5A00D) no SVG — verify: `public/assets/icons/plex.svg` tem fill="#E5A00D"

## 14. Motion Layer Implementation

- [ ] 14.1 Hero entrance: `[data-enter]` em toda hero section — verify: `assets/motion/hero-enter.css` aplicado, fade+slide-up 600ms, reduced-motion instant
- [ ] 14.2 Button hover/press/focus: `assets/motion/button.css` — verify: lift 2px, scale 0.98, ring 2px accent
- [ ] 14.3 Linked card/photo zoom: `assets/motion/linked-card.css` — verify: zoom 1.02-1.03, 200ms ease
- [ ] 14.4 Card loops: `assets/motion/card-loop.css` + JS — verify: cycle 3s, pause hover, pause `prefers-reduced-motion`
- [ ] 14.5 Dealt moments: copiar `assets/moments/_demo/in-page.html` e `in-page-service.html` — verify: photo-chapters (Logs), picture-develops (SeriesDetail, MarkerEditor), pinned-chapters (Settings)
- [ ] 14.6 Zero forbidden motion: audit — verify: zero tilt, zero scroll-reveal, zero parallax, zero animation >5s sem pause

## 15. Copy & Voice Audit

- [ ] 15.1 Revisar todo texto visível contra `copy.md`/`copy-voice.md` — verify: 60-80 palavras/screen, headings=nouns, buttons=verb-object, 1 hard fact hero, no slop words
- [ ] 15.2 Marcar assumed facts com `data-assumed` — verify: ex: "Sincronização a cada 5 min" → `data-assumed`
- [ ] 15.3 Rodar `copy-lint.mjs` — verify: zero FAIL, WARN justificados

## 16. Validation Gates

- [ ] 16.1 `npm ci && npm run build` em `vue/plex-frontend` — verify: exit 0, dist/ gerado
- [ ] 16.2 `npm run lint` em `vue/plex-frontend` — verify: exit 0, zero errors/warnings
- [ ] 16.3 `node scripts/copy-lint.mjs index.html --kind software` — verify: PASS
- [ ] 16.4 `node scripts/consistency.mjs --widths 1440,1024,820,768,390 --rhythm-widths 1440,820,768,390 --page software --category "Plex Autoplay" --primary "Iniciar Escaneamento"` — verify: PASS
- [ ] 16.5 `node scripts/check.mjs --url http://localhost:5173 --sizes 1440x900,1024x768,820x1180,768x1024,390x844 --profile .taste-skill/chrome-check` — verify: PASS
- [ ] 16.6 `node scripts/shoot.mjs --url http://localhost:5173 --w 1440 --out shots/1440.png --profile .taste-skill/chrome-check` — verify: screenshots salvas
- [ ] 16.7 `node scripts/perf.mjs index.html --profile .taste-skill/chrome-check` — verify: PASS

## 17. QA UI Obrigatória (Playwright)

- [ ] 17.1 Acessar `http://localhost:5173` (ou porta do Vite dev server) com Playwright — verify: página carrega sem erros console
- [ ] 17.2 Validar tema: toggle light→dark→system, persistência, ícones sun/moon — verify: 3 estados funcionam, localStorage persist, CSS vars aplicados
- [ ] 17.3 Validar menus: user menu (abre/fecha click outside/ESC, navega settings/change-password/logout), language selector (abre/fecha, seleciona pt/en/es, flag icon, estado ativo) — verify: todos abrem/fecham, navegação funciona, aria correto
- [ ] 17.4 Validar navegação: Dashboard→Libraries→Series→SeriesDetail→Settings→Logs→MarkerEditor→Sync — verify: header ativo, breadcrumb correto (se houver), transições suaves
- [ ] 17.5 Validar hero entrance: cada view tem `data-enter` animation no load — verify: fade+slide-up visível
- [ ] 17.6 Validar card loops: Dashboard métricas pulsam, pause hover/reduced-motion — verify: cycle 3s, pause funciona
- [ ] 17.7 Screenshots evidência: salvar em `openspec/changes/frontend-refactor-taste/qa-evidence-theme-light.png`, `qa-evidence-theme-dark.png`, `qa-evidence-menu-open.png`, `qa-evidence-libraries.png`, etc. — verify: arquivos existem

## 18. OpenSpec Validation & Archive

- [ ] 18.1 `openspec validate frontend-refactor-taste --strict` — verify: PASS
- [ ] 18.2 Commit artifacts: `git add openspec/changes/frontend-refactor-taste && git commit -m "docs(openspec): add frontend-refactor-taste artifacts"` — verify: commit na branch worktree
- [ ] 18.3 Push worktree branch e criar PR — verify: PR criado com relatório HTML