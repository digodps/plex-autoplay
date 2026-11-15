# remover-controle-https Specification (Delta)

## Purpose
Remove todo o controle de HTTPS/TLS/certificados da aplicação `plex-autoplay`. A aplicação passa a servir apenas **HTTP** na porta 80 (interno do container). TLS/HTTPS torna-se responsabilidade exclusiva da infraestrutura do usuário (reverse proxy, load balancer cloud, tunnel, etc.).

Este delta **substitui** a spec `https-transporte` (que fica obsoleta/arquivada).

## Why
- **Simplicidade**: O código de certificado auto-assinado, CERT_PATH, CERT_BASE64, DISABLE_HTTP, KestrelSetup, ServerCertificateProvider adiciona ~300 linhas de complexidade.
- **Separação de responsabilidades**: TLS é preocupação de infraestrutura/rede, não de aplicação.
- **Flexibilidade**: Usuários em produção já usam reverse proxy (NGINX/Caddy/Traefik) ou cloud LB (ALB, Cloud Run, Azure Container Apps) que terminam TLS na borda.
- **Menos bugs**: Elimina falhas de handshake IPv6 TLS, certificados expirados, geração de cert auto-assinado, validação de PFX, etc.
- **Testes mais simples**: E2E roda em HTTP puro sem `ignoreHTTPSErrors` nem `curl -k`.

## ADDED Requirements

### Requirement: Servir apenas HTTP
O sistema SHALL expor endpoints apenas via HTTP na porta 80, sem qualquer listener HTTPS ou TLS.

#### Scenario: Porta HTTP aceita conexões
- **WHEN** um cliente se conecta na porta HTTP 80 do container
- **THEN** o sistema estabelece uma conexão HTTP simples (sem TLS)

#### Scenario: Nenhum listener HTTPS existe
- **WHEN** a aplicação inicia
- **THEN** não há configuração de certificado, não há listener HTTPS, não há WSS

### Requirement: Nenhum certificado gerenciado pela aplicação
O sistema SHALL NÃO gerar, carregar, validar ou usar certificados TLS.

#### Scenario: Inicialização sem certificado
- **WHEN** a aplicação inicia
- **THEN** não há código de certificado auto-assinado, não há leitura de `CERT_PATH`, `CERT_BASE64`, `CERT_PASSWORD`
- **THEN** a aplicação inicia imediatamente sem dependência de criptografia TLS

### Requirement: Variáveis de ambiente TLS removidas
O sistema SHALL NÃO ler nem usar as variáveis de ambiente relacionadas a TLS.

#### Scenario: Variáveis legadas ignoradas
- **WHEN** variáveis `CERT_PATH`, `CERT_BASE64`, `CERT_PASSWORD`, `HTTPS_PORT`, `DISABLE_HTTP`, `WS_PORT` estão definidas no ambiente
- **THEN** a aplicação ignora completamente essas variáveis (não falha, não usa, não loga erro)

### Requirement: Config endpoint sem campos TLS
O endpoint `GET /config` SHALL NÃO retornar campos relacionados a HTTPS/TLS.

#### Scenario: Resposta /config sem https_port e disable_http
- **WHEN** cliente requisita `GET /config`
- **THEN** resposta JSON não contém `https_port`, `disable_http`
- **THEN** resposta contém apenas variáveis relevantes para operação HTTP

### Requirement: Auth cookie sem flag Secure
O cookie de autenticação SHALL ter `Secure=false` (adequado para HTTP).

#### Scenario: Cookie em HTTP
- **WHEN** usuário faz login via HTTP
- **THEN** cookie `auth_token` é definido com `Secure=false`, `SameSite=Lax`, `HttpOnly=false`

### Requirement: Docker expõe apenas porta 80 HTTP
A imagem Docker SHALL expor apenas a porta 80 para tráfego HTTP.

#### Scenario: Container roda em HTTP
- **WHEN** container inicia
- **THEN** `EXPOSE 80` no Dockerfile
- **THEN** `ASPNETCORE_URLS=http://+:80` configurado
- **THEN** `docker-compose.yml` mapeia `8080:80` (host:container)

### Requirement: Scripts e testes usam HTTP
Scripts de inicialização, health checks e testes E2E SHALL usar HTTP sem flags TLS.

#### Scenario: start.sh health check HTTP
- **WHEN** `./scripts/start.sh` aguarda aplicação responder
- **THEN** usa `curl -s http://localhost:8080/login` (sem `-k`)
- **THEN** exibe link de login `http://localhost:8080/login`

#### Scenario: Testes E2E aceitação em HTTP
- **WHEN** testes de aceitação rodam (`acceptance-test.sh`)
- **THEN** `ACCEPTANCE_BASE_URL=http://localhost:8080`
- **THEN** Playwright configurado sem `ignoreHTTPSErrors`

### Requirement: WebSocket PlexAutoSkip em WS (não WSS)
O WebSocket do PlexAutoSkip SHALL operar em `ws://` (texto plano) na porta 8081 interno.

#### Scenario: WebSocket interno sem TLS
- **WHEN** cliente Plex conecta no WebSocket interno
- **THEN** conexão é `ws://` (não `wss://`)
- **THEN** porta 8081 não exposta no host; acessível apenas via rede Docker interna ou proxy reverso configurado pelo usuário

## REMOVED Requirements

### Requirement: Servir apenas HTTPS (de https-transporte)
O sistema SHALL expor endpoints apenas via HTTPS, sem qualquer listener HTTP.

#### Scenario: Porta HTTPS aceita conexões
- **WHEN** um cliente se conecta na porta HTTPS configurada
- **THEN** o sistema estabelece uma conexão TLS criptografada

#### Scenario: Porta HTTP rejeitada
- **WHEN** um cliente tenta se conectar em qualquer porta HTTP
- **THEN** o sistema não aceita a conexão (não há listener HTTP)

### Requirement: Certificado auto-assinado por padrão (de https-transporte)
O sistema SHALL gerar um certificado auto-assinado automaticamente quando nenhum certificado for fornecido via variável de ambiente.

#### Scenario: Nenhum certificado configurado
- **WHEN** nenhuma variável de certificado está definida
- **THEN** o sistema gera um certificado auto-assinado na inicialização e o usa para TLS

#### Scenario: Certificado válido para localhost
- **WHEN** o certificado auto-assinado é gerado
- **THEN** o certificado inclui `localhost` como nome de sujeito ou SAN

### Requirement: Importar certificado válido via caminho de arquivo (CERT_PATH) (de https-transporte)
O sistema SHALL aceitar um certificado válido fornecido pelo operador através da variável de ambiente `CERT_PATH`, apontando para um arquivo PFX no container.

#### Scenario: CERT_PATH definido com arquivo válido
- **WHEN** a variável `CERT_PATH` aponta para um arquivo PFX válido e acessível no container
- **THEN** o sistema usa esse certificado para TLS em vez do auto-assinado

#### Scenario: CERT_PATH definido com arquivo inexistente
- **WHEN** a variável `CERT_PATH` aponta para um caminho que não existe
- **THEN** o sistema falha na inicialização com mensagem clara indicando que o arquivo não foi encontrado

### Requirement: Importar certificado válido via string base64 (CERT_BASE64) (de https-transporte)
O sistema SHALL aceitar um certificado válido fornecido pelo operador através da variável de ambiente `CERT_BASE64`, contendo o conteúdo base64 de um arquivo PFX.

#### Scenario: CERT_BASE64 definido com conteúdo válido
- **WHEN** a variável `CERT_BASE64` contém o conteúdo base64 de um PFX válido
- **THEN** o sistema grava o conteúdo em um arquivo temporário seguro e usa esse certificado para TLS

#### Scenario: CERT_BASE64 definido com conteúdo inválido
- **WHEN** a variável `CERT_BASE64` contém conteúdo que não é base64 válido ou não é um PFX válido
- **THEN** o sistema falha na inicialização com mensagem clara indicando o problema

### Requirement: Certificado inválido ou expirado (de https-transporte)
O sistema SHALL rejeitar certificados inválidos, corrompidos ou expirados, independentemente da origem (CERT_PATH ou CERT_BASE64).

#### Scenario: Certificado fornecido é inválido
- **WHEN** o certificado fornecido via `CERT_PATH` ou `CERT_BASE64` está expirado, corrompido ou não é um PFX válido
- **THEN** o sistema falha na inicialização com mensagem clara indicando o problema e não inicia o servidor HTTPS

### Requirement: Prioridade entre fontes de certificado (de https-transporte)
O sistema SHALL definir uma ordem de prioridade clara quando múltiplas fontes de certificado estão configuradas.

#### Scenario: CERT_BASE64 tem prioridade sobre CERT_PATH
- **WHEN** ambas as variáveis `CERT_PATH` e `CERT_BASE64` estão definidas
- **THEN** o sistema usa o certificado de `CERT_BASE64` e ignora `CERT_PATH`

#### Scenario: Apenas CERT_PATH definido
- **WHEN** apenas `CERT_PATH` está definido e `CERT_BASE64` não está
- **THEN** o sistema usa o certificado do caminho apontado por `CERT_PATH`

#### Scenario: Apenas CERT_BASE64 definido
- **WHEN** apenas `CERT_BASE64` está definido e `CERT_PATH` não está
- **THEN** o sistema usa o certificado do conteúdo base64 de `CERT_BASE64`

### Requirement: WebSocket Secure (WSS) (de https-transporte)
O sistema SHALL expor o WebSocket do PlexAutoSkip apenas via WSS (WebSocket sobre TLS).

#### Scenario: Conexão WSS aceita
- **WHEN** um cliente Plex se conecta na porta WSS configurada
- **THEN** o sistema estabelece uma conexão WebSocket segura sobre TLS

#### Scenario: Conexão WS pura rejeitada
- **WHEN** um cliente tenta se conectar via WS (texto plano)
- **THEN** o sistema não aceita a conexão

### Requirement: Porta HTTPS configurável (de https-transporte)
O sistema SHALL permitir configurar a porta HTTPS via variável de ambiente, com padrão 443.

#### Scenario: Porta padrão 443
- **WHEN** nenhuma variável de porta HTTPS está definida
- **THEN** o sistema escuta HTTPS na porta 443

#### Scenario: Porta customizada
- **WHEN** a variável de porta HTTPS é definida como 8443
- **THEN** o sistema escuta HTTPS na porta 8443

### Requirement: Bloqueio de HTTP configurável via DISABLE_HTTP (de https-transporte)
O sistema SHALL permitir configurar se o listener HTTP deve ser totalmente desabilitado através da variável de ambiente `DISABLE_HTTP`.

#### Scenario: DISABLE_HTTP=true (padrão)
- **WHEN** a variável `DISABLE_HTTP` está definida como `true` ou não está definida
- **THEN** o sistema não expõe qualquer listener HTTP; apenas HTTPS e WSS estão disponíveis

#### Scenario: DISABLE_HTTP=false
- **WHEN** a variável `DISABLE_HTTP` está definida como `false`
- **THEN** o sistema expõe um listener HTTP adicional (porta configurável, padrão 8080) para uso quando o container está atrás de um load balancer que termina TLS externamente (ex: AWS ALB, Azure Container Apps, Google Cloud Run, Heroku)

## Non-Requirements (Fora de Escopo)
- **Não** implementar ACME/Let's Encrypt
- **Não** implementar geração de certificado sob demanda
- **Não** validar certificados de terceiros
- **Não** terminar TLS na aplicação
- **Não** expor porta 443 ou 8443
- **Não** manter compatibilidade com `DISABLE_HTTP=true` (HTTPS-only)

## Migration Notes (para operadores)
| Antes | Depois | Ação do Operador |
|---|---|---|
| `https://localhost:8443` | `http://localhost:8080` | Atualizar bookmarks/links |
| `CERT_PATH` / `CERT_BASE64` | — | Remover do `.env`; configurar TLS no proxy reverso |
| `DISABLE_HTTP=true` | — | Remover; proxy reverso termina TLS |
| Cookie `Secure=true` | `Secure=false` | Funciona automaticamente behind proxy com `X-Forwarded-Proto` |
| `docker-compose.yml` ports `8081` | `8080:80` | Atualizar port mapping se customizado |

## Related Specs
- **Obsoleta**: `https-transporte` (arquivada, mantida para histórico)
- **Substitui**: todos os requirements de `https-transporte`