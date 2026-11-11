# Tasks

## 1. Remover título duplicado da página de séries

- [ ] 1.1 Remover o bloco `<div class="border-b ...">` contendo `<h1>Biblioteca → ...</h1>` e `<p>Biblioteca selecionada.</p>` de `src/views/Series/SeriesView.vue`.
- [ ] 1.2 Atualizar testes de breadcrumb (se necessário) para refletir que a página de séries não duplica a biblioteca.

## 2. Build, QA e validação

- [ ] 2.1 `npm run lint` em `vue/plex-frontend` passa.
- [ ] 2.2 `npm run test:breadcrumb` (ou `node --test`) passa.
- [ ] 2.3 `openspec validate remove-lib-title --strict` passa.
- [ ] 2.4 Build do frontend `npm run build` passa.
- [ ] 2.5 QA UI Playwright contra `http://localhost:8080`: tema light/dark toggle e menus (abertura/navegação/fechamento/estado ativo) continuam funcionando; página de séries não exibe "Biblioteca →" nem "Biblioteca selecionada."
