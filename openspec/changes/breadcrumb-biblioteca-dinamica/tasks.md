# Tasks

## 1. Mudar última migalha do breadcrumb para nome dinâmico da biblioteca

- [ ] 1.1 Em `src/lib/breadcrumb.js`, rota `series-markers`, mudar `text(query.seriesTitle) || "Biblioteca"` para `text(query.seriesTitle) || lib || "Biblioteca"`.
- [ ] 1.2 Atualizar teste `qa-remove-lib-title.spec.js` / testes de breadcrumb se necessário (o teste sem metadados continua passando pois `lib` vazio cai em "Biblioteca").

## 2. Build, QA e validação

- [ ] 2.1 `npm run build` em `vue/plex-frontend` passa.
- [ ] 2.2 `node --test tests/breadcrumb.test.mjs` passa (12 testes).
- [ ] 2.3 `openspec validate breadcrumb-biblioteca-dinamica --strict` passa.
- [ ] 2.4 QA UI Playwright contra `http://localhost:8080`: tema light/dark toggle e menus (abertura/navegação/fechamento/estado ativo) continuam funcionando; breadcrumb da página de marcadores mostra nome dinâmico da biblioteca.
