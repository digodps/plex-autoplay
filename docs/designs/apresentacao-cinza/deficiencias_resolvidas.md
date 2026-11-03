# Registro de resolução (EGM — fase Goldfish Review → Resolve)

3 revisores paralelos (arquitetura/SOLID/Clean Code, tema/visual/a11y,
escopo/viabilidade/regressão) sobre `design.md` v1. Total: 9 + 21 + 12 achados.
Design v2 (`design.md`) já incorpora as correções. Abaixo o que foi feito com
cada achado.

## Arquitetura

| id | achado | resolução |
|---|---|---|
| D-01 | Diagrama de dependência era cadeia linear, contradizia o texto | §4 virou **matriz "quem importa quem"** normativa |
| D-02 | `stores/dashboard.js` importa `composables/useDashboardAggregators.js` (invertido) | arquivo vira `src/lib/aggregators.js`; store importa de lá |
| D-03 | 4 views importam `api.js` e fazem fetch | cada view ganha composable; aceite §10.7 cobre views **e** components |
| D-04 | `MarkerRow.vue` importa `useMarkerStore` | componente só props/emit; aceite §10.8 |
| D-05 | `useLibrarySeries`/`useSeriesCatalog` com estado próprio sem store | criar `stores/series.js`; composable só orquestra |
| D-06 | Fronteira só por revisão humana | `eslint-plugin-boundaries` obrigatório (§4.1) |
| D-07 | `useDashboardAggregators.js` é função pura em `composables/` | mover para `src/lib/` |
| D-08 | `main.js`/`router` importam `api.js` sem estar documentado | exceção documentada; helpers de token/cookie → `src/lib/auth.js` |
| D-09 | `StatusCard`/`Breadcrumb` usam `useRoute`/`useRouter` | derivar rota na view e passar por prop; §4 proíbe `vue-router` em componente |

## Tema / visual / acessibilidade

| id | achado | resolução |
|---|---|---|
| T-01 | `--text-tertiary` escuro `#737373` < 4.5:1 | escuro → `#8c8c8c` |
| T-02 | `--text-tertiary` claro `#737373` sobre `--surface-raised` < 4.5:1 | claro → `#616161` |
| T-03 | `--border`/`--border-strong` < 3:1 em inputs | novo `--border-input: #737373` (≈4.7:1 claro, ≈3.7:1 escuro) |
| T-04 | Aceite só cobria texto ≥4.5:1 | aceite §10.5 inclui não-texto ≥3:1 |
| T-05 | Nenhuma resolução de tema antes do primeiro paint | script inline bloqueante no `<head>` (§5.2) |
| T-06 | Código usa `.theme-dark`, nenhum CSS responde; `sync-view.css` com hex hardcoded | padronizar `[data-theme="dark"]`; remover bloco hardcoded |
| T-07 | `mode="system"` não resolvia | `@media (prefers-color-scheme: dark) :root:not([data-theme])` + `matchMedia` no JS |
| T-08 | `accent` único vs. token por tema (contradição) | §5.4: accent vira token de sistema; preferência do usuário = só `mode` |
| T-09 | `--accent == --text` (item ativo indistinguível) | accent vira cinza médio (`#404040`/`#d4d4d4`) + peso/subfundo |
| T-10 | Controles de tema só no Dashboard | toggle no shell (`App.vue`), visível em toda rota |
| T-11 | `applyTheme` vivia no `DashboardView` | aplicação global (shell/`main.js`) |
| T-12 | Paleta amarela/colorida contradiz "cinza" | `accentPalette` removida; `#e5a00d` e `#a855f7` removidos |
| T-13 | Sem `prefers-reduced-motion` | adicionado em `base.css` |
| T-14 | Alvo 44px prometido, controles menores | token `--target-min: 44px`; alvo via padding |
| T-15 | `index.html lang="en"` com UI pt-BR | `lang="pt-BR"` |
| T-16 | Emoji de ícone sem `aria-hidden` | `<span aria-hidden="true">` + `aria-label` |
| T-17 | Exceção danger/success sem pares documentados | pares documentados + `--warning` |
| T-18 | Anel de foco usava accent salvo (<3:1) | `--focus-ring` fixo por tema |
| T-19 | `--text`/`--text-secondary` — OK | mantido |
| T-20 | radius/sombra/transição — OK | mantido; remover sombras extras |
| T-21 | ARIA base do Dashboard — OK | mantido |

## Escopo / viabilidade / regressão

| id | achado | resolução |
|---|---|---|
| E-01/E-07 | `theme.js` nunca seta `data-theme`; `system` não tratado | §5.2/§5.3 + tarefa explícita em `theme.js` |
| E-02 | `redesign.css` roxo, default amarelo | §5.4 remove ambos |
| E-03 | 38 hex literais em CSS | migrar para tokens (tarefa de migração) |
| E-04 | `app-shell.css` provê layout real | absorver para `shell.css` antes de deletar |
| E-05 | Dezenas de `style=` em `App.vue` | §7 remove 100% |
| **E-06** | "sintaxe inválida em `router/index.js:45`, build falha" | **falso positivo** — verificado: linha correta (`component: () => import(...)`); baseline `npm run build` passa (459ms) |
| E-08 | `tokens.css` não existe | §5/§6 criam |
| E-09 | `StatusCard/Skeleton/Error` já existem | §8 muda para **reusar**, não criar `State*` |
| E-10 | "só auth.spec.js" | **impreciso** — existem `auth`, `container`, `images`, `libraries`, `markers` specs; aceite mantém "e2e existentes passam" |
| E-11 | Config eslint ausente | **parcial** — `eslint.config.js` existe na raiz do repo; falta plugin boundaries + script `lint` (§4.1) |
| E-12 | 3 arquivos CSS redundantes | §9 deleta após absorção |

## Decisões abertas fechadas

1. `accent` deixa de ser preferência do usuário (token de sistema). (§5.4)
2. `State*` → reusar `StatusCard*`. (§8)
3. Enforcement de fronteira via `eslint-plugin-boundaries`, não revisão humana.
   (§4.1)
4. Rollback: tag git `pre-apresentacao-cinza` no commit `dc8fa0d`.

## Achados verificados como falsos (não agir)

- E-06 (router syntax) — build passa no baseline.
- E-10 parcialmente — cobertura e2e maior que o relatado.
