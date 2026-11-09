# Deficiências Resolvidas — unificar-sincronizacao

Goldfish Review A/S/C sobre os artefatos da change. Achados triados como
**aceito**, **rejeitado (com motivo)** ou **parcial**.

## Revisor A — Arquitetura

| # | Achado | Triagem | Ação |
|---|--------|---------|------|
| A1 | Código ainda tem dois intervalos (`AppSettings.cs`, `UpdateAppSettingsUseCase.cs`, `SqliteAppSettingsStore.cs`) | Aceito | É o trabalho da Phase 2; tasks cobrem remoção completa nos 5 pontos. |
| A2 | `UpdateAppSettingsUseCase` valida/persiste `SyncCatalogIntervalMinutes` | Aceito | Task explícita de remoção do campo/validação. |
| A3 | `POST /api/plex/scan` duplica lógica inline | Aceito | Task: alias delega a `SynchronizationOrchestrator`, remove inline. |
| A4 | `SyncView.vue` chama `/api/plex/scan` | Aceito | Task: `startScan → POST /api/sync/run` + helper `syncRun()`. |
| A5 | `IScanJobStore` singleton sem eviction | Parcial | Não-bloqueante. Aceito risco; documentado em design (Riscos). Eviction quando houver uso prolongado. |
| A6 | Lógica de catálogo copiada para Application | Aceito | Resolvido: extraída em `CatalogSyncUseCase` (decomposição SOLID). |
| A7 | Status não expõe fase atual | Rejeitado (motivo) | Progresso por etapa vai nos logs SSE; spec exige "logs/status informam a etapa", atendido pelo log. Não adicionar campo novo sem necessidade. |
| A8 | `CompleteSetupUseCase`/snapshot persistem campo catálogo | Aceito | Task de remoção cobre ambos. |

## Revisor S — SOLID/coesão

| # | Achado | Triagem | Ação |
|---|--------|---------|------|
| S1 | `SynchronizationUseCase` monolítico (2 etapas + logs) | Aceito | Design/tasks atualizados: `CatalogSyncUseCase` + `EpisodeMarkerSyncUseCase` + `SynchronizationOrchestrator`. |
| S2 | `IScanJobStore` acoplaria Application | Parcial | `IScanJobStore` já é porta em `Domain.Ports` (não Application). Orquestrador consome a porta; logging fica no orquestrador. Aceito como está. |
| S3 | Deve reduzir deps por classe para ≤4 | Aceito | Cada caso de uso desenhado com exatamente 4 dependências; orquestrador 4. |
| S4 | Semáforo de concorrência para jobs | Parcial | Aceito risco benigno (job-keyed store); semáforo só se surgir corrupção. |
| S5 | Generalizar `SyncResult` / renomear métricas | Rejeitado (motivo) | Manter nomes atuais de métricas (`plex_catalog_sync_total`, `plex_scan_duration_ms`) evita quebrar dashboards/telemetria existentes. |

## Revisor C — Clean Code/escopo/testabilidade

| # | Achado | Triagem | Ação |
|---|--------|---------|------|
| C1 | Testes existentes quebram (`SettingsPersistenceTests.cs`) | Aceito | Task explícita: adaptar o teste (snapshot/request sem `SyncCatalogIntervalMinutes`). Pré-requisito do build gate. |
| C2 | Migração silenciosa, sem teste de compatibilidade | Aceito | Task: teste de loja com chave antiga presente → leitura sem exceção. |
| C3 | Corrida job agendado × force sync | Aceito | Documentado em design (Riscos, aceito). Sem semáforo. |
| C4 | `syncRun()` em `api.js` não consumido pelos views | Aceito | Task: `SyncView.vue`/dashboard chamam o helper efetivamente. |
| C5 | Duplicação persiste se `/api/plex/scan` não delegar | Aceito | Task de alias com remoção da lógica inline. |
| C6 | Typo `sync_interval_minentes` no design | Aceito | Corrigido para `sync_interval_minutes`. |
| C7 | Escopo do dashboard vago | Aceito | Task de grep e ajuste do dashboard se referenciar dois intervalos. |
| C8 | Cenário "falha etapa 1 impede etapa 2" sem teste | Aceito | Design/testes incluem o caso de falha no orchestrator. |
| C9 | `AppSettingKeys.All` perde entrada | Aceito | Task de verificação de consumo antes de remover. |
| C10 | Sem teste do `BackgroundService` loop | Aceito | Rejeitado adicionar (baixo valor); registrado como fora de escopo. |

## Resultado

Bloqueantes A1–A4, S1, S3, C1 endereçados: decomposição SOLID aplicada ao
design/tasks, alias de endpoint e frente unificada, remoção total do segundo
intervalo e adaptação dos testes existentes. `openspec validate --strict` passa.
