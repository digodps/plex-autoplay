# Spec Delta

## MODIFIED Requirements

### Requirement: Read custom.json for markers
O sistema SHALL ler os marcadores do SQLite (tabela Markers) para determinar os pontos de pular do autoskip.

#### Scenario: Markers loaded from SQLite
- **WHEN** o serviço de autoskip recebe uma mensagem de play via WebSocket para um episódio
- **THEN** o sistema consulta a tabela Markers do SQLite pelo ratingKey e retorna os marcadores de intro e créditos do episódio

#### Scenario: Marker updates reflected without restart
- **WHEN** um marcador é gravado ou atualizado no SQLite (via API, scan ou sync)
- **THEN** o próximo pedido de play do mesmo episódio retorna o marcador atualizado, sem exigir reinicialização nem watcher de arquivo
