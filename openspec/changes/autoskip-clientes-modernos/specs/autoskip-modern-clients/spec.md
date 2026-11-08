# Spec Delta

## Purpose
Suporte a skip automático de intro/créditos em clientes Plex modernos que não expõem mais o Plex Companion ("Advertise as Player"), incluindo Plex Web, Plex Desktop (Windows/Mac/Linux) e Smart TVs (Samsung/LG/Vizio), mantendo compatibilidade com clientes legados via detecção dinâmica de capacidade.

## ADDED Requirements

### Requirement: Detecção de capacidade do cliente
O serviço de auto-skip SHALL detectar a capacidade de seek do cliente a partir dos atributos `product`, `platform` e `version` presentes no elemento `<Player>` retornado por `/status/sessions`, sem depender de lista hardcoded de versões quebradas.

#### Scenario: Cliente Web moderno detectado
- **WHEN** `product` é `Plex Web` (ou contém `Chrome`/`Firefox`/`Safari`/`Edge`) e `version` >= `4.83.2`
- **THEN** a estratégia é `ModernWeb` com `SupportsDirectConnection = false` e `DirectPort = 0`

#### Scenario: Cliente desktop moderno detectado
- **WHEN** `product` é `Plex for Windows`/`Plex for Mac`/`Plex for Linux` e `version` >= `1.46.1`
- **THEN** a estratégia é `ModernDesktop` com `SupportsDirectConnection = false` e `DirectPort = 0`

#### Scenario: Smart TV detectada
- **WHEN** `product` contém `Samsung`, `LG`, `Vizio`, `Hisense`, `Tizen` ou `webOS`
- **THEN** a estratégia é `SmartTV` com `SupportsPmsProxy = true` e `ExpectsNonXmlResponse = true`

#### Scenario: Cliente legado com companion
- **WHEN** `product` é `Plex for Android (TV)`, `Plex for Android (Mobile)`, `Plex for Apple TV` ou `Plex for Roku`
- **THEN** a estratégia é `LegacyCompanion` com `SupportsDirectConnection = true` e `DirectPort` mapeado (32500 ou 8324)

#### Scenario: Produto desconhecido
- **WHEN** `product` não corresponde a nenhuma categoria conhecida
- **THEN** a estratégia é `Unknown` com `SupportsPmsProxy = true` (best effort)

### Requirement: Seek via PMS proxy para clientes modernos
O serviço SHALL tentar seek via PMS proxy (`{PlexUrl}/player/playback/seekTo` com header `X-Plex-Target-Client-Identifier`) para todos os clientes com `SupportsPmsProxy = true`, incluindo clientes modernos sem companion.

#### Scenario: Seek Web via proxy
- **WHEN** cliente `ModernWeb` está tocando dentro de um marcador
- **THEN** o serviço envia GET `{PlexUrl}/player/playback/seekTo?offset={EndMs}&commandID={id}&type=video` com header `X-Plex-Target-Client-Identifier={machineId}`

#### Scenario: Cliente moderno com IP mas sem porta
- **WHEN** `ClientIp` está presente mas `DirectPort = 0`
- **THEN** o serviço NÃO tenta conexão direta e usa apenas PMS proxy

### Requirement: Tratamento de resposta não-XML
O serviço SHALL considerar um seek bem-sucedido quando o HTTP status for 2xx E o corpo for vazio, `OK`, ou um XML válido com `code="200"`, mesmo que não seja XML completo.

#### Scenario: Samsung retorna OK
- **WHEN** cliente `Plex for Samsung` responde `200` com corpo `OK`
- **THEN** o serviço registra sucesso e não tenta fallback adicional

#### Scenario: Resposta XML parcial
- **WHEN** cliente responde `200` com corpo `<?xml version="1.0"?><Response code="200" status="OK">`
- **THEN** o serviço registra sucesso

#### Scenario: Resposta de erro
- **WHEN** cliente responde `4xx` ou `5xx`
- **THEN** o serviço registra falha e tenta a próxima estratégia (se houver)

### Requirement: CommandID em todos os seeks
O serviço SHALL gerar um `commandID` único por tentativa de seek e incluí-lo em todas as requisições (direta e proxy).

#### Scenario: CommandID presente
- **WHEN** qualquer seek é enviado (direto ou proxy)
- **THEN** a URL contém parâmetro `commandID` não vazio

### Requirement: Compatibilidade retroativa com clientes legados
O serviço SHALL manter o caminho de conexão direta para clientes `LegacyCompanion` antes de tentar PMS proxy, preservando comportamento atual para Android TV/Mobile, Apple TV e Roku.

#### Scenario: Roku continua direto
- **WHEN** cliente `Plex for Roku` está tocando dentro de um marcador
- **THEN** o serviço tenta primeiro `http://{ClientIp}:8324/player/playback/seekTo?...` e só usa proxy se falhar

#### Scenario: Android TV continua direto
- **WHEN** cliente `Plex for Android (TV)` está tocando dentro de um marcador
- **THEN** o serviço tenta primeiro `http://{ClientIp}:32500/player/playback/seekTo?...` e só usa proxy se falhar

### Requirement: Remoção de lista hardcoded de versões quebradas
O serviço SHALL remover `IsBrokenClient` e `VersionCompare` como mecanismo de exclusão; a decisão de estratégia SHALL ser baseada em categoria de capacidade detectada.

#### Scenario: Log de cliente moderno
- **WHEN** um cliente `ModernWeb` está dentro de marcador
- **THEN** o serviço tenta PMS proxy imediatamente sem registrar "lacks Plex Companion support, skipping seek"