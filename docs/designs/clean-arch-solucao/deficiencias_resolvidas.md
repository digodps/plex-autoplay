# Registro de resolução — EGM (review → resolve)

3 Goldfish em paralelo: A=Arquitetura (13), S=SOLID (8), C=Clean Code/Escopo (7).
33 achados. Status: 30 aceitos, 3 rejeitados (falso positivo / sugestão errada).
Cada aceite abaixo tem a verificação que o sustenta.

## Aceitos

| id | achado | verificação | onde entra |
|---|---|---|---|
| A-01 | `MediaPathResolver.Resolve` usa `File.Exists` | `MediaPathResolver.cs:27` confirmado | R12 |
| A-02 | Fluxo de scan duplicado (`SyncBackgroundService.RunScanAsync` × `/api/plex/scan`) | ambos implementam skip→resolve→detect→grava | R4 |
| A-03 | `MarkerEndpoints` devolve 409; `SaveMarkerUseCase` retorna em silêncio | código paralelo confirmado | R3 |
| A-04 | `PlexAutoSkipService:19` usa `GetAwaiter().GetResult()` | grep confirmado | R6 |
| A-05 | `ImageEndpoints` injeta `ImageProxyService` concreto | linha 13 confirmada | R2 |
| A-06 | DDL + `Directory.CreateDirectory` no construtor | confirmado **em 4 repos**, não só Marker | R5 |
| A-07 | `JwtTokenService` tem **2** `catch` nu (24 e 64) | grep confirmado | R8 |
| A-08 | `AuthEndpoints:19` usa `ReadToEndAsync().Result` | confirmado | R6/R7 |
| A-09 | `ScanLibrariesUseCase` nunca referenciado | grep sem referências | R4/R10 |
| A-10 | `PlexSaveEndpoint`/`PlexWebSaveEndpoint` escrevem `.env` na Web | `File.*` confirmado | R9 |
| A-12 | `ScanJobStore` concreto em `Web/Services` | confirmado | R2 |
| A-13 | Parsing manual query/corpo em `MarkerEndpoints` | confirmado | R7 |
| S-01 | Construtor de repositório com efeito de I/O | confirmado (4 repos) | R5 |
| S-03 | `PlexApiClient` mistura HTTP + telemetria + parsing | 367 LOC, partials | R2 |
| S-04 | Parsing manual de casing dentro do `PlexApiClient` | `TryGetProperty` × N | R2 (OCP) |
| S-05 | `DatabaseInitializer` com DDL hardcoded não estende | análise de design | R5 (lista versionada) |
| S-09 | Endpoints de **leitura** também injetam repositório | Theme/Marker/Series confirmados | R3 |
| S-10 | `.env` sem porta/serviço de config | confirmado | R9 |
| S-12 | Migração com `catch` nu inline | confirmado | R5/R8 |
| C-01 | 3 `catch` nu | confirmado (4 pontos: 2 no JWT) | R8 |
| C-03 | `AuthEndpoints.ExtractPassword` via regex | confirmado | R7 |
| C-05 | `PlexWebSaveEndpoint` nunca registrado | `MapPlexWebSaveEndpoint` sem chamada em `Program.cs` | R10 |
| C-06 | `ScanJobStore` fora de Infrastructure | confirmado | R2 |

## Rejeitados (com motivo)

| id | alegação | por que rejeitado |
|---|---|---|
| **C-02** | `.Wait()` em `PlexEndpoints.cs:21,31` | **Falso positivo.** `grep -rn '\.Wait()\|\.Result\|GetAwaiter'` não retorna nada em `PlexEndpoints`. Os `.Wait()` estão nos 4 repositórios, já cobertos por R5/R6. |
| **C-04** | "Remover `ScanLibrariesUseCase` e manter a lógica no endpoint/`ScanJobStore`" | **Sugestão invertida.** Deletar o caso de uso para preservar lógica no endpoint é o oposto do objetivo. O correto é fiá-lo (vira `LibraryScanUseCase`, R4). |
| **C-07** | "Código morto gera warnings que reprovam o build" | **Justificativa errada.** C# não emite warning para classe pública não usada. A remoção do código morto (C-05) é aceita por si, não por warnings. |

## Parciais / anotados

| id | nota |
|---|---|
| A-11 | Diz que o `using System.Diagnostics` do `ProcessInfoService` é "não utilizado". Na prática é usado (`Process.*`), mas a classe **sai** do Domain por R1 — o ponto vira moot. |
| S-11 | Ciclo de vida do `PlexApiClient` (singleton) — mantido como singleton de forma deliberada (client sem estado por request, base address fixo); documentado, sem ação. |
| C-07 | Remoção de código morto aceita (ver C-05); a alegação de warning, não. |

## Verificação feita (comandos)

```
grep -rn "\.Wait()\|GetAwaiter\|\.Result\b" --include=*.cs .
  → AuthEndpoints.cs:19 .Result
  → PlexAutoSkipService.cs:19 GetAwaiter
  → SqliteSeriesRepository.cs:39 / SqliteMarkerRepository.cs:38,44 / SqliteLibraryRepository.cs:37 .Wait()
grep -rn "File\.\(ReadAll\|WriteAll\|Exists\)" --include=*.cs .
  → MediaPathResolver.cs:27 (Application)
  → PlexSaveEndpoint.cs:37,39,125 ; PlexWebSaveEndpoint.cs:37,39,68 (Web)
  → ServerCertificateProvider.cs:29 (bootstrap, fora de escopo)
grep -rn "ScanLibrariesUseCase" . → só a definição (morto)
grep -rn "MapPlexWebSaveEndpoint" Program.cs → sem chamada (morto)
grep -rln "EnsureTableCreated\|CREATE TABLE" Infrastructure/Persistence → 4 repositórios
```

## Gate

Design v2 cobre 100% dos achados aceitos. Os 2 achados rejeitados não são
violações reais; o 1 restante é justificativa incorreta sobre item já aceito.
Deficiências abertas: **0**.
