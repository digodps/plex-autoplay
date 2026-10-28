# Tasks

## 1. Dependências e configuração base

- [x] 1.1 Adicionar pacotes NuGet OpenTelemetry, OpenTelemetry.Exporter.Console, OpenTelemetry.Exporter.Otlp, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.AspNetCore ao projeto Infrastructure e Web e verificar que `dotnet build` segue com 0 erros
- [x] 1.2 Adicionar variáveis de ambiente `OTEL_EXPORTER`, `OTEL_ENDPOINT`, `OTEL_SERVICE_NAME` ao `docker-compose.yml` e `.env.example` e verificar que o container inicia com as variáveis carregadas
- [x] 1.3 Criar serviço de configuração de OpenTelemetry (`OpenTelemetryService`) no Infrastructure que configura traces, métricas e logs com base nas variáveis de ambiente e verificar que o serviço se registra no DI container

## 2. Correlation ID e middleware

- [x] 2.1 Implementar middleware de correlation ID que gera ou preserva o `X-Correlation-ID` e injeta no `Activity.Current` e verificar que requisições sem o cabeçalho recebem um ID gerado no cabeçalho de resposta
- [x] 2.2 Registrar o middleware de correlation ID no pipeline do ASP.NET Core antes do roteamento e verificar que todas as requisições HTTP passam por ele

## 3. Traces distribuídos

- [x] 3.1 Configurar `ActivitySource` para chamadas externas ao Plex Media Server e instrumentar o `PlexApiClient` para criar spans de saída e verificar que spans são criados com nome, duração e status
- [x] 3.2 Instrumentar execução do ffprobe no `FfmpegMarkerDetector` para criar spans de detecção de marcadores e verificar que spans incluem duração e resultado da detecção
- [x] 3.3 Verificar que o correlation ID da requisição entrante é propagado para os spans de chamada externa

## 4. Métricas Prometheus

- [x] 4.1 Configurar métricas de duração de requisição HTTP (histograma agrupado por método, caminho e status) e verificar que a métrica é exposta no endpoint `/metrics`
- [x] 4.2 Configurar contador de erros de chamada externa agrupado por serviço externo e tipo de erro e verificar que o contador incrementa em falhas ao Plex
- [x] 4.3 Configurar contador de scans realizados e duração média de scans (histograma) e verificar que a métrica é atualizada após cada scan de biblioteca

## 5. Logs estruturados

- [x] 5.1 Configurar `OpenTelemetry.Logs` para emitir logs em formato JSON com `correlation_id`, timestamp, nível, mensagem e contexto da requisição e verificar que os logs incluem o correlation ID
- [x] 5.2 Vincular logs de chamada externa ao Plex com campos `external_service` e `duration_ms` e verificar que esses campos aparecem nos logs

## 6. Dashboard de métricas básico

- [x] 6.1 Implementar endpoint `/metrics` que expõe métricas no formato Prometheus e verificar que retorna código 200 com dados de tempo médio de resposta e taxa de erro
- [x] 6.2 Adicionar endpoint de resumo de observabilidade (contagem de scans, duração média, taxa de erro) e verificar que retorna dados consistentes com as métricas coletadas

## 7. Integração e verificação

- [x] 7.1 Rodar o container com `OTEL_EXPORTER=console` e verificar que traces e métricas aparecem nos logs do container
- [x] 7.2 Executar um scan de biblioteca e verificar que o trace distribuído inclui span da chamada ao Plex e span do ffprobe com o mesmo correlation ID
- [x] 7.3 Verificar que o endpoint `/metrics` retorna métricas Prometheus válidas após gerar tráfego na API

## Workflow follow-up

- Archive a mudança `implementar-telemetria-ponta-a-ponta` após revisão dos requisitos de telemetria.
- Verificar que o archive foi concluído com sucesso.
