# Design — Sidebar estilo Canva com Material Tailwind

**Status:** aprovado (Fase 1)
**Capacidade nova:** `navegacao-lateral`
**Capacidade modificada:** `apresentacao`
**Change OpenSpec:** `canva-sidebar-material-tailwind`

## 1. Problema

A camada de apresentação do plex-autoplay usa CSS escrito à mão com paleta
grayscale (`src/styles/tokens.css`) e uma sidebar de 240px com itens
horizontais (ícone + rótulo na mesma linha). Não há biblioteca de componentes:
cada botão/cartão é CSS próprio. O resultado funciona, mas diverge da
referência visual desejada (rail estreito estilo Canva: ícone grande, rótulo
pequeno embaixo, destaque lavanda no item ativo) e não escala para novos
componentes.

## 2. Decisão

Adotar **Tailwind CSS 3 + `@material-tailwind/html@2.3.2`** e reescrever
**apenas** a navegação lateral no estilo Canva.

- `@material-tailwind/html` v2 é a única variante do Material Tailwind
  disponível no registry que serve Vue: entrega `utils/withMT.js` (helper de
  config que funde o tema Material Design no Tailwind). `@material-tailwind/vue`
  **não existe** no npm (404) e a v3 do pacote html entrega só comportamentos
  JS, sem CSS.
- Tailwind 3 (não 4) porque o helper e o tema do pacote seguem a API de
  configuração do Tailwind 3 (`tailwind.config.cjs` + `@tailwind` directives).
- `preflight: false` — o reset do Tailwind quebraria as telas existentes
  (inputs, tabelas, headings) que não serão migradas nesta entrega.

Escopo restrito de propósito: views existentes (`DashboardView`,
`LibrariesView`, `SyncView`, `SettingsView`, `MarkerEditorView`) e seus CSS
continuam como estão. Migrar tudo agora seria mudança grande sem ganho
imediato.

## 3. Tokens (fonte única: `src/styles/tokens.css`)

Novos tokens de marca, com par claro/escuro, mantendo a regra "nenhum literal
de cor fora do arquivo de tokens":

| Token | Claro | Escuro | Uso |
|---|---|---|---|
| `--brand` | `#7d2ae8` | `#a578f0` | ícone ativo, acento |
| `--brand-strong` | `#6a1fb8` | `#c9b4f7` | rótulo de navegação |
| `--brand-soft` | `#e9e2fb` | `#2a2145` | fundo do item ativo |
| `--rail-bg` | `#f2f0fa` | `#1b1730` | fundo do rail |
| `--rail-border` | `#e2dcf5` | `#2c2547` | divisória do rail |

Contraste verificado (WCAG 1.4.3/1.4.11):

- `--brand-strong` sobre `--rail-bg` claro: **7.45:1**
- `--brand` sobre `--rail-bg` claro: **5.53:1**
- `--brand-strong` escuro (`#c9b4f7`) sobre `--rail-bg` escuro: **9.37:1**
- `#ffffff` sobre `--brand`: **6.23:1**

## 4. Componentes

**`components/AppSidebar.vue`** (novo, apresentacional puro)

- Props: `items` (`[{ to, label, icon }]`), `activePath` (string), `statusLabel`.
- Emits: `select(to)`, `logout()`.
- Sem `vue-router`, sem store, sem `api.js` — respeita
  `boundaries/dependencies` (componente só importa `lib`).
- Cada item é `<button type="button">` com `aria-current="page"` quando ativo
  e `aria-label` quando o rótulo é visualmente truncado.
- Rodapé: botão de saída com utilitários Tailwind, ícone `AppIcon name="logout"`
  e `aria-label="Sair"`; ao lado, um indicador de status com `role="img"` e
  `aria-label` (o rail de 88px não comporta texto).

**`composables/useAppShell.js`** (alterado)

- Ganha `activePath` (`route.path`) e `navigate(to)` para o shell traduzir o
  evento do componente em navegação — a navegação continua fora da view.

**`App.vue`** (alterado)

- Troca os `router-link` por `<AppSidebar>`; `<main>` preservado.

## 5. Layout do rail

- Desktop (>768px): largura fixa **88px**, `flex-direction: column`, itens
  centrados: ícone 24px, rótulo 11px (`--text-2xs`) abaixo, `gap: 4px`.
- Item ativo: bloco arredondado (`border-radius: 16px`) com
  `background: var(--brand-soft)` e ícone/rótulo em `--brand-strong`.
- Hover: `--brand-soft` com 60% de opacidade; `prefers-reduced-motion` já
  respeitado pelos tokens de transição existentes.
- Mobile (≤768px): rail vira faixa horizontal rolável no topo, cada item com
  `min-height: var(--target-min)` (44px), mantendo alvos de toque.
- Foco visível: `outline: 2px solid var(--brand)` + `outline-offset: 2px`
  (≥3:1 contra o rail).

## 6. Material Tailwind — uso concreto

`@material-tailwind/html` v2 não entrega presets de classe prontos (a v3 beta
entrega só comportamentos JS). O que ele entrega e o projeto usa:

- `utils/withMT.js`: helper que funde o tema do Material Design (paleta
  `blue-gray`/`gray`, tipografia Roboto, sombras) na config do Tailwind.
  `tailwind.config.cjs` é o resultado desse merge, com `preflight: false` e
  `theme.extend.colors` mapeando os tokens do projeto (`bg-rail`,
  `text-brand-strong`, ...).
- Utilitários Tailwind no componente `AppSidebar.vue`, incluindo variantes
  responsivas (`max-md:*`) — nenhum CSS escrito à mão no rail.

`styles/material-tailwind.css` do pacote **não** é importado: são utilitários
usados apenas pelos comportamentos JS do pacote (tooltip, collapse), que o
projeto não usa, e o arquivo traz um reset global que brigaria com `base.css`.

## 7. Verificação

1. `node --test tests/` — teste de contraste dos tokens (parse de
   `tokens.css`, razões ≥4.5 texto / ≥3 não-texto). Sem dependências novas.
2. `npm run lint` — fronteiras de import e regras Vue continuam passando.
3. `npm run build` — bundle gerado; `build.sh` publica em `wwwroot` e sobe o
   container.
4. Checagem manual em `http://192.168.18.5:8080/dashboard`: rail 88px,
   rótulos embaixo, item ativo lavanda, foco visível ao tabular, layout
   horizontal em viewport ≤768px.

## 8. Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Preflight do Tailwind quebrar telas antigas | `preflight: false` |
| Conflito de especificidade entre CSS antigo e utilitários | utilitários só no rail; CSS antigo carregado depois do Tailwind |
| Material Tailwind v2 descontinuado | API é só CSS/config; substituível sem tocar nos componentes |
| Contraste do roxo em fundo claro | tokens medidos; teste automatizado no CI |
