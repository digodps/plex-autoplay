# Proposal

## Why

A navegação lateral do plex-autoplay é uma lista vertical de 240px com ícone e
rótulo na mesma linha, estilizada com CSS escrito à mão sobre uma paleta
grayscale. Ela não comunica hierarquia (nada distingue visualmente o destino
atual além de uma borda de 2px) e o projeto não tem biblioteca de componentes,
então cada novo elemento de UI vira CSS novo.

A referência visual desejada é o rail estreito do Canva: ícone grande com
rótulo pequeno embaixo, item ativo destacado por um bloco lavanda arredondado,
acento roxo. Além do visual, adotar Tailwind CSS + Material Tailwind dá uma
base de utilitários e presets de componente para as próximas telas.

## What Changes

- **Nova dependência de UI**: Tailwind CSS 3, PostCSS, Autoprefixer e
  `@material-tailwind/html@2.3.2` (única variante compatível com Vue: entrega o
  helper `withMT` que funde o tema Material Design no Tailwind).
  `@material-tailwind/vue` não existe no registry (404).
- **Novos tokens de marca** em `src/styles/tokens.css` (`--brand`,
  `--brand-strong`, `--brand-soft`, `--rail-bg`, `--rail-border`) para os temas
  claro e escuro, todos com contraste verificado.
- **Novo componente `AppSidebar.vue`**: rail de 88px, ícone 24px, rótulo 11px
  abaixo do ícone, item ativo em bloco lavanda arredondado, rodapé com botão de
  saída usando a classe `icon-button` do Material Tailwind.
- **Shell atualizado**: `useAppShell` expõe `activePath` e `navigate(to)`;
  `App.vue` renderiza `AppSidebar` no lugar dos `router-link` inline.
- **Responsivo**: em viewport ≤768px o rail vira faixa horizontal rolável no
  topo, preservando alvos de toque de 44px.
- **Teste de contraste** dos tokens (`node --test`, sem dependências novas).

Fora de escopo: migrar as views existentes (`DashboardView`,
`LibrariesView`, `SyncView`, `SettingsView`, `MarkerEditorView`) para
utilitários Tailwind. Elas continuam com o CSS atual.

## Capabilities

### New Capabilities

- `navegacao-lateral`: rail de navegação principal no estilo Canva — itens com
  ícone e rótulo empilhados, estado ativo destacado, comportamento responsivo,
  alvos de toque e foco visível. É dona do componente `AppSidebar` e do
  contrato de eventos com o shell.

### Modified Capabilities

- `apresentacao`: a fonte única de tokens passa a incluir a paleta de marca
  (roxo/lavanda) além da escala grayscale, e a camada de apresentação passa a
  usar Tailwind CSS + Material Tailwind como sistema de estilização. As regras
  de contraste, ausência de literal de cor fora dos tokens, ausência de estilo
  inline e fronteiras de import permanecem válidas.

## Impact

- `vue/plex-frontend/package.json` / `package-lock.json`: 4 dependências novas.
- `vue/plex-frontend/tailwind.config.js`, `postcss.config.js` (novos).
- `src/styles/tokens.css` (tokens de marca), `src/styles/shell.css` (rail),
  `src/styles/tailwind.css` (novo, diretivas + preset do Material Tailwind).
- `src/App.vue`, `src/composables/useAppShell.js`, `src/components/AppSidebar.vue`.
- `src/main.js`: passa a importar o CSS do Tailwind.
- Sem mudança de backend, API ou contrato de dados.
