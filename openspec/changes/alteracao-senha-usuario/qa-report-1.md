# QA Report 1 — alteracao-senha-usuario

**Data:** 2026-11-07
**Veredito:** PASS
**Change:** `alteracao-senha-usuario`
**Branch/worktree:** `feature/spec-alteracao-senha-usuario`
**Imagem:** `plex-autoplay:alteracao-senha-usuario` (build `--no-cache`)
**URL de validação acessada (Playwright):** `http://localhost:18081`

## Evidência de execução

| Gate | Comando | Resultado |
|---|---|---|
| Build .NET | `dotnet build PlexAutoplay.slnx` | 0 erros |
| Testes .NET | `dotnet test PlexAutoplay.slnx` | 12 passados, 0 falhas |
| Lint frontend | `npm run lint` | exit 0 |
| Build frontend | `npm run build` | sucesso |
| Schema OpenSpec | `openspec validate alteracao-senha-usuario --strict` | válido |
| Docker | `docker build --no-cache -t plex-autoplay:alteracao-senha-usuario .` | sucesso |
| Playwright | script real de navegador em `http://localhost:18081` | 18/18 asserts PASS |

## Asserts Playwright (18/18 PASS)

1. Tela `/setup` exibe instrução de token (`data-testid="setup-token-hint"`).
2. Setup com token do log conclui e redireciona para `/login`.
3. Login com senha criada redireciona para `/dashboard`.
4. Botão de configurações (gear, `aria-label="Configurações"`) removido do header.
5. Toggle de tema alterna light → dark e aplica `class="dark"` + `data-theme="dark"` no `<html>`.
6. Menu do usuário abre ao clicar em `aria-label="Menu do usuário"`.
7. Menu contém `Alterar senha` (`data-testid="change-password-menu"`).
8. Menu mantém link `Configurações` (`/settings`).
9. Modal de senha abre com os 3 campos (atual, nova, confirmação).
10. Confirmação divergente exibe erro local ("As senhas não conferem") sem chamar API.
11. Troca de senha válida retorna 204 e fecha o modal.
12. Senha antiga é rejeitada no login após a troca.
13. Login aceita somente a nova senha.
14. Navegação do menu para `/settings` funciona.
15. Tema permanece aplicado após navegação.
16. Token de setup não é reutilizado (`HTTP 409`).
17. `GET /api/auth/setup/status` reporta `setupRequired=false` após setup.
18. `PUT /api/auth/password` sem JWT retorna `401`.

## Verificações de runtime

- Container limpo emite `SETUP_TOKEN=` no primeiro start.
- Container reiniciado com credencial persistida **não** emite `SETUP_TOKEN=` (0 ocorrências).
- Senha em texto puro ausente do banco (`grep` de `NovaSenha456` em `/data/app.db` = 0).
- Capturas: `qa-evidence/qa-evidence-dashboard.png`, `qa-evidence/qa-evidence-settings.png`.

## Achados

Nenhum bloqueante. Observações não bloqueantes:

1. **502 em assets de Plex** — esperado no container isolado sem servidor Plex configurado; não relacionado à change.
2. **401 pré-login** — probe de sessão do bootstrap; esperado.

## Conclusão

PASS — requisitos do spec atendidos, sem regressões de tema/menu/navegação.
