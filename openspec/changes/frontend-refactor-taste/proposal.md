# Proposal

## Why

O frontend atual do Plex Autoplay utiliza um design genérico baseado em Tailwind CSS com cores GitHub-like, mas carece de:
- **Personalidade visual única** — visual "AI-made" genérico sem identidade própria
- **Consistência de sistema** — múltiplos padrões de botões, cards, espaçamentos e interações
- **Hierarquia visual clara** — ausência de "one section per screen", hero entrance, dealt moments
- **Ícones reais** — uso de ícones desenhados (lucide/heroicons) em vez de ícones reais de produtos (Plex, etc.)
- **Micro-interações profissionais** — hover states, press states, card loops, hero entrance
- **Acessibilidade real** — testes em navegador real via Playwright, não apenas inspeção de código

O **Taste Skill** (Leon Lin) fornece um framework completo anti-slop para construir interfaces nível figma.com/x.ai com:
- Um sistema único (one system): uma escala tipográfica, um par de botões, um estilo de card, um raio
- Photographs reais onde caberem; product UI real (app-frame) para software
- Hero entrance único por página + 1-2 dealt moments por página
- Shader effects/scene apenas para tech/gaming/software (nosso caso: software)
- Checker + finish gate que roda em navegador real (Playwright) validando: visual draw, contraste, tap-size, rhythm, motion layer, copy quality

## What Changes

1. **Design System Migration**: Adotar Taste Skill como design system único
   - Tokens: `system.css` + `system.js` (CSS custom properties, grid, buttons, nav, footer, colour)
   - Layout: `assets/layout/` (hero, sections, nav, footer recipes)
   - UI Kit: `assets/ui/` (cards, buttons, diagrams, fragments)
   - Motion: `assets/motion/` (hover, press, focus, nav, tabs, card loops, hero entrance)
   - Icons: `scripts/icons.mjs` para ícones reais em cores originais (Plex, etc.)

2. **Page-Level Refactor** (cada view = uma página com hero + sections):
   - **Dashboard** (`DashboardView.vue`) → hero type-led + 4 metric cards (software pattern)
   - **Libraries** (`LibrariesView.vue`) → hero photograph-led + library cards grid
   - **Series** (`SeriesView.vue`) → hero photograph-led + series list/cards
   - **Series Detail** (`SeriesPage.vue`) → product page pattern (app-frame para player)
   - **Settings** (`SettingsView.vue`) → service pattern
   - **Logs** (`LogsView.vue`) → software pattern com log carousel/accordion
   - **Marker Editor** (`MarkerEditorView.vue`) → software pattern com app-frame
   - **Sync** (`SyncView.vue`) → software pattern

3. **Component Consolidation**:
   - Substituir `AppHeader`, `Breadcrumb`, `AppLogo`, `AppIcon`, `StatusCard`, `ActionBar` por componentes baseados no Taste Kit
   - Um único `Button` (solid|pill|tonal|arrow|block), um único `Card` style
   - Remover CSS scoped duplicado; usar `system.css` tokens + page CSS apenas onde `sections.md` diz

4. **Icon System**:
   - `scripts/icons.mjs scan` → gera `assets/icons/` com SVGs reais (Plex, etc.) em cores originais
   - Substituir todos `AppIcon name="..."` por `<img src="/assets/icons/plex.svg" data-icon="plex" />`

5. **Micro-interactions & Motion**:
   - Hero entrance `data-enter` em toda página
   - Card loops 2.5-4s (cycle on hover, pause on reduced-motion)
   - Button hover (lift 2px), press (scale 0.98), focus-visible ring
   - Linked card/photo zoom 1.02-1.03
   - Zero tilt, zero scroll-linked animation (exceto dealt moments)

6. **Validation Gate**:
   - `gate.mjs --start` antes de qualquer página
   - `copy-lint.mjs` para voice/copy quality
   - `consistency.mjs` para rhythm, edges, heading scale, font load, contrast
   - `check.mjs` Playwright em 1440, 1024, 820, 768, 390
   - `perf.mjs` para image weight, WebGL contexts, layout shift

## Escopo

- **Frontend**: `vue/plex-frontend/` — refactor completo de views, components, stores, composables, CSS
- **Design Assets**: Copiar `design-taste-frontend/assets/` → `vue/plex-frontend/public/assets/`
- **Scripts**: Usar `design-taste-frontend/scripts/` (gate, deal, start, icons, photos, copy-lint, consistency, check, shoot, perf)
- **Backend**: Nenhuma alteração (API endpoints existentes são reutilizados)
- **Testes**: Playwright QA obrigatório em `http://localhost:<port>` validando tema (light/dark) e menus (abrir/navegar/fechar/estado ativo)