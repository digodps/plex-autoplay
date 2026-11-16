# Design

## Context

O frontend atual é uma aplicação Vue 3 + Vite + Tailwind CSS + Pinia + Vue Router + Vue I18n. Possui 8 views principais (Dashboard, Libraries, Series, SeriesDetail, Settings, Logs, MarkerEditor, Sync), header global com breadcrumb, theme toggle, user menu e language selector. O design atual usa Tailwind com paleta GitHub-like (gray scale, primary=var(--accent)), mas sofre de inconsistências: múltiplos padrões de botão, card, spacing, ícones lucide genéricos, ausência de hero entrance, motion layer fragmentado.

O **Taste Skill** (v2.5.1) é um design system completo para AI coding agents que produz interfaces nível profissional (figma.com, x.ai). Para nosso caso (site-kind: software, page-type: software), o Taste prescreve:
- **Hero**: type-led ou photograph-led (1-2 screens), `data-enter` entrance animation
- **Sections**: 1-2 screens cada, h2 primeiro, equal left/right edges
- **Grounds**: dark-band (tech/software), surface-pairs, space
- **Buttons**: um par (solid|pill|tonal|arrow|block) — escolher um e usar consistentemente
- **Cards**: um estilo apenas (feature-card ou panel fragment), card loops 2.5-4s
- **Nav**: mega|centre|cards (software: nav-mega|nav-cards|nav-centre)
- **Footer**: dark|statement|columns (software: footer-dark|footer-statement|footer-columns)
- **Close**: type|word|photo (software: close-type|close-word)
- **Rhythm**: dark-band|surface-pairs|space (software: dark-band|surface-pairs|space)
- **Icons**: real full-colour icons (Plex logo em suas cores oficiais), diagrams para hub/lanes
- **Effects**: um shader ground no hero OU um `[data-visual]` frame (WebGL), 1-2 momentos dealt
- **Motion**: hover layer (lift, zoom, underline draw), hero enter, card loops, dealt moments
- **Copy**: 60-80 palavras por screen fora UI mocks, headings noun, buttons verb-object

## Goals / Non-Goals

**Goals:**
- Migrar 100% do frontend Vue para Taste Skill design system
- Uma única página = hero (1 screen) + sections (1-2 screens cada) com dealt moments
- Um sistema: 1 button pair, 1 card style, 1 radius scale, 1 link look, 1 heading scale
- Ícones reais Plex (cores oficiais) via `icons.mjs scan`
- Hero entrance `data-enter` em toda view
- Card loops 2.5-4s com pause em reduced-motion
- Validação Playwright real: tema light/dark toggle, menus abrir/navegar/fechar/ativo
- Build `npm ci && npm run build` passa, `npm run lint` passa

**Non-Goals:**
- Alterar backend .NET ou endpoints API
- Adicionar novas features de negócio (apenas refactor visual/interação)
- Testar CRUD completo (apenas navegação/interação visual)
- Manter componentes Tailwind legados (AppHeader, Breadcrumb, StatusCard, ActionBar, etc.)

## Decisions

### 1. Stack & Asset Strategy

**Decisão:** Copiar `design-taste-frontend/assets/` → `vue/plex-frontend/public/assets/` e referenciar via `/assets/...`. Usar `starter.html` como base para `index.html`. Manter Vue + Vite + Pinia + Router + I18n; componentes Vue consomem classes/tokens do Taste Kit.

**Rationale:** O Taste Skill suporta "existing project builds in its own stack, components taking starter's markup classes, kit copied into public folder". Isso evita reescrever a arquitetura Vue; apenas substituímos o layer visual.

### 2. Page Type & Site Kind

**Decisão:** `site-kind: software`, `page-type: software` para todas as views (Dashboard, Libraries, Series, SeriesDetail, Settings, Logs, MarkerEditor, Sync). Meta tags: `<html data-type="software">`, `<meta name="site-kind" content="software">`, `<meta name="page-type" content="software">`.

**Rationale:** Somos uma developer tool / SaaS (Plex Autoplay). Taste define software pages com: dark-band grounds, shader effects permitidos, app-frame para product UI, real icons, hero type-led ou photograph-led.

### 3. Deal (Palette & Layout Recipe)

**Decisão:** Rodar `deal.mjs` e `start.mjs` para gerar 3 paletas + 3 pairings, tomar o primeiro. Para software:
- Nav: `nav-mega|nav-cards|nav-centre` → escolher `nav-cards` (simples, cards-based)
- Footer: `footer-dark|footer-statement|footer-columns` → `footer-statement`
- Close: `close-type|close-word` → `close-type`
- Rhythm: `dark-band|surface-pairs|space` → `dark-band`
- Buttons: `solid|pill|tonal|arrow|block` → `pill` (rounded, friendly)
- Logo: `wordmark|initial|arc|ring|stack|dots` → `wordmark`

**Rationale:** Deal mantém história e garante consistência cross-page. `nav-cards` funciona bem para dashboard/libraries grid. `pill` buttons são modernos e acessíveis.

### 4. Hero Strategy per View

| View | Hero Type | Picture Kind | Screens | Dealt Moment |
|------|-----------|--------------|---------|--------------|
| Dashboard | type-led | — | 1 | metric-cards (pulsing diagram) |
| Libraries | photograph-led | landscape (server room) | 1 | — |
| Series | photograph-led | tv-show collage | 1 | — |
| SeriesDetail | product (app-frame) | player UI screenshot | 2 | picture-develops (film) |
| Settings | type-led | — | 1 | pinned-chapters |
| Logs | type-led | — | 1 | log-carousel (photo-chapters) |
| MarkerEditor | product (app-frame) | editor UI | 2 | picture-develops |
| Sync | type-led | — | 1 | — |

### 5. Icon System

**Decisão:** Rodar `node scripts/icons.mjs scan index.html --out public/assets/icons --colour` após build. Substituir `<AppIcon name="..." />` por `<img src="/assets/icons/plex.svg" data-icon="plex" class="icon" />` etc. Ícones: Plex (laranja), settings, user, logout, chevron, sync, play, warning, tv, edit, sun, moon, flag-*, search, filter.

**Rationale:** Taste exige "real full-colour icons (a known product's real icon, never drawn one)". Plex tem logo oficial laranja; flags para idiomas; resto do kit.

### 6. Motion Layer

**Decisão:** Usar `assets/motion/` CSS/JS. Implementar:
- Hero: `[data-enter]` entrance (fade+slide up 20px, 600ms ease-out)
- Buttons: hover lift 2px, press scale 0.98, focus-visible ring 2px accent
- Linked cards/photos: hover zoom 1.02-1.03, transition 200ms
- Card loops: feature-cards cycle 3s, pause on hover, pause on `prefers-reduced-motion`
- Dealt moments: photo-chapters (Logs), picture-develops (SeriesDetail, MarkerEditor), pinned-chapters (Settings)
- Zero tilt, zero scroll-reveal, zero parallax

**Rationale:** Taste motion layer é prescritivo e testável via `check.mjs`. Reduced-motion support é obrigatório (WCAG).

### 7. Copy & Voice

**Decisão:** Aplicar `copy.md` e `copy-voice.md`: 60-80 palavras/screen, headings = nouns, buttons = verb-object, 1 hard fact first screen, no slop words (seamless, effortless, streamline, unlock, empower, etc.), assumed facts marked `data-assumed`.

**Rationale:** Copy quality é gate no `copy-lint.mjs`. Voice própria evita "AI-made" feel.

### 8. Validation Gates

**Decisão:** Pipeline obrigatório antes de merge:
1. `gate.mjs --start` (arm finish gate)
2. `npm ci && npm run build` (build pass)
3. `npm run lint` (lint pass)
4. `copy-lint.mjs index.html --kind software` (copy pass)
5. `consistency.mjs --widths 1440,1024,820,768,390 --rhythm-widths 1440,820,768,390` (rhythm/contrast/heading-scale/font-load pass)
6. `check.mjs --url http://localhost:5173 --sizes 1440x900,1024x768,820x1180,768x1024,390x844` (visual draw, tap-size, overflow, collide pass)
7. `shoot.mjs` screenshots para evidência
8. `perf.mjs` (image weight, WebGL contexts ≤4, layout shift)

**Rationale:** Taste finish gate é o quality checkpoint único. Playwright real browser validation é obrigatório per skill spec.