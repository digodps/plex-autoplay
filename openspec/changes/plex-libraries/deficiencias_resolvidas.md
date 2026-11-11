# Deficiências resolvidas — plex-libraries

Goldfish Review (A/S/C) executado sobre requisitos/artefatos da change.

## A — Arquitetura, dependências e integração

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| A1 | `LibraryDiscoveryService` (task 1.1) e `EpisodeMappingService` (task 2.1) chamam a API do Plex diretamente; o design declara reuso do `PlexApiClient` existente (`plex-api-client`) mas as tasks não especificam injeção de `IPlexApiClient`. Risco de duplicar token (`PlexRequestHandler`), retry e telemetria. | Aceito (parcial) | Funcionalidade coberta; reuso de dependência não. Ação: injetar `IPlexApiClient` nas services; proibir `new HttpClient`. |
| A2 | `GET /api/libraries` (task 3.1) duplica `GET /api/plex/libraries` existente (filtra `type="show"` e devolve key/title/type/agent). `GET /api/libraries/{sectionKey}/episodes` (task 3.2) duplica `GET /library/{section_key}` e `GET /api/series/{sectionKey}/{seriesKey}/episodes` (rating_key, title, season, episode, `local_path` via `MEDIA_ROOTS`, `thumb`). | Aceito (parcial) | Expor dados ao frontend é goal, mas rotas duplicam existentes. Ação: reusar endpoints/use cases atuais; criar novas rotas apenas com contrato justificado. |
| A3 | `MEDIA_ROOTS` como config via env/JSON array já existe: `AppSettingKeys.MediaRoots`, `MediaRoots.ParseEnvironment`, default `/media`, validação `MediaRoots.Normalize` e persistência SQLite. | Aceito | Infraestrutura pronta; task 4.1 apenas garante leitura via `IAppSettingsStore`. |
| A4 | Tasks 1.4/2.5 prescrevem `throw` de exceção específica em falha HTTP do Plex, mas `PlexApiClient.GetLibrariesAsync`/`GetEpisodesAsync` retornam `PlexLibrariesResult`/`PlexEpisodesResult` (`.Failed` + `ErrorMessage`) sem lançar exceção em não-2xx. | Aceito (parcial) | Erro "servidor Plex indisponível" já tratado pelo client. Ação: services traduzem `Result.Failed` em erro ao caller, sem exceções de controle de fluxo; testes continuam simulando status não-2xx. |
| A5 | Proposal (Impact) cita persistência de "tabelas de bibliotecas e episódios", mas spec/tasks exigem só persistir bibliotecas (`SqliteLibraryRepository` + `CatalogSyncUseCase` já existem). Não há repository/tabela de episódios; episódios são listagem transitória via API. | Aceito (parcial) | Bibliotecas cobertas. Ação: esclarecer que episódios não são persistidos (consulta sob demanda) ou adicionar tarefa de tabela/repository se houver requisito. |
| A6 | Limite com `plex-sync` não definido: `CatalogSyncUseCase` já faz upsert/remoção de bibliotecas tipo show (etapa 1 do ciclo de sync). Descoberta fora desse use case cria duas fontes de verdade e risco de corrida com `plex-sync`; o non-goal do design não define a integração. | Aceito (parcial) | Persistência de bibliotecas coberta por `CatalogSyncUseCase`. Ação: service delegar a repositórios/use case existentes para não divergir do ciclo de sincronização. |
| A7 | Proposal cita "Interface web (exibição de bibliotecas de séries)" no impacto, mas tasks não incluem tarefa de frontend (Vue em `vue/`); só endpoints backend + testes. | Aceito (parcial) | Dados expostos por endpoints; consumo na UI não planejado. Ação: adicionar tarefa de frontend quando a UI consumir os endpoints de bibliotecas/episódios. |

## S — SOLID e coesão

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| S1 | `EpisodeMappingService` (tasks 2.1–2.5) acumula múltiplas responsabilidades: chamada à API Plex, resolução de `MEDIA_ROOTS`, construção de DTO, filtro de itens não-episódio e tratamento de erro. Viola SRP e coesão. | Aceito (parcial) | Tasks separam resolução de caminho (2.2) e DTO (2.3) como passos internos, mas não extraem classes dedicadas (ex.: `MediaPathResolver`, `EpisodeDtoBuilder`). |
| S2 | `LibraryDiscoveryService` (tasks 1.1–1.4) combina chamada API, filtragem por tipo `show`, persistência no DB e tratamento de erro em um único serviço. Baixa coesão. | Aceito (parcial) | Tasks 1.2 (filtro) e 1.3 (persistência) são passos separados, mas dentro do mesmo serviço; não há repositório ou filtro dedicado. |
| S3 | Nenhuma interface ou abstração definida para `LibraryDiscoveryService` e `EpisodeMappingService`; dependências concretas do cliente Plex e do banco. Viola DIP. | Aceito (parcial) | Tasks não mencionam interfaces, injeção de dependência ou portas/adapteres; serviços provavelmente instanciados diretamente nos endpoints (tasks 3.1–3.2). |
| S4 | `spec.md` Req "Map episodes local media files" agrega listagem de episódios, resolução de caminho via `MEDIA_ROOTS` e construção de URL de miniatura em um único requisito. Requisito sem coesão. | Aceito (parcial) | Tasks 2.2 e 2.3 separam parcialmente na implementação, mas o requisito no spec permanece agregado e sem decomposição. |
| S5 | `design.md` Decision 3 define resolução de `MEDIA_ROOTS` no backend, mas não especifica classe ou camada onde essa regra reside, risco de ser embutida no `EpisodeMappingService`. | Aceito (parcial) | Task 2.2 coloca a resolução dentro do serviço de episódios; nenhuma tarefa cria `MediaPathResolver` isolado ou camada de infraestrutura de arquivos. |
| S6 | `proposal.md` declara impacto em UI ("exibição de bibliotecas de séries") mas `tasks.md` não contém tarefas de frontend. Desalinhamento de escopo entre artefatos. | Rejeitado | Não é deficiência de SOLID/coesão de código; escopo de frontend está fora deste change (sem requisitos de interface no spec). É assunto de planejamento/change separado. |

## C — Clean Code, escopo e testabilidade

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| C1 | `spec.md`: Cenário "Movie library excluded" possui descrição truncada/incorreta (mistura campos de episódios ao invés de verificar exclusão por `type != "show"`). | Aceito (parcial) | O comportamento lógico de filtro está coberto no requisito, mas o texto do cenário no spec necessita correção de redação não contemplada explicitamente nas tarefas. |
| C2 | `spec.md`: Inconsistência de nomenclatura entre o Purpose (`MEDIA_ROOT` no singular) e os cenários/tarefas (`MEDIA_ROOTS` no plural). | Aceito (parcial) | A tarefa 4.1 trata o parâmetro `MEDIA_ROOTS`, porém o texto descritivo do Purpose do spec permanece no singular. |
| C3 | `tasks.md`: Proposta de criação de novos serviços (`LibraryDiscoveryService` e `EpisodeMappingService`) duplica componentes já implementados na arquitetura (`PlexApiClient`, `MediaPathResolver` e `SeriesEndpoints`). | Aceito (parcial) | As tarefas devem focar na persistência em banco e testes de integração sobre a infraestrutura existente, evitando recriação de código. |
| C4 | `proposal.md`: A seção Impacto cita a interface web (exibição de bibliotecas), porém `tasks.md` não contempla tarefas de front-end. | Aceito (parcial) | A exibição na UI pode estar coberta por outra especificação ou escopo adjacente, mas gera desalinhamento documental no proposal. |

**Nenhum achado altera requisitos do spec.** Os achados aceitos são atendidos pelas tarefas em `tasks.md` (com ajustes de implementação conforme ações). Registro em `deficiencias_resolvidas.md` concluído.