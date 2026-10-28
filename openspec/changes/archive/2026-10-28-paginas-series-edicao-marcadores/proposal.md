# Proposal

## Why

O frontend atual é um scaffold: sem shell de navegação, sem ícones de UI, e o formulário de marcadores exige digitar ratingKey e milissegundos na mão. "Editar" hoje é apagar e recriar (não existe endpoint de atualização) e não há como distinguir marcador manual de marcador detectado pelo scanner. Além disso, não existe página para navegar pelas séries suportadas, e o `/api/markers` retorna tudo sem paginação — gargalo à medida que a biblioteca cresce.

## What Changes

- Nova página de catálogo de séries: grade com capa, título e descrição de **todas** as séries de cada biblioteca do tipo `show`, com busca (debounce) e **paginação server-side**.
- Nova página de edição de marcadores estilo Plex: ao clicar numa série, abre o editor com episódios agrupados por temporada, mostrando intro/créditos de cada episódio, com edição inline, salvamento otimista e feedback ambiente.
- **BREAKING** no wire-format (aditivo): `GET/POST /api/markers` passam a expor campo `Origin` (`manual` | `detected`); coluna nova `Origin` na tabela `Markers` com migração `ALTER TABLE` (default `manual`); o scanner grava `detected`.
- Novo endpoint `PUT /api/markers/{ratingKey}/{kind}` para atualização (usa o `UpdateAsync` existente no port, hoje órfão).
- Novos endpoints de catálogo com paginação Plex (`X-Plex-Container-Start`/`X-Plex-Container-Size`):
  - `GET /api/series/{sectionKey}?page&pageSize&query` — séries (título, summary, thumb reescrita para `/api/images/...`);
  - `GET /api/series/{sectionKey}/{seriesKey}/episodes` — episódios da série (via `allLeaves`).
- Shell de navegação no `App.vue` (sidebar com ícones, reutilizando `SVG <symbol>`), tema visual minimalista light/dark consistente.
- **Sessão autenticada em todas as páginas**: guard de rotas que redireciona para o login e retorna para a página original após autenticar; token movido de `localStorage` para cookie (para sobreviver sem "logar toda vez"); na tela de login, o usuário escolhe se deseja manter a autenticação (cookie persistente ~30 dias) ou apenas pela sessão do navegador (cookie de sessão).

## Capabilities

### New Capabilities

- `auth-session`: sessão de navegação no frontend — guard de rotas autenticado com retorno à página original, persistência do token em cookie (sessão vs. persistente) e opção "manter-me conectado" na tela de login.
- `series-catalog`: navegacão paginada do catálogo de séries por biblioteca — listar séries com metadados (título, descrição, capa), buscar, paginar server-side e listar episódios de uma série.

### Modified Capabilities

- `plex-markers`: o requisito de "criar, atualizar e remover marcadores" passa a exigir endpoint de atualização real (`PUT`), registro de origem (`manual`/`detected`) em toda gravação, e UI de edição por série com episódios agrupados por temporada.

## Impact

- **Backend**: `PlexApiClient` (novos métodos de catálogo com container paging), novo `SeriesEndpoints.cs`, `MarkerEndpoints.cs` (PUT + Origin), `SqliteMarkerRepository.cs` (migração de coluna + queries), `CustomJsonWriterService`/scanner (`PlexEndpoints.cs` grava `detected`), DTOs.
- **Frontend**: `vue/plex-frontend` — `App.vue` (shell), novo `SeriesView.vue`, refactor de `MarkersView.vue` para editor por série, sistema de ícones, tema; roteador ganha rotas `/libraries/:key/series` e `/series/:seriesKey/markers`.
- **Dados**: migração aditiva em SQLite (coluna `Origin`), sem dados destruídos.
- **Specs**: `series-catalog` e `auth-session` (novos), `plex-markers` (modificado); `plex-images` e `plex-libraries` não têm requisitos alterados; `plex-auth` (backend JWT) não muda — o middleware continua consumindo o header `Authorization`, o cookie é gerido em frontend.
