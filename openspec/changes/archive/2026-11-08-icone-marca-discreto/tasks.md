# Tasks

## 1. Implementar marca discreta

- [x] 1.1 Reescrever `src/components/AppLogo.vue` removendo `rect` de fundo gradiente, borda e preenchimento sólido; usar `fill="none"`, `stroke="currentColor"`, `stroke-width="3"`. Verificar que não resta `rect` de fundo e que `stroke` existe.
- [x] 1.2 Atualizar `public/favicon.svg` com mesma marca em traço e cor neutra `#8B949E` sobre fundo transparente. Verificar sem `rect` de fundo.
- [x] 1.3 Rodar `npm run build` em `vue/plex-frontend` e verificar saída sem erros. Artefato de build em `dist`.

## 2. Verificação visual/funcional

- [x] 2.1 Iniciar app localmente (`npm run dev` ou `preview`) e abrir `http://localhost:<port>`.
- [x] 2.2 Confirmar o logo do cabeçalho renderiza como traço fino sem fundo sólido (fundo transparente) tanto no tema claro quanto escuro.
- [x] 2.3 Confirmar navegação do menu do usuário (abrir/fechar/ir para Ajustes) ainda funciona.
- [x] 2.4 Confirmar o toggle de tema (claro/escuro) continua alternando e aplicando `data-theme`.

## Workflow follow-up

- Commit dos artefatos da change + worktree `feature/spec-icone-marca-discreto` antes do apply.
- Após QA PASS, avaliar archive da change.
