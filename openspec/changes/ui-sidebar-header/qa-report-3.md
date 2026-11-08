PASS

# QA independente — ui-sidebar-header (iteração 2)

- URL: http://localhost:8090
- Data: 2026-11-07
- Método: Playwright 1.64.1, Chromium, storageState `/tmp/opencode/qa-sidebar/auth.json`, navegador real contra a imagem Docker recompilada.
- Gates do orquestrador não reexecutados (`openspec validate`, build, lint, testes), conforme instrução.
- Nenhuma credencial ou token registrado.

## Checagens

| # | Checagem | Resultado | Evidência medida |
|---|---|---|---|
| 1 | Sidebar abaixo do header, sem sobreposição | PASS | 1280x800: header bottom `49px`, aside top `49px`, gap `0px`, overlap `false`; aside `256x751`. Também gap `0px` em 768 e 1024. |
| 2 | Sidebar lista `Animes`; sem `Menu`, `Light`/tema, `Bibliotecas` | PASS | 1 link, texto `Animes`, `href="/libraries/1/series"`, `aria-label="Animes"`; 0 `<button>`; `hasMenuText=false`, `hasLightText=false`, `hasBibliotecasText=false`. |
| 3 | Clique navega e marca ativo | PASS | URL `http://localhost:8090/libraries/1/series`; exatamente 1 `aria-current="page"`; bg `rgb(241,243,245)`, cor `rgb(9,105,218)`. |
| 4 | Gap vertical >= 4px | PASS | `<ul>` flex, `row-gap: 4px`, padding `8px`, item `44px`. API real tem só 1 biblioteca; com 2 bibliotecas (mock read-only da resposta) gap medido entre `<li>` = `4px`. |
| 5 | Toggle de tema alterna `dark` e persiste | PASS | `<html>`: `dark` -> sem classe, `localStorage["theme.mode"]` = `light`; reload mantém light. Fundos: light body/header `#FFFFFF`, aside `#F8F9FA`; dark body/header/aside `#18181B`. Ver achado 2 (contraste de texto no light). |
| 6 | Hamburger 256->64, aria, labels, persistência | PASS | Antes: `aria-expanded="true"`, label `Recolher menu`, aside `256px`. Depois: `aria-expanded="false"`, label `Expandir menu`, aside `64px`, span do rótulo `display:none`, link sem texto visível, `aria-label="Animes"`, `title="Animes"`, `localStorage["sidebar.collapsed.v2"]="true"`. Reload: `64px`, `aria-expanded="false"`. |
| 7 | 768px compacta; 1024px expandida | PASS | 768: aside `64px`, rótulo `display:none`, `aria-label="Animes"`. 1024: aside `256px`, rótulo `display:block`, link `239px`. Fronteira: 767=`64px`, 768=`64px`, 769=`256px`. Ver achado 1 (aria em <=768). |
| 8 | Breadcrumb no header e trilha única | PASS | `/libraries/1/series`: `nav[aria-label="Breadcrumb"]` count = `1`; texto `Início / Animes`; `Início` -> `href="/dashboard"`; `Animes` com `aria-current="page"`. |
| 9 | Menus de perfil e ajustes | PASS | Perfil: abre com `Sair` (1) e link `/settings` (1); fecha ao acionar o gatilho (0). Ajustes: abre (`#settings-title` = `Configurações`), fecha por backdrop (`true`) e por `Escape` (`true`). |
| 10 | Console sem erros relevantes | PASS | `/dashboard` e `/libraries/1/series`: 0 `console.error`, 0 `pageerror`. |

## Achados

1. **MÉDIA — `aria-expanded` contradiz o modo compacto em <=768px.**
   Em 767/768px a sidebar renderiza `64px` com rótulo oculto, mas o hamburger permanece `aria-expanded="true"` e `aria-label="Recolher menu"`; clicar alterna o estado ARIA sem mudança visual. Leitores de tela anunciam estado diferente do renderizado.
   Correção esperada: `vue/plex-frontend/src/components/AppHeader.vue` (derivar estado efetivo do breakpoint, ex. `matchMedia("(max-width: 768px)")`) e, se necessário, `vue/plex-frontend/src/stores/sidebar.js`.
2. **MÉDIA (fora do escopo da sidebar, exposto pelo novo toggle) — `text-body` ilegível no tema light.**
   Token `text-body` computa `rgb(201,209,217)` (#C9D1D9) sobre fundo branco: contraste `1.57:1`, abaixo de 4.5:1. Ocorre no conteúdo do dashboard em elementos com `text-body dark:text-bodydark` (ex.: "Serviço", "CPU", "RAM", "Última Sincronização", "Bibliotecas"). Já registrado como dívida P1 em `deficiencias_resolvidas.md`.
   Correção esperada: `vue/plex-frontend/src/styles/style.css` (linha do `body` com `text-body`) ou mapeamento do token no `tailwind.config`.
3. **BAIXA — componente morto com import de `vue-router`.**
   `vue/plex-frontend/src/components/Sidebar/SidebarItem.vue` não é importado por ninguém e usa `useRoute()`, contrariando o princípio "navegação fora do componente". Não afeta runtime.
   Correção esperada: remover `vue/plex-frontend/src/components/Sidebar/SidebarItem.vue` (ou eliminar o import de router se for reutilizado).

## Cobertura limitada

- API real retorna apenas `key=1`, `title=Animes`; cenário de gap com 2+ bibliotecas foi verificado com mock read-only da resposta de `/api/plex/libraries` (gap real de layout `4px`).
- Não foram executados logout, estados vazio/erro da API nem `prefers-reduced-motion`, por não fazerem parte das checagens obrigatórias desta rodada.

## Screenshots

- `/tmp/opencode/qa-sidebar/light.png`
- `/tmp/opencode/qa-sidebar/dark.png`
- `/tmp/opencode/qa-sidebar/collapsed.png`
- `/tmp/opencode/qa-sidebar/viewport-768.png`
- Extras: `/tmp/opencode/qa-sidebar/r2-series.png`, `/tmp/opencode/qa-sidebar/r2-two-libs.png`
- Dados brutos: `/tmp/opencode/qa-sidebar/r2-results.json`, `/tmp/opencode/qa-sidebar/probe-out.json`
