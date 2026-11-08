# Proposal

## Why

O deploy é feito por passos manuais (`build.sh` + comandos avulsos). Artefatos antigos podem sobreviver ao processo: `node_modules` desatualizado, `dist`/`wwwroot` de outra execução, DLL publicada de cache do Docker ou container rodando imagem anterior. Não há vínculo verificável entre o commit do branch e a imagem em execução.

## What Changes

- Endurecer `build.sh` para o fluxo completo: validar branch/commit, reinstalar pacotes do frontend, compilar a solução, republicar DLLs via Docker, recriar o container e validar health.
- Publicar a imagem com label `org.opencontainers.image.revision` igual ao commit do branch e conferir o label após o build.
- Bloquear deploy de working tree com alterações rastreadas não commitadas; permitir override explícito com `ALLOW_DIRTY=1`.
- Criar skill `/deploy` que executa e documenta o fluxo, incluindo diagnóstico de falha.

## Capabilities

### New Capabilities

- `deploy`: build reproduzível e verificável do branch atual até o container em execução.

### Modified Capabilities

- None.

## Impact

- `build.sh`: verificação de git, `npm ci`, `dotnet build`, label/verificação da imagem, URL final com IP do servidor.
- `Dockerfile`: label de revisão no estágio final.
- `docker-compose.yml`: build arg `GIT_COMMIT`.
- `.agent-source/commands/deploy.md`: atalho `/deploy` no OpenCode.
- `.agent-source/skills/deploy/SKILL.md` e cópia sincronizada em `.opencode`.
