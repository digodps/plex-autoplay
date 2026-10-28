# Spec Delta

## Purpose

Define o comportamento do pipeline de integração contínua no GitHub Actions para a stack .NET/Vue: build da solution, execução dos testes e2e em container e unicidade do caminho de publicação da imagem.

## ADDED Requirements

### Requirement: Build da solution .NET no CI
O pipeline SHALL executar o build da solution .NET (`PlexAutoplay.slnx`) com cache de pacotes NuGet em todo push para a main e em todo pull request para a main.

#### Scenario: Build bem-sucedido
- **WHEN** o job de build roda sobre um commit que compila sem erros
- **THEN** o job termina com sucesso sem instalar nenhum runtime ou dependência Python

#### Scenario: Build quebrado em pull request
- **WHEN** um pull request introduz um erro de compilação
- **THEN** o job de build falha e bloqueia o merge do pull request

### Requirement: Executar testes e2e em container no CI
O pipeline SHALL executar a suíte e2e (Playwright) dentro do stack `docker-compose.test.yml` (aplicação + executor + plex-mock), sem instalar Playwright ou Node diretamente no runner.

#### Scenario: PR verde com e2e passando
- **WHEN** um pull request para a main é aberto e os testes e2e passam
- **THEN** o job de e2e termina com sucesso

#### Scenario: Falha de e2e bloqueia merge
- **WHEN** pelo menos um teste e2e falha em um pull request
- **THEN** o job de e2e falha, publica os artefatos de falha (screenshots, traces, relatório) como artefatos do run e bloqueia o merge

#### Scenario: Sem Plex real no CI
- **WHEN** o stack de teste sobe no CI
- **THEN** a aplicação se conecta a um Plex mockado servido dentro da rede de teste, sem usar segredos de Plex reais

### Requirement: Caminho único de publicação da imagem
O pipeline SHALL usar exclusivamente o workflow de publish de container para buildar e publicar a imagem no GHCR, sem job de publish duplicado no workflow de CI.

#### Scenario: Push na main publica a imagem
- **WHEN** um push para a main completa o workflow de publish
- **THEN** a imagem `ghcr.io/digodps/plex-autoplay` é atualizada com as tags `latest` e `sha`

#### Scenario: Sem publish a partir do CI
- **WHEN** o workflow de CI termina em qualquer branch
- **THEN** nenhuma imagem é publicada por ele no registry
