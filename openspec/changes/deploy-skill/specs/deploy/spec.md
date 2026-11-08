# deploy Specification

## Purpose

Garantir que o container em execução contenha frontend Vue, DLLs .NET e pacotes gerados a partir do branch atual, sem artefatos de builds anteriores.

## ADDED Requirements

### Requirement: Build vinculado ao branch atual

O deploy SHALL identificar branch e commit, SHALL recusar working tree com alterações rastreadas não commitadas e SHALL permitir override somente com `ALLOW_DIRTY=1`.

#### Scenario: Working tree limpo

- WHEN o deploy roda em branch com working tree limpo
- THEN o script imprime branch e commit e prossegue.

#### Scenario: Alterações não commitadas

- WHEN existem alterações rastreadas não commitadas e `ALLOW_DIRTY` não é `1`
- THEN o script aborta antes de compilar ou reiniciar o container.

### Requirement: Frontend reconstruído a partir do lockfile

O deploy SHALL executar `npm ci` e `npm run build` em `vue/plex-frontend` e SHALL substituir `src/PlexAutoplay.Web/wwwroot` pelo `dist` recém-gerado.

#### Scenario: Bundle atualizado

- WHEN o build do frontend termina
- THEN `wwwroot` contém somente os assets do `dist` atual e o bundle referenciado em `index.html` pertence ao build executado.

### Requirement: Solução compilada e imagem publicada

O deploy SHALL compilar `PlexAutoplay.slnx` em `Release`, SHALL publicar a aplicação via estágio de build do Docker e SHALL recriar o container com a nova imagem.

#### Scenario: Publicação .NET

- WHEN `docker compose build` conclui
- THEN a imagem contém as DLLs publicadas de `PlexAutoplay.Web` e dependências do commit atual.

### Requirement: Imagem rastreável e saudável

A imagem SHALL carregar `org.opencontainers.image.revision` com o commit usado no build, o script SHALL conferir o label e SHALL validar HTTP 200 em `/settings` antes de declarar sucesso.

#### Scenario: Deploy bem-sucedido

- WHEN o container reinicia com a imagem recém-construída
- THEN o label da imagem é igual ao commit do branch e `/settings` responde 200 dentro do timeout.

#### Scenario: Health falha

- WHEN `/settings` não responde 200 no timeout
- THEN o script imprime os últimos logs do container e retorna código de saída não zero.

### Requirement: URL final de acesso

Ao final de um deploy bem-sucedido, o script SHALL imprimir a URL de acesso usando o IP do servidor e a porta publicada pelo container.

#### Scenario: Deploy concluído

- WHEN o health check retorna 200
- THEN a última linha exibe `URL: http://<IP-do-servidor>:<porta>`.

#### Scenario: IP indisponível

- WHEN não é possível detectar o IP do servidor
- THEN a URL usa `127.0.0.1` como fallback.
