# Spec Delta

## MODIFIED Requirements

### Requirement: Protótipo para GitHub Actions com Plex mockado
O sistema SHALL ter um workflow do GitHub Actions que roda o mesmo executor de testes e2e, com o Plex Media Server mockado como serviço dentro do stack de teste (docker-compose.test.yml), acessível pela rede do Compose, sem depender de um Plex real nem de service containers do runner.

#### Scenario: CI com Plex mockado
- **WHEN** o workflow do GitHub Actions é disparado
- **THEN** o executor roda com um serviço Plex mock respondendo às rotas consumidas pelos testes e2e, alcançável pelo mesmo endereço usado pela aplicação no stack de teste

#### Scenario: Execução local e no CI com o mesmo stack
- **WHEN** o stack de teste é executado no ambiente local (Podman rootless) ou no GitHub Actions (Docker)
- **THEN** o endereço da aplicação é resolvido a partir de variável de ambiente (`.dns.podman` apenas no local), sem edição de arquivos para trocar de ambiente
