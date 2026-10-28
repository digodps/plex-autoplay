# Proposal

## Why

O projeto `plex-autoplay` foi reimplementado em .NET 10 com arquitetura DDD/Clean Architecture, Vue.js SPA e Docker, mas não possui testes de integração de ponta a ponta que validem o comportamento completo da stack (backend API + frontend SPA + Docker) antes de arquivar a mudança. Sem cobertura e2e, não há garantia de que os endpoints, a autenticação JWT, o proxy de imagens, o WebSocket do PlexAutoSkip e a navegação da Vue funcionem juntos em um container real.

## What Changes

- Adicionar suporte a testes de integração e2e com Playwright no projeto
- Criar arquivo `.env.example` com variáveis obrigatórias para modo dev (PLEX_URL, PLEX_TOKEN, MEDIA_ROOT, JWT_SECRET, ADMIN_PASSWORD, etc.)
- Criar scripts `start.sh`, `stop.sh` e `logs.sh` para gerenciar o container Docker
- Adicionar suíte de testes e2e que inicia o container Docker, navega pela Vue SPA, autentica via JWT, valida endpoints da API, testa o fluxo de bibliotecas, marcadores e sincronização
- Garantir que os testes rodem contra o container real (não mocks), fechando o ciclo de verificação antes do archive

## Capabilities

### New Capabilities

- `e2e-integration`: Testes de ponta a ponta que validam a stack completa (Docker + .NET API + Vue SPA + Playwright), incluindo autenticação JWT, endpoints de bibliotecas/scan/marcadores, proxy de imagens e navegação da SPA.

### Modified Capabilities

- Nenhum (os specs existentes de plex-* continuam inalterados)

## Impact

- Novos arquivos na raiz do projeto: `.env.example`, `start.sh`, `stop.sh`, `logs.sh`
- Nova suíte de testes e2e (Playwright) em diretório dedicado
- Nenhuma alteração nos specs existentes (plex-auth, plex-auto-skip, plex-images, plex-libraries, plex-markers, plex-sync)
- O container Docker precisa estar disponível para executar os testes e2e
