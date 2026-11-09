# Proposal: Padronizar tamanhos dos botões do Dashboard

## Problem
O Dashboard (`DashboardView.vue`) possui botões com tamanhos inconsistentes:
- Botão "Ver Log Completo": `px-5 py-2.5 text-sm`
- Botão "Iniciar Escaneamento": `px-5 py-2.5 text-sm` (com ícone)
- Botões "Tentar novamente" (estados de erro): `px-4 py-2 font-medium`

Esta inconsistência visual quebra a harmonia da UI e não segue um padrão de design system.

## Solution
Padronizar todos os botões do Dashboard para usar o mesmo sistema de tamanhos:
- **Tamanho padrão (md)**: `px-4 py-2 text-sm` — usado para ações primárias/secundárias
- **Tamanho pequeno (sm)**: `px-3 py-1.5 text-xs` — usado para ações terciárias/em cards
- Remover variações ad-hoc (`px-5 py-2.5`, `px-4 py-2 font-medium`)
- Garantir que o dark mode, focus-visible, disabled e hover funcionem uniformemente

## Scope
- `vue/plex-frontend/src/views/Dashboard/DashboardView.vue` — corrigir classes dos 4 botões
- Opcional: criar/utilizar utilitários de botão consistentes em `style.css` ou componente `AppButton.vue` (se existir)

## Acceptance Criteria
1. Todos os botões visíveis no Dashboard têm altura visual idêntica quando são do mesmo tipo (primário/secundário)
2. Botões "Ver Log Completo" e "Iniciar Escaneamento" têm mesmo padding vertical/horizontal
3. Botões "Tentar novamente" nos estados de erro usam mesmo tamanho dos botões principais
4. Dark mode mantém contraste adequado
5. Estados focus-visible, hover, disabled funcionam corretamente
6. Build passa (`npm run build` em `vue/plex-frontend`)
7. Lint passa (`npm run lint` em `vue/plex-frontend`)

## Affected Specs
- `specs/ui-theme/spec.md` — tokens de espaçamento/tamanho de botão (se definidos)
- `specs/dashboard/spec.md` — quick actions e error states referenciam botões