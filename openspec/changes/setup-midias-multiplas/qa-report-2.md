# QA Report — setup-midias-multiplas (iteração 2: wizard em 3 passos)

**Resultado: PASS**

- URL: `http://127.0.0.1:8091` (app local) + Plex mock `http://127.0.0.1:32411`
- Playwright: `tests/e2e/qa-setup-multidir.mjs` — **27/27 checks OK**

## Mudança avaliada

1. `/setup` virou wizard sequencial: (1) token + senha, (2) URL/token do Plex + "Testar conexão", (3) diretórios de mídia.
2. Conclusão faz login automático e abre `/dashboard` sem passar pelo `/login`.
3. `POST /api/plex/identity` liberado sem sessão apenas enquanto `setupRequired` (mesma regra do seletor de diretórios).

## Evidências

| Critério | Verificação | Resultado |
| --- | --- | --- |
| Passo 1 só senha | painel `setup-panel-password` visível na entrada | PASS |
| Avanço 1→2 | "Continuar" abre `setup-panel-plex` | PASS |
| Teste de Plex no passo 2 | `POST /api/plex/identity` → `Conectado: Plex Mock (1.40.0)` | PASS |
| Avanço 2→3 | "Continuar" abre `setup-panel-media` | PASS |
| Mídia no passo 3 | picker seleciona `/tmp/opencode/qa/media/Animes`; segundo diretório adicionado à mão | PASS |
| Conclusão autenticada | `204` → `/dashboard` direto | PASS |
| Sessão criada | cookie `auth_token` presente após setup | PASS |
| Regressão de tema/menus | toggle light↔dark, breadcrumb e menu do usuário | PASS |
| Sem erros de runtime | `pageerror` vazio | PASS |

## Observação

Fluxo continua atômico no backend (settings antes da credencial); os três passos são de UI e um único `POST /api/auth/setup` é enviado no passo 3, evitando instalação travada.
