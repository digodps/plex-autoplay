# Tasks — Dashboard Card: Sessões Ativas e Histórico de Skip

## Backend (.NET)
- [ ] Criar `SkipHistoryEntry` (entidade de domínio)
- [ ] Criar `SessionStatus` + `MarkerTarget` (DTOs de status de sessão)
- [ ] Criar porta `ISkipMonitor`
- [ ] Criar implementação `SkipMonitor` (singleton, thread-safe, anel de 50 skips)
- [ ] Registrar `ISkipMonitor` no DI (`ServiceCollectionExtensions`)
- [ ] Injetar `ISkipMonitor` no `PlexAutoSkipBackgroundService` e atualizar snapshot de sessões + registrar skips executados
- [ ] Criar endpoint `GET /api/sessions/skip-status` (`SessionsEndpoints`)
- [ ] Registrar `MapSessionsEndpoints()` em `Program.cs`

## Frontend (Vue)
- [ ] Adicionar card "Sessões & Skips" em `useDashboardCards.js` (`skipSessionsCard`)
- [ ] Renderizar card no `DashboardView.vue`
- [ ] Criar view `SessionsView.vue` (tabelas sessões ativas + skips recentes, polling 5s, tema light/dark)
- [ ] Adicionar rota `/sessions` no `router/index.js`

## QA
- [ ] Build backend passa (`dotnet build PlexAutoplay.slnx -c Release`)
- [ ] Build frontend passa (`npm run build` em `vue/plex-frontend`)
- [ ] Lint frontend passa (`npm run lint` em `vue/plex-frontend`)
- [ ] Playwright: tema light/dark aplicado; card navega para /sessions; /sessions renderiza tabelas e polling
- [ ] `openspec validate dashboard-card-skip-execucao --strict` passa
