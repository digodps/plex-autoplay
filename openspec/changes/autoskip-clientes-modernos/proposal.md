# Proposal

## Why

O Plex removeu o recurso "Advertise as Player" / Plex Companion do **Plex Web** (versão 4.83.2+) e dos clientes desktop **Windows/Mac/Linux** (versão 1.46.1+). Isso quebra o `PlexAutoSkipBackgroundService` atual que depende de conexão direta ao cliente (`/player/playback/seekTo` na porta do companion) ou proxy via PMS com `X-Plex-Target-Client-Identifier`.

Clientes testados que **não funcionam** hoje:
- **Plex Web** (Chrome/Edge/Firefox/Safari) — sem companion
- **Plex para Windows/Mac/Linux** (desktop moderno) — sem companion
- **Samsung TV (Tizen)** — comportamento inconsistente, resposta não-XML
- **iOS** — pode funcionar via companion na porta 32500 mas falha silenciosamente em alguns casos

Clientes que **ainda funcionam** (companion ativo):
- Android TV / Mobile (porta 32500)
- Apple TV (porta 32500)
- Roku (porta 8324)

O código atual (`IsBrokenClient`) apenas **ignora** clientes conhecidos como quebrados, retornando sem tentar seek. Precisamos implementar estratégias modernas que funcionem **sem** companion.

## What Changes

- **Atualizar `GetClientPort`** — adicionar mapeamento para clientes modernos: `Plex for Samsung`, `Plex for LG`, `Plex for Vizio`, `Plex Web (Chrome)`, etc. Porta 0 = usar apenas PMS proxy.
- **Melhorar `SeekToEndOfMarkerAsync`** — priorizar **PMS proxy** para clientes sem companion conhecido; tratar respostas não-XML (OK, `<Response code="200" status="OK">`) como sucesso.
- **Adicionar `PlexModernClientDetector`** — detectar capacidades do cliente via `/status/sessions` (product, platform, version, capabilities) e escolher estratégia.
- **Suporte a WebSocket (opcional futuro)** — endpoint `/ws` já existe em `AutoSkipWebSocketEndpoints`; documentar como clientes customizados poderiam usá-lo.
- **Remover/Descontinuar `IsBrokenClient`** — substituir por lógica baseada em capacidades detectadas, não lista hardcoded de versões.

## Capabilities

### New Capabilities

- `autoskip-modern-clients`: Suporte a seek em clientes Plex modernos (Web, Desktop, Smart TVs) sem depender de "Advertise as Player".
- `plex-client-capability-detection`: Detecção dinâmica de capacidades do player via API de sessões do Plex.

### Modified Capabilities

- `plex-autoskip-background`: Lógica de seek atualizada para usar PMS proxy como padrão para clientes modernos; fallback robusto.
- `plex-autoskip-websocket`: Endpoint WebSocket existente documentado para uso futuro por players customizados.

## Impact

- **Backend (.NET)**: `PlexAutoSkipBackgroundService.cs` — `GetClientPort`, `SeekToEndOfMarkerAsync`, nova classe `PlexModernClientDetector`.
- **Testes**: Adicionar testes unitários para detecção de clientes modernos e estratégias de seek.
- **Sem breaking changes** — clientes legados (Android, Roku, Apple TV) continuam funcionando igual.
- **Configuração**: Nenhuma nova config obrigatória; detecção automática via sessões Plex.