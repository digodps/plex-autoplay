---
name: deploy
description: Compila o branch atual, publica Vue e .NET em imagem Docker, recria o container e valida health.
allowed-tools: Bash(*)
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Skill `deploy` (`/deploy`)

Deploy reproduzível do branch atual. `build.sh` é a fonte de verdade: a skill não duplica comandos, apenas executa, valida e diagnostica.

## Fluxo

1. Entrar no projeto corrente (o diretório da sessão OpenCode) e confirmar contexto:
   ```bash
   git rev-parse --show-toplevel
   git branch --show-current
   git status --short
   ```
   O deploy usa o repositório e o branch do diretório corrente. Working tree com alterações rastreadas não commitadas bloqueia o deploy. Para publicar estado local conscientemente: `ALLOW_DIRTY=1 ./build.sh`.

2. Executar o pipeline:
   ```bash
   ./build.sh
   ```
   O script:
   - resolve a raiz do repositório a partir do diretório corrente e imprime branch e commit;
   - recusa working tree sujo;
   - roda `git fetch --prune origin` e `git merge --ff-only` do upstream do branch atual, para compilar o branch atualizado; sem upstream, avisa e usa o estado local;
   - roda `npm ci` em `vue/plex-frontend` (pacotes exatos do `package-lock.json`);
   - roda `npm run build` e substitui `src/PlexAutoplay.Web/wwwroot` pelo `dist` novo, sem assets antigos;
   - compila `PlexAutoplay.slnx -c Release`;
   - roda `docker compose build`, cujo estágio `dotnet publish` gera as DLLs a partir do contexto atual (sem `bin`/`obj` do host, ignorados pelo `.dockerignore`);
   - confere o label `org.opencontainers.image.revision` da imagem contra o commit do branch;
   - remove o container com `docker compose down --remove-orphans`;
   - recria o container `plex-autoplay` a partir da imagem do branch atualizado e valida HTTP 200 em `/settings`;
   - imprime no final `URL: http://<IP-do-servidor>:<porta>` usando o IP primário da máquina e a porta publicada.

3. Confirmar resultado:
   ```bash
   docker inspect --format '{{ index .Config.Labels "org.opencontainers.image.revision" }}' plex-autoplay:latest
   git rev-parse HEAD
   ```
   Os dois valores devem ser iguais.

## Garantias

| Artefato | Origem no deploy |
| --- | --- |
| Vue/bundle | `npm ci` + `npm run build` + `wwwroot` recriado |
| DLLs .NET | `dotnet build` da solução + `dotnet publish` no Docker |
| Pacotes NuGet | restore do `publish` no Docker, sem cache de `obj` do host |
| Imagem | `docker compose build` com `GIT_COMMIT` do branch |
| Container | recriado com `--no-build` após validar o label |

## Opções

- `ALLOW_DIRTY=1 ./build.sh`: permite alterações rastreadas não commitadas.
- `DEPLOY_HEALTH_URL=http://host:porta/rota ./build.sh`: troca o endpoint de health.

## Falhas comuns

- **Working tree sujo**: commitar ou usar `ALLOW_DIRTY=1`. A imagem não representará o branch se houver diff pendente.
- **Branch divergente do upstream**: `git merge --ff-only` falha; rebase/merge manual antes do deploy.
- **Diretório errado**: rodar `/deploy` no projeto desejado; o script usa a raiz git do diretório corrente.
- **Label divergente**: `docker compose build` rodou sem `GIT_COMMIT` exportado. Rodar via `./build.sh`.
- **Health 200 ausente**: o script imprime `docker logs plex-autoplay --tail 20`. Verificar `.env`, portas e volume `./data`.
- **Porta duplicada**: remover `docker-compose.override.yml` ou ajustar o mapeamento antes do deploy.
- **URL final**: sempre reportar a linha `URL: http://...` ao usuário.

## Fonte OpenSpec

- Change: `openspec/changes/deploy-skill/`
- Spec: `openspec/changes/deploy-skill/specs/deploy/spec.md`
- Após editar, executar `python3 scripts/sync-agent-configs.py`.
