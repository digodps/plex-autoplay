# Change Proposal — simplificar-setup-inicial

## Why

O setup atual exige que o usuário leia o token `SETUP_TOKEN` do log do container (`docker logs plex-autoplay`), acesse `/setup` e informe o token. Isso cria fricção desnecessária na primeira configuração.

O usuário quer simplificar:
- Primeira configuração deve começar no **login** — a tela de login mostra a senha padrão `123456`
- Usuário faz login com `123456`
- Após autenticar, o **setup inicia obrigando a troca de senha** (nova senha + confirmação)
- Depois continua solicitando dados do Plex e diretórios de mídia
- Remove a necessidade do token `SETUP_TOKEN` no log

## What Changes

1. **Backend**:
   - Remover `SetupTokenService` e `ISetupTokenValidator`
   - No primeiro start, se não existe credencial no SQLite, o backend NÃO gera token
   - `GET /api/auth/setup/status` retorna `{ "setupRequired": true, "defaultPassword": "123456" }` quando não há credencial
   - `POST /auth/login` com senha `123456` quando credencial existe (primeiro login) → retorna `{ token, forcePasswordChange: true }`
   - `POST /api/auth/setup` aceita `{ currentPassword, newPassword, confirmPassword, plexUrl, plexToken, mediaRoots }` — valida `currentPassword` = "123456" (primeiro setup) OU hash salvo (setups futuros)
   - Login com senha padrão força `forcePasswordChange: true` no JWT/resposta

2. **Frontend**:
   - `SigninView.vue`: exibir "Senha padrão: 123456" na tela de login
   - Login com 123456 → detecta `forcePasswordChange: true` → redireciona para `/setup` (fluxo obrigatório)
   - `SetupView.vue`: 
     - Passo 1: troca de senha (senha atual=123456 pré-preenchida/oculta, nova senha, confirmação)
     - Passo 2: Plex (URL + token)
     - Passo 3: Mídia (diretórios)
   - Remove campo de token do setup
   - Após setup completo → autentica com nova senha → dashboard

3. **Remover**:
   - `SetupTokenService` e `ISetupTokenValidator`
   - Token de setup do log do container
   - Campo de token no setup

4. **Banco**: mantém `AuthCredential` (já existe)

## Impact

- BREAKING: remove token de setup, muda API `/api/auth/setup` e `/auth/login`
- UX: muito mais simples — sem precisar ler logs do container
- Segurança: senha padrão fraca (123456) mas forçada a trocar imediatamente após primeiro login
- Fluxo: login → setup obrigatório (senha → Plex → mídia) → dashboard