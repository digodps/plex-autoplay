# plex-api-client Specification

## Purpose

Define o contrato de transporte do cliente HTTP do Plex: URL absoluta derivada da configuração e handler restrito a credenciais HTTP.

## ADDED Requirements

### Requirement: Requisições com URL absoluta

O sistema SHALL montar a URL absoluta de cada chamada ao Plex concatenando `PLEX_URL` (sem barra final) com o caminho do recurso, preservando qualquer prefixo de caminho configurado, em todos os métodos do cliente.

#### Scenario: URL sem prefixo
- **WHEN** `PLEX_URL=http://192.168.1.5:32400`
- **THEN** a chamada de bibliotecas usa `http://192.168.1.5:32400/library/sections`

#### Scenario: URL com prefixo de caminho
- **WHEN** `PLEX_URL=https://exemplo.com/plex`
- **THEN** bibliotecas, episódios, séries e episódios de série usam `https://exemplo.com/plex/<recurso>`

#### Scenario: Requisição relativa
- **WHEN** uma requisição chega ao handler do Plex sem URL absoluta
- **THEN** o sistema falha explicitamente com erro de programação em vez de reescrever a URL

### Requirement: Handler injeta credenciais

O sistema SHALL usar o handler HTTP apenas para adicionar o header `X-Plex-Token` (quando ausente) e `Accept: application/json` (quando ausente), relendo a configuração a cada requisição.

#### Scenario: Token alterado em runtime
- **WHEN** o operador troca o token em Ajustes
- **THEN** a próxima chamada ao Plex usa o novo token sem reiniciar o container

#### Scenario: Header explícito não sobrescrito
- **WHEN** a requisição já possui `X-Plex-Token` ou `Accept`
- **THEN** o handler mantém o valor enviado

#### Scenario: Validação de identidade usa XML
- **WHEN** o setup ou a validação de identidade chama `GET /identity`
- **THEN** a requisição envia `Accept: application/xml`, compatível com o parser `XDocument`
