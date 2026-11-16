# Proposal — simplificacao-setup

## O que muda

Simplificação do fluxo de configuração inicial (setup):

**Antes**: Setup em 3 passos
1. Token de setup + Nova senha + Confirmação
2. URL do Plex + Token do Plex + Testar conexão
3. Diretórios de mídia

**Depois**: Setup em 1 passo único (apenas senha)
1. Nova senha + Confirmação (senha padrão: `123456`)

Após o primeiro login, o usuário é redirecionado para a tela de alteração de senha obrigatória (já existe `ChangePasswordView.vue`).

## Por que

- Elimina configuração de Plex e mídia no setup inicial
- Reduz atrito: operador só define senha e entra
- Configuração de Plex/mídia passa a ser feita após login, na tela de Settings existente
- Senha padrão conhecida (`123456`) facilita primeiro acesso em ambientes controlados

## Escopo

**Frontend (Vue):**
- `SetupView.vue`: remover passos 2 e 3; manter apenas passo 1 (senha/confirmação); pré-preencher senha padrão `123456`
- Rota `/setup`: continua pública; guarda de navegação inalterada
- Após `submitSetup` sucesso + login automático → redirecionar para `/change-password` (rota nova ou existente)

**Backend (.NET):**
- `CompleteSetupUseCase`: remover validação de `PlexUrl`, `PlexToken`, `MediaRoots`; manter apenas `Token` + `NewPassword`
- `SetupRequest` DTO: remover campos `PlexUrl`, `PlexToken`, `MediaRoots`
- `CompleteSetupRequest` record: remover `PlexUrl`, `PlexToken`, `MediaRoots`
- `IAppSettingsStore`: setup não persiste mais configurações de Plex/mídia
- `SetupTokenService`: inalterado (token de uso único)

**Segurança:**
- Senha padrão `123456` é fraca → troca obrigatória no primeiro login mitiga
- `ChangePasswordUseCase` já exige senha atual + nova válida (8-128 chars)

## Fora do escopo

- Tela de Settings para configurar Plex/mídia após login (já existe ou será separada)
- Recuperação de senha perdida
- Invalidação de sessões JWT após troca de senha

## Critérios de aceite

1. Acesso a `/setup` sem credencial no SQLite → exibe apenas campos: token, nova senha (pré-preenchida `123456`), confirmação
2. Submissão com token válido + senhas iguais ≥ 8 chars → cria credencial, invalida token, faz login automático, redireciona para `/change-password`
3. Tela `/change-password` exige senha atual (`123456`) + nova senha + confirmação; sucesso → volta ao dashboard
4. Tema light/dark e menus funcionando nas telas de setup e change-password
5. Build `npm run build` + `dotnet build` + `docker compose build` passam