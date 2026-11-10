# Design: Fix Biblioteca Título & Breadcrumb Alignment

## Contexto
O app exibe "Séries" em múltiplos lugares, mas o conceito correto é "Biblioteca" (coleção de mídia do Plex). O usuário reportou:
1. Título incorreto: "Séries → Anime" → deve ser "Biblioteca → Anime"
2. Terminologia inconsistente em sidebar, router, breadcrumb
3. Breadcrumb visual desalinhado (padding/alinhamento)

## Mudanças de Texto/Terminologia

### 1. SeriesView.vue (vue/plex-frontend/src/views/Series/SeriesView.vue)
- Linha 48: `Séries {{ libraryTitle ? `→ ${libraryTitle}` : '' }}` → `Biblioteca {{ libraryTitle ? `→ ${libraryTitle}` : '' }}`
- Linha 51: `Séries da biblioteca selecionada` → `Biblioteca selecionada`
- Linha 113: `{{ seriesTotalSize }} séries` → `{{ seriesTotalSize }} itens` (ou manter "séries" como contagem? Usuário disse "tudo que é Séries para Biblioteca", mas aqui é contagem de itens. Vamos manter "itens" para neutralidade)

### 2. SidebarArea.vue (vue/plex-frontend/src/components/Sidebar/SidebarArea.vue)
- Linha 11: `{ name: "Séries", to: "libraries", icon: "tv" }` → `{ name: "Biblioteca", to: "libraries", icon: "tv" }`

### 3. Router (vue/plex-frontend/src/router/index.js)
- Linha 35: `meta: { title: "Séries" }` → `meta: { title: "Biblioteca" }`

### 4. Breadcrumb lib (vue/plex-frontend/src/lib/breadcrumb.js)
- Linha 36: `items.push({ label: text(query.seriesTitle) || "Série" });` → `items.push({ label: text(query.seriesTitle) || "Biblioteca" });`

### 5. Testes (vue/plex-frontend/tests/breadcrumb.test.mjs)
- Linha 84: `{ label: "Série", to: undefined, isCurrent: true }` → `{ label: "Biblioteca", to: undefined, isCurrent: true }`
- Linha 96: `{ label: "Série", to: undefined, isCurrent: true }` → `{ label: "Biblioteca", to: undefined, isCurrent: true }`

## Correção Visual do Breadcrumb

### Análise do problema
- `AppHeader.vue` linha 41: `<header class="relative z-30 flex shrink-0 items-center gap-2 px-4 py-2 text-sm">`
- `AppHeader.vue` linha 57: `<ol class="flex w-max items-center gap-1 whitespace-nowrap text-gray-700 dark:text-bodydark">`
- O breadcrumb fica em `nav` com `min-w-0 flex-1 overflow-x-auto` (linha 57)
- O conteúdo principal em `DefaultLayout.vue` linha 45: `<div class="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">`

### Causa raiz
O `header` tem `px-4` (16px padding horizontal). O conteúdo principal tem `mx-auto max-w-screen-2xl p-4 md:p-6` que centra com max-width e padding variável. Em mobile o alinhamento é OK (p-4 = 16px), mas em md+ o conteúdo tem mais padding (24px) que o header (16px), causando desalinhamento visual.

### Solução
Opção A: Aumentar padding do header para `px-4 md:px-6` (alinhar com conteúdo)
Opção B: Usar container compartilhado (refatoração maior - YAGNI)

Vou usar Opção A: `px-4 md:px-6` no header para alinhar com conteúdo em md+.

### Arquivo: AppHeader.vue
- Linha 41: `class="relative z-30 flex shrink-0 items-center gap-2 px-4 py-2 text-sm"` → `class="relative z-30 flex shrink-0 items-center gap-2 px-4 md:px-6 py-2 text-sm"`

Isso faz o header ter 16px em mobile, 24px em md+, igualando o padding do conteúdo principal.

## Contratos
- Nenhuma API alterada
- Nenhum tipo TypeScript alterado (projeto usa JS)
- Testes de breadcrumb atualizados

## Riscos
- Baixo: apenas textos e padding CSS
- Verificar que "Biblioteca" no breadcrumb fallback não confunda com navegação para /libraries (o item "Bibliotecas" já existe como primeiro nível)