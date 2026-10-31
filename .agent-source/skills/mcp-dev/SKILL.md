---
name: mcp-dev
description: Servidor MCP dedicado a ferramentas de desenvolvimento (melhores práticas, mapa do código, validação de commits).
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Servidor MCP `mcp-dev`

Esta skill fornece acesso às ferramentas de desenvolvimento via protocolo Model Context Protocol (MCP). As ferramentas são úteis para agentes de backend, frontend e testador que precisam de contexto sobre a base de código.

## Ferramentas Disponíveis

### `get_best_practices` / resource `plex://guidelines`

Retorna as guidelines de desenvolvimento e o checklist Definition of Done (DoD).

**Uso:** Consulte antes de codar ou revisar para garantir conformidade com as regras do projeto.

### `get_code_map` / resource `plex://codemap`

Retorna o mapa estrutural da base de código, incluindo:

- **Backend**: `src/` - C# .NET, API Endpoints, Domain, Infraestrutura.
- **Frontend**: `vue/plex-frontend/` - Vue 3, Vite, Pinia.
- **Tests**: `tests/` - Unit tests (C#) e E2E Playwright.
- **OpenSpec**: `openspec/` - Mudanças, design e propostas.

**Uso:** Localize arquivos e entenda a arquitetura sem ler documentos extensos.

### `validate_commit_msg`

Valida se uma mensagem de commit segue o formato Conventional Commit com emoji.

**Esperado:** `tipo(emoji[scope]): descrição` (ex: `feat(✨auth): add login endpoint`).

**Uso:** Verifique commits antes de finalizar ou use o `commit-skill.py` que já integra esta validação.

## Inicialização do Servidor

O servidor MCP deve estar em execução para que as ferramentas funcionem. O agente orquestrador (ou qualquer agente) deve garantir que ele está rodando:

```bash
# Iniciar se não estiver rodando
pgrep -f "mcp-server/server.py" >/dev/null || nohup python3 mcp-server/server.py >/tmp/mcp-dev.log 2>&1 &

# Verificar status
pgrep -f "mcp-server/server.py" && echo "MCP rodando" || echo "MCP parado"
```

## Integração com Outras Skills

Esta skill pode ser usada isoladamente ou combinada com outras skills (ex: `dev-spec`, `code-review-skill`). As ferramentas MCP são leves e não bloqueiam o fluxo principal do agente.

---

**Exemplo de chamada (via OpenCode config):**

```json
{
  "name": "validate_commit_msg",
  "arguments": { "message": "feat(✨auth): add login endpoint" }
}
```