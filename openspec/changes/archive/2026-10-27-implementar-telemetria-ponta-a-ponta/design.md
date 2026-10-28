# Design

## Context

O projeto `plex-autoplay` já foi reescrito em .NET 10 com arquitetura DDD/Clean Architecture, Vue.js SPA servida pelo ASP.NET Core, Dockerfile com ffmpeg e docker-compose com volume persistente. Os specs principais (plex-auth, plex-libraries, plex-markers, plex-sync, plex-auto-skip, plex-images) já foram sincronizados para `openspec/specs/`. Atualmente não existe nenhuma instrumentação de observabilidade: não há traces, métricas nem logs estruturados com correlation ID. A mudança anterior (`replace`) implementou toda a aplicação; a mudança `teste-integracao-e2e-playwright` adicionou a camada de validação e2e. Esta mudança adiciona telemetria de ponta a ponta sobre a stack existente.

## Goals / Non-Goals

**Goals:**
- Rastrear requisições do frontend Vue até o backend .NET com correlation ID
- Exportar traces distribuídos para chamadas externas (Plex Media Server, ffprobe)
- Expor métricas de Prometheus (duração, taxa de erro, duração de scans)
- Emitir logs estruturados em JSON com correlation ID
- Configurar exportador via variáveis de ambiente (console para dev, OTLP para produção)

**Non-Goals:**
- Dashboard de métricas completo com gráficos (apenas endpoint básico de métricas)
- Armazenamento de traces a longo prazo (depende do exportador configurado)
- Alertas automáticos ou notificações baseadas em métricas
- Rastreamento de métricas de negócio customizadas além das listadas nos specs

## Decisions

**1. OpenTelemetry .NET SDK como instrumento de observabilidade**
- Rationale: padrão da indústria, integração nativa com ASP.NET Core, suporte a traces e métricas em uma única biblioteca, exportadores flexíveis (console, OTLP, Prometheus)
- Alternativas consideradas: Serilog + Elastic (rejeitado: focado em logs, não traces distribuídos), OpenCensus (rejeitado: em descontinuação em favor do OpenTelemetry)

**2. Middleware de correlation ID na camada Web**
- Rationale: gera ou preserva o correlation ID em cada requisição e injeta no `Activity.Current` do OpenTelemetry, garantindo que todos os spans downstream herdem o mesmo ID
- Alternativas considerada: injetar correlation ID em cada controller individualmente (rejeitado: não escala, esquecível)

**3. Instrumentação automática do HttpClient para chamadas ao Plex**
- Rationale: `OpenTelemetry.Instrumentation.Http` captura automaticamente spans de saída com duração, método e status, sem alterar o código do `PlexApiClient`
- Alternativas considerada: criar spans manualmente em cada chamada (rejeitado: mais código, maior risco de esquecer spans)

**4. Métricas expostas via endpoint `/metrics` (Prometheus) no próprio container**
- Rationale: simples, não requer infraestrutura externa para modo dev, compatível com scrapers Prometheus em produção
- Alternativas considerada: enviar diretamente para um backend de métricas (rejeitado: adiciona dependência externa obrigatória)

**5. Exportador condicional por variável de ambiente**
- Rationale: modo dev usa console (sem infraestrutura), produção usa OTLP (envia para coletor)
- Alternativas considerada: exportador fixo (rejeitado: menos flexível para diferentes ambientes)

## Risks / Trade-offs

[Sobrecarga de desempenho devido à instrumentação de todas as requisições] → Mitigar configurando sampling de traces (ex: 10% em produção) e desabilitando instrumentação desnecessária

[Correlation ID vazado para clientes externos] → Mitigar validando formato do cabeçalho de entrada e não expondo IDs internos em respostas públicas

[Métricas de Prometheus expostas sem autenticação] → Mitigar expondo o endpoint `/metrics` apenas em rede interna ou protegendo com auth quando houver múltiplos usuários

[Logs estruturados podem crescer rapidamente em disco] → Mitigar com rotação de logs e nível configurável via variável de ambiente

## Open Questions

- Qual taxa de sampling de traces usar em produção (ex: 10%, 50%, 100%)?
- O endpoint de métricas deve ser protegido por autenticação JWT ou ficar disponível em rede interna?
