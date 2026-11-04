# Tasks

## 1. Base de estilização

- [x] 1.1 Instalar `tailwindcss@^3.4`, `postcss`, `autoprefixer` e
      `@material-tailwind/html@2.3.2` no `vue/plex-frontend`
- [x] 1.2 Criar `tailwind.config.js` com `withMT`, `preflight: false`,
      `content` cobrindo `index.html` e `src/**/*.{vue,js}` e cores mapeadas
      para variáveis CSS
- [x] 1.3 Criar `postcss.config.js` com `tailwindcss` e `autoprefixer`
- [x] 1.4 Criar `src/styles/tailwind.css` com as diretivas `@tailwind` e o
      preset `@material-tailwind/html/styles/material-tailwind.css`
- [x] 1.5 Importar `styles/tailwind.css` em `main.js` antes de `tokens.css`

## 2. Tokens de marca

- [x] 2.1 Adicionar `--brand`, `--brand-strong`, `--brand-soft`, `--rail-bg`,
      `--rail-border` em `:root` (tema claro)
- [x] 2.2 Adicionar os mesmos tokens em `[data-theme="dark"]`
- [x] 2.3 Criar `tests/tokens.contrast.test.mjs` com `node --test` validando
      texto ≥4.5:1 e não-texto ≥3:1 nos dois temas
- [x] 2.4 Adicionar script `test:tokens` no `package.json`

## 3. Componente de navegação

- [x] 3.1 Criar `src/components/AppSidebar.vue` (props `items`, `activePath`,
      `statusLabel`; emits `select`, `logout`), sem importar `vue-router`,
      stores, composables ou `api.js`
- [x] 3.2 Estilo do rail: 88px, itens empilhados (ícone 24px + rótulo 11px),
      ativo em bloco lavanda arredondado, `aria-current="page"`
- [x] 3.3 Rodapé com `icon-button` do Material Tailwind e `aria-label="Sair"`
- [x] 3.4 Responsivo ≤768px: faixa horizontal rolável com alvos de 44px
- [x] 3.5 Foco visível (`outline` com `--brand`) e `prefers-reduced-motion`

## 4. Integração com o shell

- [x] 4.1 `useAppShell` expõe `activePath` e `navigate(to)`
- [x] 4.2 `App.vue` renderiza `AppSidebar` e trata `select`/`logout`
- [x] 4.3 Remover do `shell.css` as regras do layout antigo que ficaram órfãs

## 5. Verificação

- [x] 5.1 `node --test tests/` passa
- [x] 5.2 `npm run lint` passa (fronteiras de import preservadas)
- [x] 5.3 `npm run build` gera bundle sem erro
- [x] 5.4 `./build.sh` publica a solução e o container responde 200 em
      `/dashboard` e `/settings`
- [ ] 5.5 Conferir visualmente o rail no container (ícone + rótulo embaixo,
      item ativo lavanda, layout horizontal em ≤768px)
- Nota: 5.5 depende de login no app; verificado ate agora via bundle servido (classes do rail, aria-current, rotulos). Falta inspecao visual em navegador logado.
