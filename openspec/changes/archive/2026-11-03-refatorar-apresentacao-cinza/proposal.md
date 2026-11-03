# Proposal

## Why

A infraestrutura de tema (`tema-paleta`, já implementada) entregou store Pinia e
endpoints `GET/PUT /api/theme`, mas a camada de apresentação continua sem
desenho: `App.vue` com estilo inline em todo elemento, CSS fragmentado em
vários arquivos com 38 hex literais espalhados, cor de destaque amarela/roxa
incompatível com o pedido de tons de cinza, aplicação de tema confinada ao
`DashboardView` (login/settings ficam sem tema) e dark mode sem efeito real no
shell. O resultado não atende à exigência: tema claro/escuro em escala de cinza,
minimalista, moderno e funcional.

## What Changes

- **Tokens em escala de cinza** (`src/styles/tokens.css`): fonte única de
  valores visuais para claro e escuro (`:root` + `[data-theme="dark"]` +
  fallback `prefers-color-scheme`), com contraste de texto ≥ 4.5:1 e de
  não-texto (bordas/ícones/controles/foco) ≥ 3:1.
- **Shell sem estilo inline**: `App.vue` migra para classes (`shell.css`),
  toggle de modo claro/escuro/sistema passa a viver no shell e a aplicação do
  tema passa a ser global e **antes do primeiro paint** (script inline
  bloqueante no `<head>`), eliminando FOUC.
- **Arquitetura de camadas**: `views → composables/stores → api.js`, com
  `lib/` para funções puras e helpers de auth; enforcement por
  `eslint-plugin-boundaries`.
- **Preferência de tema simplificada**: `accent` deixa de ser escolha do
  usuário e vira token de sistema (cinza); `mode` é a única preferência
  persistida. Paleta colorida e defaults amarelo/roxo removidos.
- **Estados padronizados**: reuso de `StatusCard`/`StatusCardSkeleton`/
  `StatusCardError` em todas as views.
- **Acessibilidade**: `lang="pt-BR"`, foco visível fixo, alvos de 44px,
  `prefers-reduced-motion`.

## Impact

- **Affected specs**: `apresentacao` (nova), `ui-theme` (semântica de `accent`
  alterada — ver nota).
- **Affected code**: `vue/plex-frontend/src` (styles, App.vue, views,
  components, stores, composables, lib, main.js, router, index.html),
  `eslint.config.js` (raiz), `vue/plex-frontend/package.json` (script `lint`),
  backend default de `accent` (`#e5a00d` → `#171717`).
- **Contrato HTTP**: preservado (`GET/PUT /api/theme`); campo `accent` mantido
  por compatibilidade, ignorado na renderização.
- **Backward compatibility**: modo claro continua default; nenhuma mudança de
  rota ou de fluxo de negócio.

> Nota `ui-theme`: a requirement "User can select accent color" deixa de valer —
> a seleção de cor é removida. A store e a API mantêm o campo por
> compatibilidade, mas `accent` vira token de sistema. Registrado aqui por não
> haver delta MODIFIED no escopo desta mudança de apresentação.
