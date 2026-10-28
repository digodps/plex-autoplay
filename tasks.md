# Tasks

## 1. Infraestrutura base (escrita em custom.json, CSRF, variáveis de ambiente)

- [ ] 1.1 Criar CustomJsonWriterService (ler, reescrever e gravar atomicamente custom.json no formato PlexAutoSkip) e verificar que o arquivo é gerado corretamente com entradas keyed por rating key
- [ ] 1.2 Adicionar middleware de CSRF (Antiforgery do ASP.NET Core) em rotas POST/PUT/DELETE sob /api/ e verificar que uma requisição mutante sem token retorna 400/403
- [ ] 1.3 Consumir variáveis de ambiente no código .NET: WS_PORT, LOG_LEVEL, ALLOWED_HOSTS, OTEL_EXPORTER, OTEL_ENDPOINT, MEDIA_ROOT, CUSTOM_JSON_PATH e verificar que cada uma afeta o comportamento correspondente (sem leitura de app-config.json)

## 2. Scanner real de bibliotecas Plex

- [ ] 2.1 Implementar listagem de episódios de uma biblioteca de séries (GET de episódios no Plex) e verificar que retorna rating key, título, temporada, episódio e MediaPart.file
- [ ] 2.2 Implementar resolução de caminho local do episódio (MediaPart.file + MEDIA_ROOT) e verificar que o caminho local é montado corretamente
- [ ] 2.3 Integrar FfmpegMarkerDetector ao fluxo de scan (rodar ffprobe em episódios sem marcadores) e verificar que marcadores intro/créditos são detectados
- [ ] 2.4 Gravar marcadores detectados em SQLite e reescrever custom.json via CustomJsonWriterService, sem duplicar existentes; verificar leitura posterior via GET /api/markers e inspeção do arquivo custom.json

## 3. SyncBackgroundService real

- [ ] 3.1 Substituir o stub do SyncBackgroundService por varredura periódica real que orquestra o scanner (intervalo via SYNC_INTERVAL_MINUTES, primeira varredura após 5 minutos) e verificar que o serviço executa varreduras agendadas
- [ ] 3.2 Adicionar suporte a cancelamento de varredura em execução via POST /api/sync/cancel e verificar que o status muda para cancelled

## 4. PlexAutoSeek WebSocket (seek real ao player)

- [ ] 4.1 Reescrever o endpoint WebSocket /ws (porta 8081, WSS) para interpretar mensagens de controle do player (rating key + estado de reprodução) e verificar que a conexão WSS é aceita e WS puro é rejeitado
- [ ] 4.2 Implementar envio de comando de seek para pular intro e créditos com base nos marcadores de custom.json e verificar que o player recebe o comando de seek nos pontos corretos

## 5. Endpoints ausentes

- [ ] 5.1 Implementar GET /api/status (último job + métricas de processo: CPU, RAM, rede, disco) e verificar que retorna estado do último scan e métricas de processo
- [ ] 5.2 Implementar GET/POST /config (leitura/validação de config via variáveis de ambiente, sem gravar app-config.json) e verificar que retorna os valores atuais
- [ ] 5.3 Implementar GET /library/{section_key} (listar episódios de uma seção com caminho de mídia resolvido) e verificar que retorna episódios da biblioteca solicitada

## 6. Observabilidade (métricas Prometheus reais)

- [ ] 6.1 Configurar OpenTelemetry Prometheus exporter no OpenTelemetryService (respeitando OTEL_EXPORTER e OTEL_ENDPOINT) e verificar que /metrics retorna métricas reais (não hardcoded)
- [ ] 6.2 Verificar que traces incluem spans de chamada ao Plex e de execução do ffprobe, e que logs JSON incluem correlation_id

## 7. Testes e documentação

- [ ] 7.1 Atualizar testes E2E (Playwright) para cobrir scanner real, gravação de custom.json, WebSocket seek e novos endpoints; verificar que os testes passam contra o container
- [ ] 7.2 Atualizar README: remover contradição com spec config-env (app-config.json não é lido), corrigir ws:// para wss:// na porta 8081, documentar CSRF e novos endpoints; verificar que o README está consistente com o código

## Workflow follow-up

- Validar a mudança com `openspec validate`.
- Archivar a change após revisão dos requisitos.
