# Proposal

## Why

O pipeline de CI/CD herdado da solução Python está quebrado após a migração para .NET/Vue: o `ci.yml` ainda instala Python, `requirements.txt` e roda ruff/mypy/pytest (falha em 10s), e o `test.yml` falha no parse do schema (0s, em todos os runs) porque usa chaves de Compose (`working_dir`, `command`) inválidas no `services:` do GitHub Actions. O único workflow funcional é o `publish-container.yml` (publica no GHCR). Sem CI verde não há regressão barrada em PR.

## What Changes

- **BREAKING**: Reformar `.github/workflows/test.yml` (workflow inteiro removido) — seu papel migra para o novo `ci.yml`.
- Reescrever `.github/workflows/ci.yml` para a stack .NET:
  - Job `build`: checkout + `dotnet restore`/`build` da solution (`PlexAutoplay.slnx`) com cache NuGet.
  - Job `e2e`: sobe o stack `docker-compose.test.yml` (app + executor + plex-mock) e roda `scripts/run-e2e-tests.sh` em container, no PR e na main.
  - Remover o job `docker` que duplica o publish (o `publish-container.yml` permanece como único publicador da imagem).
- Mover o `plex-mock` (`tests/e2e/mock-plex`) para dentro do `docker-compose.test.yml` como serviço da rede de teste (em vez de service container do runner do Actions).
- Parametrizar o host da aplicação (`APP_HOST`) e o `security_opt` no compose/script para funcionar tanto no Podman rootless local quanto no Docker do GitHub Actions (`.dns.podman` só no ambiente local).
- Sem projeto de testes de unidade C# nesta change (fica como evolução futura).

## Capabilities

### New Capabilities

- `ci-pipeline`: comportamento do pipeline de CI no GitHub Actions para a stack .NET/Vue — build da solution, e2e em container em PR e push, e unicidade do caminho de publish da imagem.

### Modified Capabilities

- `test-executor`: o requirement de protótipo de GitHub Actions com Plex mockado passa a exigir o mock como serviço do Compose dentro do stack de teste e compatibilidade de rede com Docker (CI) e Podman (local) — em vez de service container do runner.

## Impact

- `.github/workflows/ci.yml` (reescrita), `.github/workflows/test.yml` (remoção).
- `docker-compose.test.yml` e `scripts/run-e2e-tests.sh` (parametrização de host/rede; acréscimo do serviço `plex-mock`).
- `tests/e2e/mock-plex/` (sem mudança de código; vira serviço do Compose).
- `.github/workflows/publish-container.yml` e `copilot-setup-steps.yml` (inalterados).
- Segredos do GitHub: nenhuma credencial real de Plex é necessária — o e2e roda contra o mock.
