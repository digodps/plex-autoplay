# EGM — Configurações no banco

## Revisão A — Arquitetura

| # | Achado | Decisão | Resolução |
|---|---|---|---|
| A1 | `/api/plex/save` e `/config` públicos gravam/leem credencial | Aceito | Rotas removidas; allowlist anônima removida do middleware; `/api/settings` exige JWT. |
| A2 | Spec `backend-architecture` e `ArchitectureTests` citam `IPlexConfigStore` | Aceito | Delta de `backend-architecture`; teste passa a proibir `IAppSettingsStore` em Web. |
| A3 | `IPlexApiClient` singleton captura `HttpClient` com base/token fixos | Aceito | Registro transiente via `IHttpClientFactory`; sem `BaseAddress`/token fixos. |
| A4 | `GetIdentityAsync` muta `DefaultRequestHeaders` compartilhados | Aceito | Token por `HttpRequestMessage`; sem mutação de estado global. |
| A5 | `MEDIA_ROOT` congelado no composition root e nos endpoints | Aceito | Removido parâmetro `mediaRoot`; handlers e services leem `IAppSettingsStore`. |
| A6 | Ordem do seed e perda de efeito do `.env` antigo | Aceito | Seed após migração, somente com tabela vazia; sem re-seed; documentado. |
| A7 | Sem WAL + leitura por uso pode gerar contenção | Parcial | WAL e `busy_timeout` aceitos; cache adiado (leitura por PK é barata). |
| A8 | Defaults duplicados em camadas | Aceito | `AppSettingKeys`/`AppSettingDefaults` únicos em Application. |

## Revisão S — SOLID / coesão

| # | Achado | Decisão | Resolução |
|---|---|---|---|
| S1 | `/api/plex/save` e `/config` sem JWT | Aceito | Ver A1. |
| S2 | Duas fontes de URL/token no mesmo client | Aceito | Handler único; `GetIdentityAsync` usa request próprio. |
| S3 | `MEDIA_ROOT` fechado na assinatura dos endpoints | Aceito | Ver A5. |
| S4 | `IAppSettingsStore` gordo (reader/writer/seed no mesmo contrato) | Parcial | Porta reduzida a `GetAsync`/`SaveAsync`; seed em classe separada. Split reader/writer e providers por concern rejeitado: 5 campos, sem consumidor independente. |
| S5 | Defaults duplicados | Aceito | Ver A8. |
| S6 | Três regras de composição de URL | Aceito | Handler normaliza base; `ImageProxyService` e auto-skip usam `TrimEnd('/')` + concatenação consistente. `IPlexUrlBuilder` rejeitado: três usos simples. |
| S7 | Validação replicada em endpoints | Aceito | `UpdateAppSettingsUseCase` central; endpoints só traduzem DTO. |
| S8 | Cache de imagem ignora URL/token | Aceito | Cache key inclui `plex_url`. Invalidação de prefixo rejeitada: chave nova já isola servidor. |

## Revisão C — Clean Code / escopo

| # | Achado | Decisão | Resolução |
|---|---|---|---|
| C1 | `/api/plex/save` e `/api/plex/identity` anônimos | Aceito | Rotas removidas da allowlist; `/api/plex/save` removido. |
| C2 | Token em texto puro, retornado inteiro e campo visível | Parcial | Token não é retornado (`plex_token_set`); campo `type="password"`; PUT só altera quando enviado. Criptografia em repouso rejeitada: exigiria gerenciamento de chave e não há requisito; registrado como limite. |
| C3 | Validação fraca e semântica parcial | Aceito | `Uri.TryCreate`, `1..1440`, `Path.IsPathRooted` + `Directory.Exists`, validação antes da transação. |
| C4 | Seed por chave ausente ressuscita valor | Aceito | Seed só quando tabela vazia. |
| C5 | Cache de imagem ignora nova URL | Aceito | Ver S8. |
| C6 | Porta grande e `/config` sem consumidor | Aceito | Porta mínima; `/config` removido. |
| C7 | Sem testes do efeito imediato/autorização | Parcial | Testes de store/seed/validação e build/QA; teste de handler e autorização incluído no teste unitário. E2E de troca de servidor real depende de Plex disponível; coberto por unitário. |
| C8 | `SettingsView` não atende spec | Aceito | View reescrita: `onMounted`, sem `localStorage`, 5 campos, `max=1440`, `extractErrorMessage`. |

## Limites registrados

- Token do Plex fica em texto puro no SQLite local, como já ocorria no `.env`.
  Upgrade futuro: criptografia com chave derivada de `JWT_SECRET`.
- Alterar `MEDIA_ROOT` não cria mount Docker; a validação exige diretório
  existente no container.
