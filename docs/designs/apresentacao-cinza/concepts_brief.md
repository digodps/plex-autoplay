# Concepts Brief — Refatoração da camada de apresentação (cinza, claro/escuro, minimalista)

## Problema

O painel Vue (`vue/plex-frontend`) funciona, mas a camada de apresentação está
acumulada, não desenhada:

- `App.vue` carrega o shell inteiro e praticamente todo o estilo está em
  atributos `style="..."` inline por elemento (sidebar, brand, nav, footer,
  botão Sair, main). Não há folha de estilo do shell governando esses elementos.
- O CSS está espalhado em 6 arquivos sem fonte única de verdade:
  `style.css` (2 linhas), `app-shell.css`, `redesign.css`, e CSS por view
  (`marker-editor.css`, `sync-view.css`, `SeriesView.css`,
  `components/marker-row.css`). Tokens de cor aparecem duplicados.
- A cor de destaque padrão é amarela (`#e5a00d`, herdada do Plex). A exigência
  atual é tons de cinza — o amarelo contradiz o pedido.
- Sobras do scaffold do Vite: `HelloWorld.vue`, `App.vue.bak`,
  `assets/vue.svg`, `assets/vite.svg`.
- Estados de carregando/vazio/erro existem de forma desigual entre as telas.

## Hipótese de solução

Uma refatoração de apresentação em três movimentos:

1. **Camada única de tokens em escala de cinza**, claro e escuro, em
   `src/styles/tokens.css` (valores) + `base.css` (reset/tipografia), com o
   tema aplicado no elemento raiz. Componentes e views consomem apenas tokens —
   nenhum valor de cor literal espalhado.
2. **Shell reescrito sem estilo inline**: `App.vue` passa a usar classes, com o
   estilo do shell em `styles/shell.css`. Mesma estrutura (sidebar + main),
   agora legível e alterável em um lugar só.
3. **Minimalismo funcional**: hierarquia por contraste e espaço em vez de
   sombras/bordas fortes; radius pequeno; transições curtas; foco visível
   (contraste AA); sidebar colapsando no mobile; estados vazio/carregando/erro
   padronizados por componente.

O toggle claro/escuro e a persistência já existem (change `tema-paleta`,
implementado: store Pinia `theme`, `GET/PUT /api/theme`, tabela
`UserThemePreferences`). Esta mudança **preserva esse contrato** e só muda o
valor padrão e a paleta ofertada, que passam a ser cinza.

## Limites (fora de escopo)

- Não mexer no backend .NET, exceto se algum endpoint de tema precisar de ajuste.
- Não trocar o framework, nem adotar biblioteca de UI (Tailwind, Vuetify etc.).
- Não redesenhar fluxos nem telas de negócio; só apresentação.
- Não alterar contratos das stores/API.

## Por que agora

O change `tema-paleta` terminou de criar a infraestrutura de tema. Sem a
refatoração da apresentação, cada tela continua com estilo próprio e o tema
"cinza" não é aplicável de forma consistente.
