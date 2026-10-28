# Tasks

## 1. Corrigir versão OpenTelemetry (corrigir-versao-opentelemetry-prometheus)

- [x] 1.1 Corrigir versão do OpenTelemetry.Exporter.Prometheus e remover AddPrometheusExporter — verificar: `src/PlexAutoplay.Web/PlexAutoplay.Web.csproj` e `src/PlexAutoplay.Infrastructure/Telemetry/OpenTelemetryService.cs`

## 2. Executor de testes e2e (executor-de-testes-e2e)

- [x] 2.1 Criar docker-compose.test.yml, Dockerfile.test-executor, scripts/run-e2e-tests.sh, workflow GitHub Actions e Plex mock — verificar: `docker-compose.test.yml`, `Dockerfile.test-executor`, `scripts/run-e2e-tests.sh`, `.github/workflows/test.yml`, `tests/e2e/mock-plex/server.js`

## 3. Corrigir testes e2e para stack .NET/Vue (corrigir-testes-e2e-stack-dotnet)

- [x] 3.1 Atualizar seletores de UI, caminhos de scripts e endpoints dos testes e2e — verificar: `tests/e2e/tests/container.spec.js`, `tests/e2e/tests/libraries.spec.js`, `tests/e2e/tests/markers.spec.js`, `tests/e2e/tests/images.spec.js`

## 4. HTTPS-only com certificado (https-somente-certificado)

- [x] 4.1 Configurar Kestrel HTTPS-only, registro DI do IMarkerRepository, e correções de aplicação — verificar: `src/PlexAutoplay.Web/Program.cs`, `docker-compose.yml`, `Dockerfile`, `.env`

## 5. Cache genérico plugável (implementar-cache-generico-plugavel)

- [x] 5.1 Implementar porta ICacheService, ImageCacheService com IMemoryCache e integração no ImageProxyService — verificar: `src/PlexAutoplay.Domain/Ports/ICacheService.cs`, `src/PlexAutoplay.Infrastructure/Cache/ImageCacheService.cs`, `src/PlexAutoplay.Infrastructure/PlexProxy/ImageProxyService.cs`

## 6. Novos arquivos de infraestrutura (checkpoint)

- [x] 6.1 Adicionar novos arquivos de infraestrutura (.NET solution, Vue SPA, testes e2e, scripts, openspec) — verificar: `PlexAutoplay.slnx`, `vue/`, `tests/e2e/`, `scripts/`, `openspec/`

## 7. Remover placeholders Class1.cs (limpeza de template)

- [x] 7.1 Remover `src/PlexAutoplay.Application/Class1.cs` e `src/PlexAutoplay.Domain/Class1.cs` — placeholders vazios gerados pelo template `dotnet new classlib`, sem referências no código — verificar: ausência dos arquivos e nenhuma referência a `Class1` em `src/`
