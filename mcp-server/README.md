# MCP Server

Executa em stdio e escuta JSON-RPC para servir ferramentas de desenvolvimento:

- `get_best_practices`: Retorna guidelines de desenvolvimento.
- `get_code_map`: Retorna o mapa de arquitetura do backend/frontend/tests.
- `validate_commit_msg`: Verifica commits convencionais.

**Executar**

```bash
python mcp-server/server.py
```

**Ajuda para desenvolvimento**

```bash
python mcp-server/server.py &
```

**Endpoint**

```
STDIO Server: Executa no mesmo processo da habilidade do agente.
```