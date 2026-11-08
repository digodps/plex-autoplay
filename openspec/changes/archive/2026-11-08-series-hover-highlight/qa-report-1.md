# QA Report 1 — series-hover-highlight

**Resultado: PASS**

## URL validada

`http://127.0.0.1:18085` (container isolado `plex-qa-series-hover`, imagem `plex-autoplay:series-hover-highlight`, porta interna 18080).

Rotas percorridas: `/login`, `/dashboard`, `/libraries/1/series`, `/series/{key}/markers`, `/settings`.

## Evidências

- Playwright: 34/34 checks PASS (`qa-ui-check.mjs`, `PLAYWRIGHT_BASE_URL=http://127.0.0.1:18085 ADMIN_PASSWORD=admin node qa-ui-check.mjs`).
- Hover: `transform: none -> matrix(1.05, 0, 0, 1.05, 0, 0)`; overlay `rgba(17,17,17,0) -> rgba(17,17,17,0.25)`.
- Foco por Tab: card recebe foco, miniatura escala, `box-shadow` de anel visível.
- `prefers-reduced-motion: reduce`: sem zoom (`transform: none`), overlay continua.
- Tema: toggle light/dark funciona e persiste; tema escuro aplicado e hover mantém realce na página de séries.
- Menu: abrir/fechar menu do usuário, navegar para `/settings`, modal de senha abre/fecha; item ativo da sidebar e breadcrumb corretos.
- Sem erros JS de página.

Screenshots: `qa-evidence/qa-series-hover-light.png`, `qa-evidence/qa-series-hover-dark.png`.

## Build

- `npm run lint`: 0 errors (8 warnings pré-existentes).
- `npm run build`: PASS.
- `dotnet build PlexAutoplay.slnx`: PASS (0 errors).
- `docker build --no-cache -t plex-autoplay:series-hover-highlight .`: PASS.
- Container isolado iniciou e `/login` respondeu 200.

## Achados

Nenhum bloqueante. Observações fora do escopo (pré-existentes): `viewState === 'seriesError'` nunca é retornado e bloco de erro correspondente é código morto; blocos de erro duplicados usam `bg-gray-900` no tema claro.
