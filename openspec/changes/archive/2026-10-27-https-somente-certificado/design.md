# Design

## Context

O `plex-autoplay` roda em .NET 10 com Kestrel ouvindo apenas HTTP nas portas 8080 (API/SPA) e 8081 (WebSocket PlexAutoSkipService). O PlexAutoSkip original (Python) foi substituído por `PlexAutoSkipService` em .NET (`src/PlexAutoplay.Infrastructure/AutoSkip/PlexAutoSkipService.cs`), registrado como Singleton e exposto via WebSocket `/ws` na porta 8081. O `docker-compose.yml` expõe `8080:8080` e `8081:8081` com `ASPNETCORE_URLS=http://+:8080`. Não existem arquivos de certificado (`.pfx`, `.pem`, `.crt`) no repositório e nenhuma variável de ambiente de TLS. O `UseHttpsRedirection()` está presente no `Program.cs` mas não tem efeito pois não há endpoint HTTPS. A change de telemetria já foi concluída e arquivada; não há sobreposição com este escopo.

## Goals / Non-Goals

**Goals:**
- Kestrel escuta apenas HTTPS (e WSS para WebSocket)
- Certificado auto-assinado gerado automaticamente quando nenhum for fornecido
- Importação de certificado válido via variável de ambiente (caminho PFX ou conteúdo base64)
- HTTP completamente desabilitado (sem listener)
- Porta HTTPS configurável via variável de ambiente

**Non-Goals:**
- Integração com Let's Encrypt ou ACME para certificados válidos públicos
- Terminação TLS em proxy reverso externo (NGINX, Traefik, etc.)
- Suporte a mTLS (certificado do cliente)
- Gestão de rotação ou renovação de certificados

## Decisions

**1. Porta HTTPS padrão: 443**
- Rationale: porta padrão HTTPS; clientes não precisam especificar porta na URL
- Alternativa considerada: 8443 (evita privilégio de porta < 1024 no container). Rejeitada: exige que o usuário digite `:8443` e quebra a expectativa de URL limpa
- Mitigação para porta < 1024: rodar container com `cap_add: NET_BIND_SERVICE` ou mapear porta do host (ex: `443:443`)

**2. Formato do certificado: PFX suportado via duas fontes simultâneas (CERT_PATH e CERT_BASE64)**
- Rationale: Kestrel .NET suporta PFX nativamente; um único arquivo contém chave privada + certificado; senha opcional. O sistema suporta ambas as fontes ao mesmo tempo: `CERT_PATH` (caminho para arquivo PFX montado por volume) e `CERT_BASE64` (conteúdo base64 do PFX passado diretamente na variável de ambiente). Quando ambas estão definidas, `CERT_BASE64` tem prioridade. Isso dá ao operador flexibilidade total: usar arquivo montado via Docker volume ou passar o cert como string de ambiente.
- Alternativa considerada: PEM separado (cert + key). Rejeitada: exige duas variáveis de ambiente e manipulação de chave privada em texto, menos seguro e menos idiomático no .NET.
- Decisão: não é "uma ou outra" — ambas são implementadas e suportadas simultaneamente.

**3. Geração de certificado auto-assinado no entrypoint (runtime)**
- Rationale: cada container/instância recebe um cert único; não precisa rebuild de imagem para trocar cert
- Alternativa considerada: gerar no Dockerfile (build time). Rejeitada: cert fixo para todas as instâncias, exige rebuild para trocar

**4. Variáveis de ambiente customizadas (não usar `ASPNETCORE_Kestrel__...` nativas)**
- Rationale: nomes customizados dão controle explícito sobre geração vs importação, permitem validação de cert inválido antes de iniciar o Kestrel
- Alternativa considerada: usar `ASPNETCORE_Kestrel__Certificates__Default__Path` nativa. Rejeitada: não cobre o fluxo de geração automática nem validação prévia

**5. Bloqueio de HTTP configurável via DISABLE_HTTP**
- Rationale: o padrão é bloquear HTTP completamente (segurança máxima), mas a variável `DISABLE_HTTP=false` permite expor um listener HTTP interno quando o container está atrás de um load balancer de nuvem que termina TLS na borda (AWS ALB, Azure Container Apps, Google Cloud Run, Heroku). Nesse cenário o tráfego externo é HTTPS e o tráfego interno entre o load balancer e o container é HTTP.
- Alternativa considerada: bloqueio total sem exceção. Rejeitada: inviabilizaria o uso direto em provedores SaaS que encaminham HTTP interno para o container.
- Decisão: `DISABLE_HTTP=true` é o padrão (bloqueio total); `DISABLE_HTTP=false` ativa o listener HTTP para compatibilidade com proxies de nuvem.

## Risks / Trade-offs

[Certificado auto-assinado gera aviso de "não seguro" no navegador] → Mitigar: documentar que o usuário deve adicionar exceção ou importar cert válido via env var para produção

[Porta 443 requer privilégio no container Linux] → Mitigar: documentar `cap_add: NET_BIND_SERVICE` ou mapear porta do host

[Conteúdo base64 do PFX é sensível no .env] → Mitigar: alertar que `.env` não deve ser versionado; usar Docker secrets ou arquivo montado em produção

[WebSocket do PlexAutoSkip pode não funcionar com WSS se o cliente Plex não suporta] → Mitigar: documentar limitação conhecida do protocolo Plex Companion

[Provedores de nuvem que terminam TLS na borda (AWS ALB, Azure Container Apps, Google Cloud Run, Heroku) enviam HTTP interno para o container] → Mitigar: definir `DISABLE_HTTP=false` para permitir listener HTTP interno, ou usar um proxy reverso adicional (NGINX, Traefik) entre o load balancer e o container.

## Migration Plan

1. Aplicar mudanças em `Program.cs`, `docker-compose.yml`, `Dockerfile`, `.env`, `.env.example`
2. Rebuild da imagem: `docker compose build`
3. Reiniciar container: `docker compose up -d`
4. Acessar `https://localhost` (ou porta configurada) e adicionar exceção de certificado no navegador (modo dev)
5. **Se implantar atrás de load balancer de nuvem (AWS ALB, Azure Container Apps, Google Cloud Run, Heroku)**: definir `DISABLE_HTTP=false` para permitir listener HTTP interno, OU configurar proxy reverso adicional (NGINX, Traefik, etc.) entre o load balancer e o container
6. Rollback: restaurar `.env` e `docker-compose.yml` do git, desfazer `Program.cs`, rebuild

## Open Questions

Nenhuma. Todas as decisões em aberto foram resolvidas:
- WSS: o Plex Media Server suporta WSS nativamente (endpoint `wss://{ip}:32400/:/websockets/notifications`) e o PlexAutoSkipService (.NET) já lida com SSL via `sslopt`. A limitação conhecida é do lado do cliente Plex (iOS/Plex Web App podem não estabelecer WSS), documentada em Risks / Trade-offs.
- Certificado: ambas as fontes (`CERT_PATH` e `CERT_BASE64`) são suportadas simultaneamente, com prioridade para `CERT_BASE64` quando ambas estão definidas.
- HTTP atrás de proxy de nuvem: `DISABLE_HTTP=false` permite listener HTTP interno quando o container está atrás de um load balancer que termina TLS na borda.
