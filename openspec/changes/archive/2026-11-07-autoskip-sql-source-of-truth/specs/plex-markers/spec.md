# Spec Delta

## MODIFIED Requirements

### Requirement: Persist markers in custom.json format
O sistema SHALL persistir marcadores detectados ou criados manualmente apenas em SQLite (tabela Markers), sem gravar custom.json, sem duplicar marcadores existentes salvo re-scan manual explícito.

#### Scenario: New marker written to SQLite
- **WHEN** um marcador é detectado para um episódio com rating key 12345
- **THEN** o SQLite contém uma entrada para rating key 12345 com start_ms, end_ms e kind

#### Scenario: Existing marker not duplicated
- **WHEN** o SQLite já contém um marcador para rating key 12345
- **THEN** o sistema não sobrescreve salvo que um re-scan manual seja disparado

#### Scenario: Marcador criado via API gravado no SQLite
- **WHEN** um usuário envia { rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 } via API
- **THEN** o sistema armazena o marcador em SQLite

#### Scenario: Marcador removido via API removido do SQLite
- **WHEN** um usuário remove um marcador por rating key e kind
- **THEN** o sistema remove o marcador do SQLite
