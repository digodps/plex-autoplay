# Tasks

## 1. Pipeline de deploy

- [x] 1.1 Adicionar verificação de branch/commit e bloqueio de working tree sujo em `build.sh`.
- [x] 1.2 Trocar reaproveitamento de `node_modules` por `npm ci` e manter build do Vue + substituição de `wwwroot`.
- [x] 1.3 Adicionar `dotnet build PlexAutoplay.slnx -c Release` como gate da solução.
- [x] 1.4 Exportar `GIT_COMMIT`, repassar build arg no Compose e gravar label OCI no Dockerfile.
- [x] 1.5 Conferir label da imagem após o build e falhar em divergência.
- [x] 1.6 Manter recriação do container e health check configurável por `DEPLOY_HEALTH_URL`.
- [x] 1.7 Imprimir a URL final com IP do servidor e porta publicada.
- [x] 1.8 Criar comando `/deploy` em `.agent-source/commands/deploy.md`.
- [x] 1.9 Resolver a raiz git do diretório corrente, atualizar o branch com `git fetch` + `git merge --ff-only` e remover o container com `docker compose down --remove-orphans` antes de recriar.

## 2. Skill

- [x] 2.1 Criar `.agent-source/skills/deploy/SKILL.md` com o fluxo, garantias e diagnóstico.
- [x] 2.2 Executar `python3 scripts/sync-agent-configs.py` e validar a cópia em `.opencode`.

## 3. Verificação

- [x] 3.1 Rodar `bash -n build.sh` e `openspec validate deploy-skill --strict`.
- [x] 3.2 Executar `./build.sh` em working tree limpo e confirmar label igual ao commit e `/settings` com HTTP 200.
- [x] 3.3 Confirmar que working tree sujo aborta sem `ALLOW_DIRTY=1`.
- [x] 3.4 Confirmar URL final com IP do servidor e `/deploy` em `.opencode/commands/deploy.md`.
