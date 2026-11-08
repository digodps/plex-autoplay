---
description: "Compila o branch atual, publica Vue e .NET em Docker, reinicia o container e mostra a URL final com o IP do servidor."
---

Invoca a skill `deploy`. Executa o pipeline de deploy do branch atual e, no final, informa a URL de acesso usando o IP do servidor.

Uso:
```
/deploy
```

Comportamento:
1. Valida branch/commit e working tree (bloqueia alterações rastreadas não commitadas).
2. Executa `./build.sh`.
3. Frontend: `npm ci` + `npm run build`; substitui `wwwroot`.
4. Compila `PlexAutoplay.slnx -c Release` e publica DLLs via Docker.
5. Recria o container e valida `/settings`.
6. Mostra a URL final no formato `URL: http://<IP-do-servidor>:<porta>`.

Opções:
```
ALLOW_DIRTY=1 /deploy
DEPLOY_HEALTH_URL=http://127.0.0.1:18087/settings /deploy
```

Em falha, mostrar o erro, os logs do container e não declarar deploy concluído.
