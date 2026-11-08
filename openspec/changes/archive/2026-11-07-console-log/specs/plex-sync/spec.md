# Spec Delta

## MODIFIED Requirements

### Requirement: Scan progress reporting
O sistema SHALL expor o status e progresso da varredura através de um endpoint de API e um stream SSE, incluindo linhas de log detalhadas por evento.

#### Scenario: Scan status query
- **WHEN** a usuário requisita GET /api/sync/status
- **THEN** o sistema retorna o estado atual da varredura (queued, running, completed, failed) e contagens de processados/pulados/erros

#### Scenario: SSE progress stream with log lines
- **WHEN** a usuário conecta a GET /api/sync/log/stream durante uma varredura
- **THEN** o sistema envia eventos server-sent com contagens de progresso e linhas de log (timestamp, mensagem e nível de cada evento da varredura)

#### Scenario: Log line includes error correction hint
- **WHEN** um evento da varredura resulta em erro
- **THEN** a linha de log inclui uma sugestão de ação para o usuário corrigir o problema

#### Scenario: User clears the console display
- **WHEN** o usuário clica no botão "Limpar console" no frontend
- **THEN** as linhas de log são ocultadas da exibição, enquanto o histórico do job no backend é mantido
