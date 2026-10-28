# Proposal

## Why

O `plex-autoplay` expõe apenas HTTP (portas 8080 e 8081) no Kestrel, sem suporte a HTTPS, sem certificado e sem bloqueio de texto plano. Como o sistema manipula credenciais (Plex token, senha admin, JWT), o tráfego sem criptografia expõe segredos em rede local. A mudança torna o acesso HTTPS obrigatório, gera um certificado auto-assinado por padrão para modo dev, permite importar certificado válido via variável de ambiente e desativa qualquer listener HTTP.

## What Changes

- Configurar Kestrel para ouvir apenas em HTTPS (porta configurável, padrão 443) e WebSocket Secure (WSS)
- Gerar certificado auto-assinado automaticamente quando nenhum for fornecido (modo dev)
- Permitir importar certificado válido via variável de ambiente (caminho para arquivo PFX/PEM ou conteúdo base64)
- Desativar listener HTTP completamente: nenhuma porta escuta texto plano
- Manter `UseHttpsRedirection()` inofensivo (não há HTTP para onde redirecionar)
- Ajustar `docker-compose.yml`, `Dockerfile`, `launchSettings.json`, `.env` e `.env.example` para as novas variáveis de certificado e porta HTTPS
- **BREAKING**: porta 8080 não serve mais HTTP; clientes devem usar HTTPS (ex: `https://localhost:443`)

## Capabilities

### New Capabilities

- `https-transporte`: Transporte seguro HTTPS-only com gerenciamento de certificado (auto-assinado dev / importado via env var), bloqueio de HTTP e WSS para WebSocket.

### Modified Capabilities

Nenhum. Os specs existentes continuam inalterados em nível de requisito.

## Impact

- `src/PlexAutoplay.Web/Program.cs` — configuração de Kestrel com certificado HTTPS, desabilitação de HTTP
- `docker-compose.yml` — novas variáveis de ambiente, porta HTTPS exposta, volume de certificados opcional
- `Dockerfile` — instalação de `openssl` para geração de cert auto-assinado, ajustes de EXPOSE e ASPNETCORE_URLS
- `.env` / `.env.example` — variáveis de certificado e porta HTTPS
- `src/PlexAutoplay.Web/Properties/launchSettings.json` — perfis de dev para HTTPS
