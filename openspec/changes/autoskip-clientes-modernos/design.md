# Design

## Context

O `PlexAutoSkipBackgroundService` faz polling em `/status/sessions` do Plex Media Server a cada 2s. Quando detecta episódio em reprodução dentro de marcador de intro/créditos, tenta dar seek no player.

Estratégias atuais (em ordem):
1. **Direto no cliente** — HTTP GET `http://{ClientIp}:{ClientPort}/player/playback/seekTo?...` (requer "Advertise as Player")
2. **Proxy via PMS** — HTTP GET `{PlexUrl}/player/playback/seekTo?...` com header `X-Plex-Target-Client-Identifier`

Problema: Plex removeu "Advertise as Player" do Web (4.83.2+) e Desktop (1.46.1+). Clientes Smart TV (Samsung, LG, Vizio) têm comportamento inconsistente.

Pesquisa no `plexapi` (python-plexapi) revela:
- `sendCommand('playback/seekTo', offset=...)` funciona para clientes modernos
- Alguns clientes retornam respostas não-XML: `b'OK'`, `b'<?xml version="1.0"?><Response code="200" status="OK">`
- Workaround no `plexapi` trata: `Plexamp`, `Plex for Android (TV)`, `Plex for Android (Mobile)`, `Plex for Samsung`

## Goals / Non-Goals

**Goals:**
- Seek funcionar no Plex Web (Chrome, Edge, Firefox, Safari)
- Seek funcionar no Plex Desktop moderno (Windows, Mac, Linux)
- Seek funcionar em Samsung TV / LG / Vizio / outras Smart TVs
- Manter compatibilidade com Android TV/Mobile, Apple TV, Roku
- Zero configuração manual — detecção automática via `/status/sessions`

**Non-Goals:**
- Não implementar WebSocket client-side (players Plex oficiais não conectam ao nosso `/ws`)
- Não alterar contratos de API do `IPlexAutoSkipService`
- Não mexer no frontend Vue
- Não implementar descoberta GDM/Bonjour (já feito pelo PMS)

## Decisions

### 1. Detecção de cliente moderno via `Product` + `Platform` + `Version`

Em `ParsePlayingSession`, extrair `product`, `platform`, `version`, `device`, `model` do XML `<Player>`.

Classificar em **capacidade**:
| Categoria | Produtos | Estratégia |
|-----------|----------|------------|
| `LegacyCompanion` | Android TV, Android Mobile, Apple TV, Roku | Direto (porta conhecida) → PMS proxy |
| `ModernWeb` | Plex Web, Plex for Chrome, Plex for Firefox, Plex for Safari, Plex for Edge | Apenas PMS proxy (porta 0) |
| `ModernDesktop` | Plex for Windows, Plex for Mac, Plex for Linux (versão >= 1.46.1) | Apenas PMS proxy |
| `SmartTV` | Plex for Samsung, Plex for LG, Plex for Vizio, Plex for Hisense, Plex for Tizen, Plex for webOS | PMS proxy + tratamento resposta não-XML |
| `Unknown` | Qualquer outro | PMS proxy (best effort) |

### 2. `GetClientPort` → `GetClientStrategy(product, platform, version)`

Retorna `ClientSeekStrategy`:
```csharp
record ClientSeekStrategy(
    bool SupportsDirectConnection,
    int DirectPort,           // 0 se não suporta
    bool SupportsPmsProxy,    // sempre true para LAN
    bool ExpectsNonXmlResponse // Samsung, Android, Plexamp
);
```

### 3. `SeekToEndOfMarkerAsync` unificado

Ordem de tentativa:
1. Se `SupportsDirectConnection` E `DirectPort > 0` E IP conhecido → **Direto** (com tratamento não-XML)
2. **PMS Proxy** com `X-Plex-Target-Client-Identifier` (sempre tentar, trata não-XML)
3. Log warning se ambos falharem

Tratamento resposta não-XML: considerar sucesso se `HttpStatusCode == 200` E (body contém "OK" OU body vazio OU XML válido com code 200).

### 4. `commandID` obrigatório

Sempre gerar `commandID` único (GUID curto) em **todas** requisições seek (direto e proxy). Plex exige para deduplicação.

### 5. Remover `IsBrokenClient`

Substituir por `ClientSeekStrategy` baseado em capacidades detectadas, não versão hardcoded.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| PMS proxy falha para clientes remotos (não LAN) | Requisito já existente: apenas sessões LAN; documentar |
| Cliente não aparece em `/status/sessions` com `machineIdentifier` | Fallback: tentar direto se IP+porta conhecidos |
| Smart TV não responde ao proxy PMS | Tratar 200 + body não-XML como sucesso; log detalhado |
| Mudança de product name em nova versão Plex | Mapeamento por prefixo/contém ("Samsung", "LG", "webOS", "Tizen") não exato |

## Migration Plan

1. Criar `PlexModernClientDetector.cs` (nova classe, single responsibility)
2. Refatorar `GetClientPort` → `GetClientStrategy` em `PlexAutoSkipBackgroundService`
3. Refatorar `SeekToEndOfMarkerAsync` para usar strategy
4. Adicionar tratamento resposta não-XML em ambas rotas (direto e proxy)
5. Remover `IsBrokenClient` e `VersionCompare`
6. Testes unitários para detector e strategies
7. Build + QA (Playwright não necessário — backend only)

## Open Questions

- Deveríamos expor `ClientSeekStrategy` via DI para testabilidade? **Sim**, `IClientSeekStrategyProvider`.
- Precisamos de config para forçar estratégia? **Não** por enquanto — auto-detecção cobre 99%.