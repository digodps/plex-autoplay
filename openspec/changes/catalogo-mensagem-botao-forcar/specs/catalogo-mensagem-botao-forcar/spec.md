# Spec Delta

## ADDED Requirements

### Requirement: Mensagem de catálogo não processado

O sistema SHALL exibir uma mensagem informativa quando o usuário acessa uma biblioteca que ainda não teve o catálogo sincronizado (totalSize === 0 no endpoint `/api/series/{sectionKey}`).

#### Scenario: Catálogo não sincronizado

- **WHEN** o usuário navega para `/libraries/{key}/series` e `seriesTotalSize === 0` e não há erro de conexão
- **THEN** o sistema exibe uma mensagem explicando que o catálogo ainda não foi sincronizado automaticamente
- **AND** exibe um botão "Forçar sincronização" para disparar a sincronização manual

#### Scenario: Biblioteca vazia no Plex

- **WHEN** o usuário navega para uma biblioteca que existe no Plex mas não contém séries
- **THEN** a mensagem indica que a biblioteca está vazia (sem séries no Plex)

### Requirement: Botão forçar sincronização de catálogo

O sistema SHALL fornecer um botão "Forçar sincronização" que dispara `POST /api/sync/catalog` e exibe feedback visual durante e após a execução.

#### Scenario: Forçar sincronização com sucesso

- **WHEN** o usuário clica em "Forçar sincronização"
- **THEN** o botão entra em estado de carregamento
- **AND** a requisição `POST /api/sync/catalog` é enviada
- **AND** ao receber resposta de sucesso, exibe mensagem "Catálogo sincronizado: X bibliotecas, Y séries"
- **AND** recarrega a lista de séries automaticamente

#### Scenario: Forçar sincronização com erro

- **WHEN** a sincronização falha (erro de conexão, Plex indisponível, etc)
- **THEN** exibe mensagem de erro clara com sugestão de tentar novamente
- **AND** o botão retorna ao estado normal