# Design

## Context

O projeto `plex-autoplay` já foi reescrito em .NET 10 com arquitetura DDD/Clean Architecture, Vue.js SPA servida pelo ASP.NET Core, Dockerfile com ffmpeg, docker-compose com volume persistente (`plex-data`) e bind mount de `MEDIA_ROOT`. Os specs principais (plex-auth, plex-libraries, plex-markers, plex-sync, plex-auto-skip, plex-images) já foram sincronizados para `openspec/specs/`. Não existe atualmente nenhuma suíte de testes de integração e2e no repositório. A mudança anterior (`replace`) implementou toda a aplicação; esta mudança adiciona a camada de validação e2e antes do archive.

## Goals / Non-Goals

**Goals:**
- Validar a stack completa (Docker + .NET API + Vue SPA) com testes reais (sem mocks de rede)
- Cobrir fluxos críticos: login JWT, bibliotecas de séries, CRUD de marcadores, proxy de imagens, sincronização via BackgroundService
- Usar `.env` para configuração de variáveis obrigatórias em modo dev
- Gerenciar o container Docker via scripts `start.sh`, `stop.sh`, `logs.sh`
- Executar testes com Playwright contra o container em execução

**Non-Goals:**
- Testes unitários de camadas internas (Domain/Application/Infra) — estes pertencem a outra mudança
- Testes contra Plex Media Server real (os testes usam a URL/token configurados, mas não dependem de conteúdo específico do servidor)
- Testes de desempenho ou carga
- Suporte a múltiplos ambientes simultâneos (apenas um container por vez)

## Decisions

**1. Playwright como framework de e2e**
- Rationale: suporte nativo a navegadores reais (Chromium), API moderna, boa integração com CI, screenshots/trace em falhas
- Alternativas consideradas: Cypress (rejeitado: menos flexível para arranjo/encerra de container Docker), Selenium (rejeitado: mais verboso, setup mais complexo)

**2. Testes rodam contra container real, não localhost sem Docker**
- Rationale: valida o Dockerfile, docker-compose, variáveis de ambiente e servindo de arquivos estáticos da Vue pelo ASP.NET em condições reais de produção
- Alternativa considerada: rodar `dotnet run` + `npm run dev` diretamente (rejeitado: não valida o empacotamento Docker)

**3. Scripts shell para gerenciar o ciclo de vida do container**
- Rationale: simplicidade, portabilidade, alinhado ao padrão existente do projeto (start.sh/stop.sh/logs.sh já foram criados na mudança anterior)
- Alternativa considerada: task runner (Makefile, justfile) (rejeitado: adiciona dependência extra para desenvolvedores)

**4. `.env` carregado antes de iniciar testes**
- Rationale: garante que PLEX_URL, PLEX_TOKEN, JWT_SECRET, ADMIN_PASSWORD e MEDIA_ROOT estejam disponíveis tanto para o container quanto para os testes Playwright
- Alternativa considerada: variáveis hardcoded no código de teste (rejeitado: risco de exposição de credenciais, inflexível)

**5. Testes aguardam o container ficar saudável antes de iniciar**
- Rationale: evita flaky tests por race condition entre subida do container e início dos testes Playwright
- Implementação: script de espera (`wait-for-it` ou loop de `curl`) na porta 8080 antes de lançar o Playwright

## Risks / Trade-offs

[Testes e2e dependem do Docker estar instalado e rodando] → Mitigar documentando pré-requisito no README e falhando com mensagem clara se Docker não estiver disponível

[Testes podem ser flaky se o container demorar a iniciar] → Mitigar com retry de conexão e timeout adequado na porta 8080

[Credenciais do Plex (PLEX_TOKEN) expostas no .env] → Mitigar lembrando no .env.example que o token não deve ser commitado; adicionar .env ao .gitignore

[Playwright precisa baixar binários de navegador] → Mitigar incluindo `npx playwright install` no script de setup/CI

## Open Questions

- Os testes devem rodar automaticamente em CI (GitHub Actions) ou apenas localmente antes do archive?
- Deve haver um limite de timeout global para a suíte e2e (ex: 5 minutos)?
