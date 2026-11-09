---
description: "Compila o branch atual, publica Vue e .NET em Docker, reinicia o container e mostra a URL final com o IP do servidor."
---

Invoca a skill `deploy`. Executa o pipeline de deploy do branch atual e, no final, informa a URL de acesso usando o IP do servidor.

Uso:
```
/deploy
```

Comportamento:
1. Valida branch/commit do projeto no diretório corrente e working tree (bloqueia alterações rastreadas não commitadas).
2. Atualiza o branch atual: `git fetch --prune origin` + `git merge --ff-only` do upstream.
3. Executa `./scripts/build.sh`.
4. Frontend: `npm ci` + `npm run build`; substitui `wwwroot`.
5. Compila `PlexAutoplay.slnx -c Release` e publica DLLs via Docker.
6. Remove o container (`docker compose -f infra/docker-compose.yml down --remove-orphans`), recria com a imagem do branch atualizado e valida `/settings`.
7. Mostra a URL final no formato `URL: http://<IP-do-servidor>:<porta>`.

Opções:
```
ALLOW_DIRTY=1 /deploy
DEPLOY_HEALTH_URL=http://127.0.0.1:18087/settings /deploy
```

Em falha, mostrar o erro, os logs do container e não declarar deploy concluído.
