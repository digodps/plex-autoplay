# Spec Delta

## ADDED Requirements

### Requirement: Taste Skill Design System Adoption

O sistema SHALL adotar o Taste Skill (design-taste-frontend) como design system único para todo o frontend Vue.

#### Scenario: Assets copiados e referenciados

- **WHEN** o build roda
- **THEN** `design-taste-frontend/assets/` está copiado para `vue/plex-frontend/public/assets/`
- **AND** `index.html` referencia `/assets/system.css`, `/assets/system.js`, `/assets/layout/`, `/assets/ui/`, `/assets/motion/`, `/assets/diagrams/`, `/assets/moments/`, `/assets/scenes/`

#### Scenario: Tokens CSS custom properties ativos

- **WHEN** a página carrega
- **THEN** `--accent`, `--radius`, `--speed`, `--ease`, `--fontStack-sansSerif`, `--fontStack-display`, `--fontStack-mono`, `--h1`..`--h6`, `--p-h`, `--wall`, `--rail`, `--device`, `--surface`, `--text`, `--text-secondary`, `--border`, `--accent-bg`, `--danger`, `--success`, `--warning` estão definidos no `:root` e `[data-theme="dark"]`

### Requirement: Page Structure — One Section Per Screen

Cada view SHALL seguir a regra "one section per screen": hero sozinho na primeira tela (1 screen = 1440x900), cada section subsequente exatamente 1 ou 2 screens de altura, h2 primeiro no topo da section, nenhum heading de section vizinha espiando.

#### Scenario: Dashboard hero + metrics

- **WHEN** usuário acessa `/`
- **THEN** hero type-led ocupa exatamente 1 screen (1440x900), h1 + subtitle + primary action (pill button)
- **AND** section métricas (grid 2x2 StatusCard) ocupa 1 screen, h2 "Métricas do Sistema"
- **AND** nenhum outro conteúdo invade o hero ou a section

#### Scenario: Libraries hero + grid

- **WHEN** usuário acessa `/libraries`
- **THEN** hero photograph-led (server room landscape) ocupa 1 screen, h1 "Bibliotecas" + subtitle
- **AND** section library-grid ocupa 1-2 screens, h2 "Suas Bibliotecas", cards iguais edges
- **AND** empty state dentro da section, não separado

#### Scenario: Series hero + list

- **WHEN** usuário acessa `/libraries/{key}/series`
- **THEN** hero photograph-led (tv collage) 1 screen, h1 = nome da biblioteca
- **AND** section series-list 1-2 screens, h2 "Séries", cards ou list items
- **AND** se catálogo não sincronizado: mensagem + botão forçar sync dentro da section

#### Scenario: SeriesDetail product page

- **WHEN** usuário acessa `/series/{id}`
- **THEN** hero product (app-frame com player UI) 1-2 screens, h1 = nome da série
- **AND** section episódios (EpisodeList) 1-2 screens, h2 "Episódios"
- **AND** section marcadores (MarkerEditor) 1 screen, h2 "Marcadores de Intro/Créditos"
- **AND** dealt moment: picture-develops (film) na section episódios

#### Scenario: Settings, Logs, MarkerEditor, Sync

- **WHEN** usuário acessa cada rota
- **THEN** hero type-led 1 screen + sections 1-2 screens cada, h2 primeiro
- **AND** dealt moments: Settings → pinned-chapters, Logs → photo-chapters (log-carousel), MarkerEditor → picture-develops, Sync → nenhum

### Requirement: Single System — Button, Card, Radius, Link, Heading

O frontend SHALL usar exatamente: 1 button pair (primary+secondary), 1 card family, 1 radius scale, 1 link look, 1 heading scale.

#### Scenario: Button pair (pill)

- **WHEN** qualquer botão primary renderiza
- **THEN** classe `.btn.pill` (ou equivalente Taste), `--radius` aplicado, hover lift 2px, press scale 0.98, focus-visible ring 2px `--accent`
- **WHEN** botão secondary/ghost renderiza
- **THEN** mesmo shape (pill), tonal ou outline variant, mesmo motion

#### Scenario: Card family (feature-card)

- **WHEN** qualquer card renderiza (StatusCard, LibraryCard, SeriesCard, EpisodeCard)
- **THEN** mesmo `.card` base: `--radius`, `--shadow-1` default, `--shadow-2` hover, lift 2px hover, equal edges
- **AND** card loops: feature-cards cycle 3s (2.5-4s), pause on hover, pause on reduced-motion

#### Scenario: Radius scale

- **WHEN** qualquer elemento arredondado
- **THEN** usa `--radius` (sm/md/lg/xl) do `system.css`, nunca valores hardcoded

#### Scenario: Link look

- **WHEN** qualquer link (RouterLink, <a>) fora de botão
- **THEN** underline draw on hover (motion layer), color `--accent`, no underline default

#### Scenario: Heading scale

- **WHEN** headings renderizam
- **THEN** `--h1`..`--h6` do `system.css` (clamp fluid), fonts: display=Poppins, sans=Roboto, weights 700/600/500

### Requirement: Real Icons System

O sistema SHALL usar ícones reais em cores originais (Plex laranja, flags, etc.) via `icons.mjs scan`.

#### Scenario: Icons gerados e usados

- **WHEN** `node scripts/icons.mjs scan index.html --out public/assets/icons --colour` roda
- **THEN** `public/assets/icons/` contém: `plex.svg` (laranja #E5A00D), `settings.svg`, `user.svg`, `logout.svg`, `chevron-right.svg`, `chevron-down.svg`, `sync.svg`, `play.svg`, `warning.svg`, `tv.svg`, `edit.svg`, `sun.svg`, `moon.svg`, `flag-pt.svg`, `flag-en.svg`, `flag-es.svg`, `search.svg`, `filter.svg`
- **AND** todos `<AppIcon name="..." />` substituídos por `<img src="/assets/icons/{name}.svg" data-icon="{name}" class="icon" />`

#### Scenario: Nenhum ícone desenhado (lucide/heroicons) permanece

- **WHEN** build completa
- **THEN** zero referências a `lucide-vue-next`, `@heroicons/vue`, ou SVGs inline genéricos em componentes Vue

### Requirement: Hero Entrance + Dealt Moments

Toda página SHALL ter hero entrance `data-enter` (uma vez no load) e 1-2 dealt moments em suas próprias sections.

#### Scenario: Hero entrance

- **WHEN** página carrega
- **THEN** hero section tem `[data-enter]` attribute
- **AND** animação: fade-in + slide-up 20px, 600ms ease-out, `prefers-reduced-motion` → instant

#### Scenario: Dealt moments por view

- **WHEN** Dashboard carrega
- **THEN** moment: metric-cards pulsing diagram (hub/lanes) na section métricas
- **WHEN** SeriesDetail carrega
- **THEN** moment: picture-develops (film) na section episódios
- **WHEN** Settings carrega
- **THEN** moment: pinned-chapters na section principal
- **WHEN** Logs carrega
- **THEN** moment: photo-chapters (log-carousel/accordion) na section logs
- **WHEN** MarkerEditor carrega
- **THEN** moment: picture-develops na section marcadores

### Requirement: Motion Layer Compliance

Todas interações SHALL seguir motion layer prescrito: hover lift/zoom, press scale, focus ring, card loops, hero enter, dealt moments. Zero tilt, zero scroll-linked, zero parallax.

#### Scenario: Button interactions

- **WHEN** pointer entra botão
- **THEN** lift 2px (transform translateY(-2px)), transition 150ms ease
- **WHEN** pointer down (click)
- **THEN** scale 0.98, transition 50ms
- **WHEN** focus-visible
- **THEN** ring 2px `--accent`, offset 2px

#### Scenario: Linked card/photo

- **WHEN** hover card clicável ou foto linkada
- **THEN** zoom 1.02-1.03, transition 200ms ease

#### Scenario: Card loops

- **WHEN** feature-cards visíveis
- **THEN** cycle 3s (opacity/fade + translateX), pause on hover, pause on `prefers-reduced-motion: reduce`

#### Scenario: No forbidden motion

- **WHEN** qualquer animação roda
- **THEN** zero `transform: rotate()` (tilt), zero `scroll-linked` (scroll-reveal, parallax), zero animation >5s sem pause button

### Requirement: Copy & Voice Quality

Todo texto SHALL passar `copy-lint.mjs`: 60-80 palavras/screen, headings=nouns, buttons=verb-object, 1 hard fact first screen, no slop words, assumed facts `data-assumed`.

#### Scenario: Copy lint passa

- **WHEN** `node scripts/copy-lint.mjs index.html --kind software` roda
- **THEN** zero FAIL, zero WARN não justificados
- **AND** headings: "Bibliotecas", "Séries", "Marcadores", "Logs", "Configurações" (nouns)
- **AND** buttons: "Iniciar Escaneamento", "Forçar Sincronização", "Salvar Configurações", "Ver Séries" (verb-object)

### Requirement: Validation Gates (Build + Lint + Playwright)

O pipeline SHALL passar todos os gates antes de merge.

#### Scenario: Build + Lint

- **WHEN** `npm ci && npm run build` em `vue/plex-frontend`
- **THEN** exit 0, dist/ gerado
- **WHEN** `npm run lint` em `vue/plex-frontend`
- **THEN** exit 0, zero errors, zero warnings

#### Scenario: Consistency checks

- **WHEN** `node scripts/consistency.mjs --widths 1440,1024,820,768,390 --rhythm-widths 1440,820,768,390 --page software --category "Plex Autoplay" --primary "Iniciar Escaneamento"` roda
- **THEN** PASS: rhythm (equal edges, section height), heading scale, font load, text contrast (WCAG AA), no visual empty, no caption, no placeholder

#### Scenario: Playwright visual validation (check.mjs)

- **WHEN** `node scripts/check.mjs --url http://localhost:5173 --sizes 1440x900,1024x768,820x1180,768x1024,390x844 --profile .taste-skill/chrome-check` roda
- **THEN** PASS: visual draw (all sizes), tap-size ≥44px, no overflow 320px, no collide, hero fold action visible, one section per screen, motion layer compliance, reduced-motion rest

#### Scenario: Performance

- **WHEN** `node scripts/perf.mjs index.html --profile .taste-skill/chrome-check` roda
- **THEN** PASS: image weight < budget, WebGL contexts ≤4, layout shift < 0.1

#### Scenario: QA UI Obrigatória (Playwright real browser)

- **WHEN** QA roda contra `http://localhost:<port>`
- **THEN** valida: (a) tema light/dark toggle funciona e persiste, (b) menus: user menu abre/navega/fecha, language selector abre/seleciona/fecha, estado ativo correto, (c) navegação entre views mantém header ativo, breadcrumb correto
- **AND** screenshots/evidências salvas em `openspec/changes/frontend-refactor-taste/qa-evidence-*.png`