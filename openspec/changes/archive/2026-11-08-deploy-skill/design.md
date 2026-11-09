# Design

## Context

`build.sh` já faz build do Vue, cópia para `wwwroot`, `docker compose build`, recriação do container e health check. Faltam garantias: pacotes podem estar velhos (`node_modules` reaproveitado), DLLs dependem do cache do Docker, working tree pode divergir do branch e não há prova de qual commit gerou a imagem.

## Decisions

### 1. Um único entrypoint: `build.sh`

A skill chama `scripts`/`build.sh` existente em vez de duplicar comandos. O script continua sendo a fonte de verdade e pode ser executado manualmente.

### 2. Instalação limpa do frontend

`npm ci` roda sempre. Ele remove `node_modules` e instala exatamente o `package-lock.json`, garantindo que Vue, Vite e plugins correspondam ao branch. Custo aceito: deploy mais lento.

### 3. Compilação e publish

`dotnet build PlexAutoplay.slnx -c Release` valida a solução inteira no host. O publish efetivo acontece no estágio `build` do Dockerfile, sempre a partir do contexto atual; nenhum `bin`/`publish` do host é reaproveitado. `wwwroot` é apagado antes da cópia para eliminar assets antigos com hash.

### 4. Rastreabilidade por label

`GIT_COMMIT` é exportado pelo `build.sh`, repassado como build arg pelo Compose e gravado como label OCI no estágio final. Após o build, `docker inspect` compara o label com `git rev-parse HEAD`; divergência aborta o deploy.

### 5. Proteção contra estado sujo

Alterações rastreadas não commitadas indicam que a imagem não corresponderia ao branch. O script aborta, a menos que `ALLOW_DIRTY=1` seja passado conscientemente. Arquivos não rastreados não bloqueiam, e `src/PlexAutoplay.Web/wwwroot` é ignorado na checagem porque é regenerado pelo próprio deploy.

### 6. Health check

Mantém polling em `http://127.0.0.1:8080/settings`, sobrescrevível por `DEPLOY_HEALTH_URL`. Em falha, mostra logs do container e sai com erro.

### 7. URL final

Após health 200, o script detecta o IP primário com `ip route get 1.1.1.1` (fallback `hostname -I`) e a porta publicada com `docker port plex-autoplay 8081/tcp` (fallback 8080), imprimindo `URL: http://<ip>:<porta>` como última linha.

### 8. Comando `/deploy`

`.agent-source/commands/deploy.md` expõe o atalho no OpenCode e delega para a skill `deploy`, que executa `./build.sh`.

## Risks

- `npm ci` aumenta o tempo de deploy; mitigado por cache do npm quando disponível.
- Build arg ausente em execução manual de `docker compose build` grava `local`; o `build.sh` sempre exporta o commit.
- Working tree sujo é bloqueado por padrão; fluxo de desenvolvimento usa `ALLOW_DIRTY=1`.
