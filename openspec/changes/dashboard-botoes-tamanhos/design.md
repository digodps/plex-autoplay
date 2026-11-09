# Design: Padronizar tamanhos dos botões do Dashboard

## Approach
Definir classes de componente compartilhadas (`.btn`, `.btn-md`) em `vue/plex-frontend/src/styles/style.css` dentro de um bloco `@layer components`, e substituir as classes ad-hoc do Tailwind (`px-5 py-2.5`, `px-4 py-2`, `rounded-xl`, `text-sm` inline, `focus-visible` solto) nos 4 botões do `DashboardView.vue`.

## Why not tokens.css / AppButton
- `tokens.css` (do redesign-frontend-neutral) ainda não existe na branch main; criá-lo agora escapa do escopo e requer importar em `main.js` — adiar para quando uma segunda view precisar.
- Não existe componente `AppButton`; criá-lo para uma única tela com 4 botões é YAGNI. Classes utilitárias compartilhadas em `style.css` resolvem o problema com o mínimo de código.

## Files to Modify
1. `vue/plex-frontend/src/styles/style.css` — adicionar `.btn` (base: tamanho md, radius, tipografia, focus-visible ring, disabled opacity) + `.btn-md` (padding 1rem x 0.5rem) + variantes `.btn-primary`, `.btn-secondary`, `.btn-danger`, `.btn-warning`.
2. `vue/plex-frontend/src/views/Dashboard/DashboardView.vue` — substituir classes dos 4 botões pelas classes compartilhadas:
   - "Ver Log Completo": `btn btn-secondary`
   - "Iniciar Escaneamento": `btn btn-md btn-primary`
   - "Tentar novamente" (total failure): `btn btn-md btn-danger`
   - "Tentar novamente" (library error): `btn btn-md btn-warning`
   - Adicionar `:disabled="store.isLoading"` aos 2 botões de retry (paridade com os primários).
   - Adicionar `focus-visible` ring + `disabled` aos botões de retry (paridade).
   - Adicionar `text-sm` aos botões de retry.

ponytail: `.btn-sm` (px-3 py-1.5 text-xs) omitido por enquanto; adicionar quando uma ação menor aparecer. Tokens `--btn-*` omitidos por enquanto; classes fixas são suficientes para uma tela.

## UI/UX Considerations
- Um único tamanho (md) para todos os botões do Dashboard -> mesma altura visual.
- Dark mode, focus-visible rings, hover e disabled funcionam uniformemente.
- Nenhuma mudança de comportamento funcional nem de rotas.

## Migration
Nenhum dado a migrar. Mudança puramente visual.