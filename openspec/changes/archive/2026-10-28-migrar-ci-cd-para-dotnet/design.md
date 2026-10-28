# Design

## Context

A main está na era .NET/Vue (solution `PlexAutoplay.slnx` com 4 projetos de produção, sem projetos de teste), mas os workflows carregam o passado Python: `ci.yml` roda setup-python + ruff/mypy/pytest (falha em 10s) e duplica o publish; `test.yml` falha no parse (0s) porque `working_dir`/`command` não são chaves válidas de `services:` no GitHub Actions. O `publish-container.yml` é o único verde e permanece. O stack de e2e local (`docker-compose.test.yml` + `Dockerfile.test-executor` + `scripts/run-e2e-tests.sh`) funciona no host Podman rootless, mas tem endereçamento específico de Podman (`plex-autoplay.dns.podman`, `security_opt: label=disable`) que não existe no Docker do Actions. O mock do Plex (`tests/e2e/mock-plex/server.js`, Node puro) é montado como volume e servido via `node server.js`.

## Goals / Non-Goals

**Goals:**
- CI verde e útil: build .NET + e2e em container em PR e push para main.
- Um único workflow de CI (`ci.yml`); `test.yml` removido.
- O mesmo stack de teste roda localmente (Podman) e no CI (Docker) sem edição manual.
- Plex mock dentro da rede do Compose, alcançável pela aplicação e pelo executor.

**Non-Goals:**
- Projeto de testes de unidade C# (evolução futura; o CI nasce sem job de unit).
- Build/publicação do frontend Vue (`vue/plex-frontend`) na imagem — o `Dockerfile` atual é só .NET e segue assim.
- Mudanças no `publish-container.yml` e `copilot-setup-steps.yml`.
- Lint/format no CI (não existem configs .NET no repo hoje).

## Decisions

1. **Consolidar tudo em um único `ci.yml`** (jobs `build` e `e2e`) e remover `test.yml`.
   - *Alternativa considerada*: manter dois workflows separados. Rejeitada: dois workflows de teste competindo pelo mesmo trigger é a raiz da confusão atual; e2e no PR é o que barra regressão.
2. **`plex-mock` como serviço do `docker-compose.test.yml`** (imagem `node:20-slim`, volume `./tests/e2e/mock-plex:/app`, comando `node server.js` via compose — no Compose essas chaves são válidas), na rede `plex-test-network`.
   - *Alternativa considerada*: service container do runner com imagem custom que embute o CMD. Rejeitada: service container não é alcançável pelo hostname `plex-mock` de dentro da rede do Compose no Actions, e exigiria imagem nova; o mock dentro do stack resolve rede e schema de uma vez.
   - `PLEX_URL` passa a apontar para `http://plex-mock:32400` (nome de serviço Compose) em ambos os ambientes.
3. **`APP_HOST` parametrizado por env**: `scripts/run-e2e-tests.sh` lê `APP_HOST` do ambiente (default `plex-autoplay.dns.podman` para o local, preservando o comportamento atual); o CI injeta `APP_HOST=plex-autoplay-test` (hostname do container) via env do executor. O compose repassa para `PLAYWRIGHT_BASE_URL`.
   - *Alternativa considerada*: detectar Podman no script. Rejeitada: detecção implícita frágil; env explícito é determinístico nos dois ambientes.
4. **`security_opt: label=disable` fica no compose**: inofensivo/reconhecido pelo Docker do Actions e necessário no Podman rootless local — não precisa parametrizar.
5. **Build do job `build` usa `actions/setup-dotnet`** com a versão do SDK 10 e cache NuGet (`cache: true` / `cache-dependency-path` dos csproj), seguindo o padrão de `publish-container.yml` (checkout v4, actions v3/v6).
6. **Artefatos de falha do e2e** (`test-artifacts/`) publicados com `actions/upload-artifact@v4` condicionado a `failure()` — o executor já os gera em `./test-artifacts/`.

## Risks / Trade-offs

- [Rede do Compose no Actions] O nome do container (`plex-autoplay-test`) deve ser o hostname usado pelo executor; se o alias de rede divergir, o e2e falha de conexão → Mitigação: fixar `container_name` + usar esse valor no `APP_HOST` do CI; o run de e2e no Actions valida no primeiro PR.
- [`depends_on` sem healthcheck] O executor pode subir antes do mock/app estarem prontos → Mitigação: manter `condition: service_started` e a lógica de espera/retry já existente no executor de testes e2e (o mock sobe em milissegundos; a app responde no `/`).
- [Run e2e no CI sem segredo de Plex] O mock cobre as rotas consumidas, mas qualquer rota nova da app contra o Plex real pode não existir no mock → Mitigação: o mock é parte do repo; rotas novas entram no mock junto com a feature (já é o fluxo atual dos testes locais).
- [`label=disable` no Docker rootful] Comportamento pode variar entre versões do Docker Compose → Mitigação: se o Compose do Actions rejeitar a chave, mover para `options`/env; decisão low-risk revisável no primeiro run.

## Migration Plan

- A mudança é de workflow/infra: sem deploy de aplicação. Rollback = reverter o commit (o `publish-container.yml` intocado garante a imagem da main durante todo o período).
- Ordem: ajustar compose/script e validar localmente (Podman) → reescrever `ci.yml` → remover `test.yml` → validar por PR (checks de build + e2e no PR antes do merge).

## Open Questions

- Nenhuma — as decisões do modo exploração foram fechadas (mock no Compose, sem unit tests agora, e2e no PR, `test.yml` removido).
