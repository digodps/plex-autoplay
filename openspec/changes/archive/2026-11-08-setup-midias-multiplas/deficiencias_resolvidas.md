# Deficiências resolvidas — EGM (Goldfish Review A/S/C)

Change: `setup-midias-multiplas`
Revisores: A (arquitetura/integração), S (SOLID/coesão), C (clean code/escopo/testabilidade).
Total de achados: 36. Aceitos: 24. Parciais: 8. Rejeitados: 4.

## Apresentação

Dois achados apareceram nos três revisores e mudaram o design:

- **Seletor de diretórios no `/setup` recebia 401** (A1, C1): o wizard roda sem sessão. Aceito — `/api/filesystem/directories` passa a ser liberado pelo `JwtAuthMiddleware` **apenas quando `setupRequired`**. Cenário adicionado em `setup-inicial` e `media-roots`; tarefa 4.2.
- **Setup não atômico** (A3, S1, C5): gravar settings depois de criar credencial deixava a instalação travada (setup `409`, sem Plex/mídia). Aceito — ordem invertida (settings antes da credencial) e cenário "Falha ao criar credencial" adicionado. Tarefa 3.1.

## Achados

### Arquitetura

1. **A1 — 401 no picker durante setup.** Aceito. Allowlist condicional por `setupRequired`. Design §4, tarefa 4.2.
2. **A2 — e2e quebra com novo payload / mock sem `/identity`.** Aceito. Tarefas 7.3 e 7.4.
3. **A3 — atomicidade do setup.** Aceito parcialmente: ordem `SaveAsync` → `CreateAsync`; transação única entre tabelas descartada (stores separados, ganho baixo, risco de acoplar persistência). Design §5.
4. **A4 — assinaturas quebram consumidores e testes sem tarefa.** Aceito. Tarefa 7.1 + 2.3.
5. **A5 — URL absoluta precisa cobrir todos os métodos.** Aceito. Tarefa 6.1 lista os cinco métodos; verificação "nenhum `GetAsync(\"/...\")` sobra".
6. **A6 — `Accept: application/json` vs parser XML.** Aceito. `GetIdentityAsync` envia `Accept: application/xml`; cenário em `plex-api-client`; tarefa 6.2.
7. **A7 — singleton capturando typed client.** Aceito. `CompleteSetupUseCase` vira `Transient`; tarefa 3.3.
8. **A8 — seed não roda em banco populado.** Aceito. Passo de migração independente do contador; tarefa 1.3.
9. **A9 — contrato `/api/settings` ambíguo.** Aceito. `media_roots: string[]`, `null` preserva, `[]` → 400, `JsonPropertyName`; cenários em `media-roots`.
10. **A10 — setup pode trancar operador fora.** Rejeitado. A falha de validação não persiste nada e o formulário permanece com os valores; corrigir URL/token no próprio form resolve. `skipPlexValidation` seria escopo e superfície extra.
11. **A11 — UI nova não é publicada pela imagem.** Aceito. Tarefa 8.3 (build Vue + cópia para `wwwroot`).
12. **A12 — ordem de sondagem pode resolver arquivo errado.** Aceito. Ordenação por especificidade (sufixo longo → curto, depois raiz); teste de colisão na tarefa 2.4.

### SOLID / coesão

13. **S1 — `CompleteSetupUseCase` com 6 responsabilidades e enum sem payload.** Aceito parcialmente. Extraído validador compartilhado de `plex_url`/raízes (achado 15); resultado passa a carregar `errors`. Não foi criado um orquestrador extra — seria abstração para um único fluxo.
14. **S2 — IO de filesystem na Application.** Parcial. Mantido na Application: `UpdateAppSettingsUseCase` já valida `Directory.Exists` e a regra fica testável com diretórios temporários. Porta dedicada seria camada para embrulhar uma chamada. Registrado em design §2.
15. **S3 — validação de URL/raiz duplicada em três lugares.** Aceito. Validador único reutilizado; tarefa 1.5.
16. **S4 — não injetar store no `PlexApiClient`; corrigir só o handler com `TrimStart('/')`.** Rejeitado. O requisito explícito do solicitante é URL absoluta no cliente (ou `BaseAddress`), com handler restrito a host/token. Correção só no handler mantém a responsabilidade de URL no transporte, que é o que se quer eliminar.
17. **S5 — dependência cativa no DI.** Aceito (mesma correção do 7).
18. **S6 — `IPathResolver.Resolve` lendo config.** Rejeitado. Manter a lista explícita preserva resolver sem estado e testável; 5 chamadas atualizadas são custo aceitável.
19. **S7 — normalização com dois donos.** Aceito. Normaliza só na Application; store grava verbatim.
20. **S8 — parser de seed duplicado/código morto.** Aceito. Um único parser; migração separada do seed inicial.
21. **S9 — `FileSystemEndpoints` furando fronteira.** Rejeitado. Endpoint é fino (listar diretórios) e tem um único consumidor; porta + query + adapter para `Directory.GetDirectories` é cerimônia sem ganho.
22. **S10 — três formatos de erro.** Parcial. Padronizado `errors: string[]` com prefixo de campo (compatível com o `/api/settings` existente). `{field,message}` exigiria mudar contrato já consumido pela UI.
23. **S11 — portas em `Domain/Ports` vs `Application`.** Aceito. Texto do proposal corrigido para `src/PlexAutoplay.Application`.
24. **S12 — parsing de erro duplicado no front / senha duplicada.** Aceito. `extractErrors` em `api.js`; front só confere igualdade das senhas.

### Clean code / escopo / testabilidade

25. **C1 — 401 no picker.** Aceito (ver 1).
26. **C2 — endpoint de filesystem sem escopo.** Parcial. Jail em `MEDIA_ROOTS` rejeitado: o objetivo é descobrir caminhos novos montados. Aceitos: 400 para relativo, 404/403, `parent: null` na raiz, cap 2000, cenário de 401 após setup.
27. **C3 — contrato de erro ambíguo.** Aceito (ver 22).
28. **C4 — limite 32 sem comportamento.** Aceito. Cenário "33 itens → 400".
29. **C5 — atomicidade, timeout, esquema de URL.** Aceito. Ordem do setup, timeout 10s, `http/https` obrigatório, cenário de esquema inválido.
30. **C6 — cenários duplicados/contraditórios.** Parcial. Cenários reescritos e deduplicados nas capabilities novas; em `plex-libraries`/`plex-scan-real` os cenários em inglês são do spec vigente e foram preservados (o arquivo exige mantê-los).
31. **C7 — plano de testes insuficiente.** Aceito. `dotnet test` no gate, testes por item; tarefas 2.4, 7.1, 7.2, 8.2.
32. **C8 — e2e não cobre fluxo novo.** Aceito (ver 2). Cenários felizes/falha cobertos por unit + Playwright do modal.
33. **C9 — binder JSON rejeita `media_roots`.** Aceito. `JsonPropertyName("media_roots")` em request/response; cenário de contrato.
34. **C10 — canonicalização/dedupe indefinidos.** Aceito. `Path.GetFullPath`, sem barra final, `/` proibido, dedupe pós-canonicalização.
35. **C11 — docs/env/compose desatualizados.** Aceito. Tarefa 7.5.
36. **C12 — critérios de aceite não objetivos.** Aceito. Ordenação `OrdinalIgnoreCase`, cap 2000, exclusão de dot-dirs/arquivos, `data-testid` definidos no design §7.
