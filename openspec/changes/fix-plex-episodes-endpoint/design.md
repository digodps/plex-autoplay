# Design: fix-plex-episodes-endpoint

## Context

`PlexApiClient.GetEpisodesAsync` é usado por `GET /library/{sectionKey}` e pelo scan de bibliotecas. O método monta a URL `/library/sections/{sectionKey}/items`, mas o Plex responde `404` para esse caminho. A consulta real ao servidor confirmou que `/library/sections/6/all?type=4` retorna `200` com 241 episódios (`type="episode"`).

## Goals / Non-Goals

**Goals:**

- Corrigir a URL de episódios por seção sem alterar o contrato de resposta.
- Cobrir a regressão com teste unitário e mock E2E.
- Fazer o teste de aceitação parar de reprovar em `GET /library/{sectionKey}` e no scan.

**Non-Goals:**

- Alterar paginação do scan ou do endpoint de biblioteca.
- Alterar a consulta de episódios por série (`/library/metadata/{seriesKey}/allLeaves`), que já está correta.
- Alterar frontend.

## Decisions

**1. Usar `/library/sections/{sectionKey}/all?type=4`**

Rationale: é o endpoint que o Plex real aceita para retornar apenas episódios. `type=4` é o identificador numérico de episódio usado pela API do Plex; `type=episode` retorna `500`.

**2. Manter o filtro `type == "episode"` no parsing**

Rationale: protege contra versões/proxies que ignorem o parâmetro e devolvam outros tipos.

**3. Atualizar o mock para o mesmo endpoint**

Rationale: mantém os testes E2E alinhados ao contrato real e evita que o mock mascare a regressão.

## Risks / Trade-offs

[Bibliotecas grandes retornam todos os episódios em uma única resposta] → Fora do escopo; o comportamento atual já era sem paginação. Pode ser otimizado depois com `X-Plex-Container-Start/Size`.

[Proxy Plex antigo pode não suportar `type=4`] → O filtro defensivo evita dados inválidos; o erro HTTP continua sendo reportado como falha explícita.

## Migration Plan

1. Alterar a URL em `GetEpisodesAsync`.
2. Adicionar teste unitário verificando path/query e parsing.
3. Atualizar o mock E2E.
4. Rodar testes unitários, E2E e aceitação.
5. Rollback: reverter a URL para `/items` apenas se o ambiente exigir um proxy legado.

## Open Questions

Nenhuma.
