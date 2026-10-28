# Design

## Context

O backend já possui: proxy de imagens com cache (`ImageProxyService` → `/api/images/{path}`), port `IMarkerRepository` com `UpdateAsync` órfão (nunca usado por endpoints), repositório SQLite com `CREATE TABLE IF NOT EXISTS`, e scanner que grava marcadores detectados na mesma tabela que os manuais — hoje indistinguíveis. O frontend Vue tem views-scaffold (16–119 linhas), nenhum shell de navegação, `icons.svg` só com ícones de rede social e `style.css` default do Vite. Ver proposal.md para a motivação completa.

Decisões já confirmadas com o usuário: listar **todas** as séries, registrar **origem** do marcador, **front + back** no mesmo escopo, paginação server-side, edição estilo Plex.

## Goals / Non-Goals

**Goals:**
- Catálogo de séries fluido: capas lazy, busca com debounce, sem travamento de UI com bibliotecas grandes.
- Edição de marcadores real (PUT) com salvamento otimista e feedback ambiente, sem `confirm()` nativo ni blocks.
- Origem (`manual`/`detected`) persistida e visível, com migração aditiva inofensiva.

**Non-Goals:**
- Badges de cobertura por série na grade de séries (join séries→episódios na listagem) — listado como follow-up; ver "Open Questions".
- Reformular login/sync/settings além do shell compartilhado.
- Alterar formato do `custom.json` (PlexAutoSkip) — origem existe só no SQLite.

## Decisions

1. **Paginação delegada ao Plex com `X-Plex-Container-Start`/`Size`.** Alternativa: buscar tudo e paginar no .NET — simples, mas recarrega a biblioteca inteira por página e escala mal; ou pagar no cliente via lazy scroll — mantém memória alta. Cabeçalhos de container são suportados pelo Plex ≥ 1.22 e o custo de adicionar é mínimo. O total vem do `totalSize` do `MediaContainer`.

2. **`allLeaves` para episódios de uma série.** `GET /library/metadata/{seriesKey}/allLeaves` acha todos os episódios num pedido, vs. em mais dois saltos (`children` de seasons → `children` de cada season). O retorno traz `parentIndex`/`index` para agrupar por temporada no cliente.

3. **`Origin` como `TEXT` coluna nova com `ALTER TABLE` no boot.** `EnsureTableCreated` mantém `CREATE TABLE IF NOT EXISTS` e ganha tentativa idempotente de `ALTER TABLE Markers ADD COLUMN Origin TEXT NOT NULL DEFAULT 'manual'` (try/catch para ignorar "duplicate column"). `detected` é gravado pelo scanner existente; UI/API gravam `manual`. custom.json não muda — origem é interna ao produto.

4. **`PUT /api/markers/{ratingKey}/{kind}`** aceita `StartMs`/`EndMs` e reusa `UpdateAsync`; grava 404 se não existe (consulta antes). O scanner continua usando add-only, e o POST mantém o 409 para acompanhar sem duplicar (contrato existente; testes E2E de Playwright dependem dessa query string — não quebrar).

5. **Shell de navegação com `SVG <symbol>` e tema via CSS custom properties.** `icons.svg` ganha símbolos de UI (home, tv, bookmarks, sync, settings, search, edit, trash); o tema minimalista light/dark estende as variáveis existentes de `style.css`. Sem lib de UI (Vuetify etc.) — dependência nova desnecessária para o escopo.

6. **Editor inline com salvamento otimista e store central de marcadores (Pinia).** O merge episódios×marcadores acontece uma vez no store (chave `ratingKey`); alterações são otimistas com rollback em falha. Alternativa: refetch full por edição — tarde e re-renderiza tudo.

7. **Roteamento: `/libraries/:key/series` e `/series/:seriesKey/markers`.** Lazy routes mantêm o bundle inicial pequeno; transição `fade` existente continua para navegação suave.

8. **Token em cookie gerido pelo frontend, com guard de rotas.** O middleware `JwtAuthMiddleware` só lê o header `Authorization: Bearer` — sem cookie/CSRF no backend. Alternativa avaliada: cookie `HttpOnly` + middleware lendo cookie + proteção CSRF — mais segura, mas exige mudança de middleware e tratamento CSRF novos, fora do escopo desta change. Decisão: o cookie substitui o `localStorage` na gestão do token (`stores/auth.js`), que continua viajando no header; guard global no router intercepta rota não autenticada → `/login?redirect=<rota>` e o login devolve à `redirect` após autenticar (fallback `/dashboard`). Com "manter-me conectado" marcado o cookie é persistente (30 dias); sem a marca, cookie de sessão (encerra com o navegador) — o backend continua aceitando JWT sem `exp` (spec `plex-auth` intacta), o ciclo de vida é controlado só pelo cookie. Token inválido guardado → 401 na primeira chamada → limpa cookie e redireciona ao login.

## Risks / Trade-offs

- [Plex timeout / indisponível na navegação de catálogo] → Adequar timeouts e os endpoints de catálogo retornam 502 com mensagem amigável; cache curto de resposta de séries reduz batidas repetidas.
- [Bibliotecas muito grandes (dezenas de milhares de episódios)] → allLeaves de uma série resolve por série; grade de séries paginada nunca carrega tudo. Virtual scroll só se as páginas forem grandes (não previsto).
- [Migração `ALTER TABLE` em banco concorrido] → Executada no boot mesmo antes de abrir conexões de request; falha de coluna duplicada tratada como ok.
- [Capas por série geram muitas chamadas ao proxy] → Cache de imagem já existente cobre; `loading="lazy"` no frontend.
- [Testes E2E Playwright dependem do POST via query string] → Mantido; nenhuma mudança de contrato no POST/DELETE.
- [Cookie do token acessível por JS (XSS)] → Aceito neste escopo por ser app single-user/self-hosted sem third-party scripts; a evolução `HttpOnly` + CSRF fica documentada como alternativa em Decisões e pode ser uma change futura.

## Migration Plan

1. Migração da coluna `Origin` roda no boot (idempotente); rollback é re-deploy da versão anterior — coluna extra ignorada pelo código antigo.
2. Endpoints novos são aditivos; alternativa antiga (`/library/{section_key}`) fica intacta.
3. Front: views novas convivem com as atuais antes do refactor; `MarkersView` é substituída no fim.

## Open Questions

- Badges de cobertura por série ("x/y marcados") ficam fora deste escopo — requer cachear a agregação séries→episódios→marcadores de algum modo barato. Deferrable sem alterar specs.
