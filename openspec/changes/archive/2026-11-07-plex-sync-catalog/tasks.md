# Tasks

## 1. Modelos de domínio

- [x] 1.1 Criar entidade `Library` em `PlexAutoplay.Domain/Entities` com RatingKey, Title, Type, Agent e verificar que compila — verify: classes criadas e estilos consistentes.
- [x] 1.2 Criar entidade `Series` em `PlexAutoplay.Domain/Entities` com RatingKey, LibraryRatingKey, Título, Sumário, Thumb e verificar que compila — verify: classes criadas.

## 2. Repositórios SQLite

- [x] 2.1 Criar `SqliteLibraryRepository` implementando `ILibraryRepository` (repositório SQLite novo) com CreateTableIfNotExists, Upsert, GetAll, GetByRatingKey, Delete e verificar tabela criada — verify: banco de teste tem tabela `Library` após instanciar o repositório.
- [x] 2.2 Criar `SqliteSeriesRepository` com CreateTableIfNotExists, Upsert, GetAllByLibrary, GetByRatingKey, Delete e verificar tabela criada — verify: banco de teste tem tabela `Series` com FK para `Library` após instanciar o repositório.

## 3. Serviço de sincronização de catálogo

- [x] 3.1 Implementar `CatalogSyncBackgroundService` (BackgroundService separado do `SyncBackgroundService`) com intervalo configurável via `SYNC_CATALOG_INTERVAL_MINUTES` — verify: serviço é registrado no DI e inicia sem erro.
- [x] 3.2 Implementar passo de diff de bibliotecas: consultar Plex, filtrar `type == "show"`, upsert em `Library`, remover bibliotecas do banco não retornadas pelo Plex — verify: teste unitário com bibliotecas fictícias insere novas e remove órfãs.
- [x] 3.3 Implementar passo de diff de séries: para cada biblioteca, consultar séries do Plex, upsert em `Series`, remover séries do banco não retornadas — verify: teste unitário com séries fictícias insere novas, atualiza alteradas e remove órfãs.
- [x] 3.4 Adicionar logging de contadores (processados, inseridos, atualizados, removidos, erros) ao final de cada ciclo — verify: logs contêm os contadores após execução em teste.

## 4. DI, migração e configuração

- [x] 4.1 Registrar `CatalogSyncBackgroundService`, `SqliteLibraryRepository` e `SqliteSeriesRepository` em `ServiceCollectionExtensions.AddApplicationServices` — verify: `Program.cs` inicia e resolve os serviços.
- [x] 4.2 Adicionar migração aditiva `CREATE TABLE IF NOT EXISTS Library` e `CREATE TABLE IF NOT EXISTS Series` nos repositórios (sem tocar tabela `Markers`) — verify: instanciar repositórios em banco vazio cria as tabelas; banco existente com `Markers` continua intacto.

## 5. Endpoints de catálogo lendo do banco

- [x] 5.1 Alterar `SeriesEndpoints` (`/api/series/{sectionKey}`) para listar séries do banco filtrado por biblioteca, com paginação (`LIMIT/OFFSET`) e busca por título (`LIKE`), sem chamar `GetSeriesAsync` da API — verify: endpoint retorna séries do banco com total para paginação.
- [x] 5.2 Verificar que `/api/series/{sectionKey}/{seriesKey}/episodes` continua funcionando (resolução de caminho local via `MediaPathResolver`, leitura de episódios pode continuar via Plex) — verify: endpoint de episódios responde com lista de episódios e caminhos locais.

## 6. Testes e verificação de integração

- [x] 6.1 Escrever testes de unidade para os repositórios `Library` e `Series` (upsert, get, delete) — verify: testes passam (`dotnet test`).
- [x] 6.2 Escrever teste de unidade para o fluxo de diff de bibliotecas e séries do `CatalogSyncBackgroundService` — verify: testes passam.
- [x] 6.3 Executar a aplicação localmente e confirmar que `/api/series/{sectionKey}` retorna dados do banco e que a sincronização de catálogo roda no intervalo configurado — verify: endpoint responde e logs do serviço aparecem no console.

## Workflow follow-up

- Executar `openspec validate` para confirmar que a mudança está completa e coerente.
- Arquivar a mudança após revisão e aprovação dos requisitos.