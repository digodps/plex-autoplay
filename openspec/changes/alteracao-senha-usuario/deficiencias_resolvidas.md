# Deficiências resolvidas — alteracao-senha-usuario

## EGM — Goldfish Review A/S/C

Requisitos revisados: setup inicial com token de uso único no log, remoção de
`ADMIN_PASSWORD`, senha do operador no SQLite e troca de senha pelo menu.

## Achados aceitos

| # | Revisor | Achado | Resolução |
|---|---|---|---|
| A1 | Arquitetura | Guarda de rotas não conhece `/setup` | Rota pública, consulta de `setupRequired` antes do guard autenticado, sem loop |
| A2 | Arquitetura | `ADMIN_PASSWORD` permanece em código/config/docs | Remover de `Program`, `PlexConfigStore`, `.env.example`, compose, scripts, CI, README e e2e |
| A3 | Arquitetura | E2E não obtém token do log | Hook `SETUP_TOKEN` para ambientes de teste; volume limpo; token sempre logado |
| A4 | Arquitetura | Composition root ainda cria login com senha de ambiente | Registro no DI; token emitido após `DatabaseInitializer`; remover parâmetro `adminPassword` |
| A5 | Arquitetura | Token pode sumir com `LOG_LEVEL` alto | Escrever token direto em `Console.Out`, independente do nível |
| A6 | Arquitetura | Rotas de auth fora de `/api` ficariam públicas | Rotas canônicas `/api/auth/*`, allowlist exata no middleware, alias legado de login |
| A7 | Arquitetura | Troca de senha sem senha atual | `currentPassword` obrigatória no body e verificada contra o hash |
| S1 | SOLID | Lógica de setup no endpoint | `CompleteSetupUseCase` na Application com portas |
| S2 | SOLID | Token service mistura geração/estado/log | `SetupTokenService` com `EnsureGenerated`/`TryValidate`/`Invalidate`; log no composition root |
| S3 | SOLID | Middleware depende de classe concreta | `ValidateToken` na porta `ITokenService`; injetar interface |
| S4 | SOLID | Composition root disperso | Dependências de auth registradas em `ServiceCollectionExtensions` |
| S5 | SOLID | Política de senha duplicada | `PasswordPolicy` único na Application; UI espelha mensagem |
| S6 | SOLID | Hasher concreto na Application | Porta `IPasswordHasher` + `Pbkdf2PasswordHasher` na Infrastructure |
| S8 | SOLID | Porta não expõe “configurado” | `IsConfiguredAsync`, `CreateAsync` (não sobrescreve) e `UpdateAsync` |
| C1 | Clean Code | E2E sem bootstrap e com estado persistido | Setup via `SETUP_TOKEN`; volume/DB limpo por execução |
| C3 | Clean Code | `/setup` ausente e loop de redirecionamento | Mesma resolução de A1 |
| C4 | Clean Code | Marcador do token inconsistente | Formato único `SETUP_TOKEN=<token>` no log e na spec |
| C5 | Clean Code | Token vaza em artefatos de log | Filtrar `SETUP_TOKEN=` ao copiar logs para `test-artifacts` |
| C6 | Clean Code | `SettingsModal` inerte duplica senha | Remover componente; `PasswordModal` com labels/ids/testids |
| C7 | Clean Code | Testes omitem limites e casos de erro | Testes de hasher, política, login, setup, troca e repositório; iterações baixas em teste |

## Achados parciais

| # | Revisor | Achado | Resolução |
|---|---|---|---|
| A8 | Arquitetura | Docker não compila Vue | Manter convenção do repo: build frontend + commit do bundle em `wwwroot`; QA valida container final |
| C2 | Clean Code | JWT antigo continua válido e cookie não é HttpOnly | Aceito `currentPassword`; invalidação de JWT stateless e cookie `HttpOnly` ficam fora do escopo desta change (contrato existente) |
| C8 | Clean Code | Specs principais ainda citam `ADMIN_PASSWORD` | Código/config/docs/e2e atualizados; specs principais serão sincronizadas no archive da change |

## Achados rejeitados

| # | Revisor | Achado | Motivo |
|---|---|---|---|
| S7 | SOLID | Connection string duplicada em `DatabaseInitializer` e DI | Padrão existente do projeto; a change não altera a fonte de conexão nem aumenta o risco funcional. Refatoração separada. |

## Token de uso único

Reforçado em requisito e design: `SETUP_TOKEN=` é invalidado no primeiro setup
bem-sucedido; tentativas posteriores retornam `409` (credencial existente) ou
`401` (token inválido). A tela `/setup` informa explicitamente que o token é de
uso único e está no log do container.
