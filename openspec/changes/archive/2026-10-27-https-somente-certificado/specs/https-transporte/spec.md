# Spec Delta

## Purpose

Define o comportamento de transporte seguro HTTPS-only do plex-autoplay: certificado auto-assinado por padrão em dev, importação de certificado válido via variável de ambiente, bloqueio completo de HTTP e WebSocket Secure (WSS) para o canal do PlexAutoSkip.

## ADDED Requirements

### Requirement: Servir apenas HTTPS
O sistema SHALL expor endpoints apenas via HTTPS, sem qualquer listener HTTP.

#### Scenario: Porta HTTPS aceita conexões
- **WHEN** um cliente se conecta na porta HTTPS configurada
- **THEN** o sistema estabelece uma conexão TLS criptografada

#### Scenario: Porta HTTP rejeitada
- **WHEN** um cliente tenta se conectar em qualquer porta HTTP
- **THEN** o sistema não aceita a conexão (não há listener HTTP)

### Requirement: Certificado auto-assinado por padrão
O sistema SHALL gerar um certificado auto-assinado automaticamente quando nenhum certificado for fornecido via variável de ambiente.

#### Scenario: Nenhum certificado configurado
- **WHEN** nenhuma variável de certificado está definida
- **THEN** o sistema gera um certificado auto-assinado na inicialização e o usa para TLS

#### Scenario: Certificado válido para localhost
- **WHEN** o certificado auto-assinado é gerado
- **THEN** o certificado inclui `localhost` como nome de sujeito ou SAN

### Requirement: Importar certificado válido via caminho de arquivo (CERT_PATH)
O sistema SHALL aceitar um certificado válido fornecido pelo operador através da variável de ambiente `CERT_PATH`, apontando para um arquivo PFX no container.

#### Scenario: CERT_PATH definido com arquivo válido
- **WHEN** a variável `CERT_PATH` aponta para um arquivo PFX válido e acessível no container
- **THEN** o sistema usa esse certificado para TLS em vez do auto-assinado

#### Scenario: CERT_PATH definido com arquivo inexistente
- **WHEN** a variável `CERT_PATH` aponta para um caminho que não existe
- **THEN** o sistema falha na inicialização com mensagem clara indicando que o arquivo não foi encontrado

### Requirement: Importar certificado válido via string base64 (CERT_BASE64)
O sistema SHALL aceitar um certificado válido fornecido pelo operador através da variável de ambiente `CERT_BASE64`, contendo o conteúdo base64 de um arquivo PFX.

#### Scenario: CERT_BASE64 definido com conteúdo válido
- **WHEN** a variável `CERT_BASE64` contém o conteúdo base64 de um PFX válido
- **THEN** o sistema grava o conteúdo em um arquivo temporário seguro e usa esse certificado para TLS

#### Scenario: CERT_BASE64 definido com conteúdo inválido
- **WHEN** a variável `CERT_BASE64` contém conteúdo que não é base64 válido ou não é um PFX válido
- **THEN** o sistema falha na inicialização com mensagem clara indicando o problema

### Requirement: Certificado inválido ou expirado
O sistema SHALL rejeitar certificados inválidos, corrompidos ou expirados, independentemente da origem (CERT_PATH ou CERT_BASE64).

#### Scenario: Certificado fornecido é inválido
- **WHEN** o certificado fornecido via `CERT_PATH` ou `CERT_BASE64` está expirado, corrompido ou não é um PFX válido
- **THEN** o sistema falha na inicialização com mensagem clara indicando o problema e não inicia o servidor HTTPS

### Requirement: Prioridade entre fontes de certificado
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

### Requirement: WebSocket Secure (WSS)
O sistema SHALL expor o WebSocket do PlexAutoSkip apenas via WSS (WebSocket sobre TLS).

#### Scenario: Conexão WSS aceita
- **WHEN** um cliente Plex se conecta na porta WSS configurada
- **THEN** o sistema estabelece uma conexão WebSocket segura sobre TLS

#### Scenario: Conexão WS pura rejeitada
- **WHEN** um cliente tenta se conectar via WS (texto plano)
- **THEN** o sistema não aceita a conexão

### Requirement: Porta HTTPS configurável
O sistema SHALL permitir configurar a porta HTTPS via variável de ambiente, com padrão 443.

#### Scenario: Porta padrão 443
- **WHEN** nenhuma variável de porta HTTPS está definida
- **THEN** o sistema escuta HTTPS na porta 443

#### Scenario: Porta customizada
- **WHEN** a variável de porta HTTPS é definida como 8443
- **THEN** o sistema escuta HTTPS na porta 8443

### Requirement: Bloqueio de HTTP configurável via DISABLE_HTTP
O sistema SHALL permitir configurar se o listener HTTP deve ser totalmente desabilitado através da variável de ambiente `DISABLE_HTTP`.

#### Scenario: DISABLE_HTTP=true (padrão)
- **WHEN** a variável `DISABLE_HTTP` está definida como `true` ou não está definida
- **THEN** o sistema não expõe qualquer listener HTTP; apenas HTTPS e WSS estão disponíveis

#### Scenario: DISABLE_HTTP=false
- **WHEN** a variável `DISABLE_HTTP` está definida como `false`
- **THEN** o sistema expõe um listener HTTP adicional (porta configurável, padrão 8080) para uso quando o container está atrás de um load balancer que termina TLS externamente (ex: AWS ALB, Azure Container Apps, Google Cloud Run, Heroku)
