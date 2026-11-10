# Proposal: Fix Biblioteca Título & Breadcrumb Alignment

## Problema

1. **Título incorreto**: A view de séries exibe "Séries → Anime" mas deve ser "Biblioteca → Anime" (o título da biblioteca).
2. **Terminologia inconsistente**: "Séries" no sidebar, router meta.title e breadcrumb fallback deve ser "Biblioteca" pois o app trabalha com bibliotecas, não séries isoladas.
3. **Breadcrumb desalinhado**: O breadcrumb no AppHeader tem padding/alinhamento visual que não corresponde ao conteúdo principal (conforme imagem do usuário).

## Escopo

### Mudanças de texto/terminologia
- `SeriesView.vue`: Título de "Séries {{ libraryTitle ? `→ ${libraryTitle}` : '' }}" para "Biblioteca {{ libraryTitle ? `→ ${libraryTitle}` : '' }}"
- `SeriesView.vue`: Texto "Séries da biblioteca selecionada" para "Biblioteca selecionada"
- `SidebarArea.vue`: Menu item "Séries" → "Biblioteca"
- `router/index.js`: `meta: { title: "Séries" }` → `meta: { title: "Biblioteca" }`
- `breadcrumb.js`: Fallback "Série" → "Biblioteca"
- `breadcrumb.test.mjs`: Atualizar testes para esperar "Biblioteca"

### Correção visual do breadcrumb
- `AppHeader.vue`: Ajustar padding/alinhamento do breadcrumb para alinhar com o conteúdo principal (padding horizontal consistente com `main > div > div` em DefaultLayout.vue)

## Critérios de aceite
- [ ] Título da view mostra "Biblioteca → <Nome>" em vez de "Séries → <Nome>"
- [ ] Sidebar mostra "Biblioteca" em vez de "Séries"
- [ ] Browser tab title mostra "Biblioteca" em vez de "Séries" (router meta.title)
- [ ] Breadcrumb fallback em series-markers mostra "Biblioteca" em vez de "Série"
- [ ] Breadcrumb visualmente alinhado com o conteúdo principal (padding esquerdo consistente)
- [ ] Tema light/dark toggle funciona
- [ ] Menus abrem/navegam/fecham corretamente
- [ ] Testes passam
- [ ] Build passa