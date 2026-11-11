# Design

## Context

Proposta: bibliotecas de séries do Plex não carregam; necessidade de descoberta, filtragem por tipo `show`, persistência local e mapeamento de arquivo de mídia dos episódios via `MEDIA_ROOTS`. (Veja proposal.md - Why/What Changes.)

Backend .NET (`PlexAutoplay.Web`) já possui cliente Plex (`openspec/specs/plex-api-client`) e infraestrutura de sincronização (`openspec/specs/plex-sync`). A mudança adiciona o serviço de descoberta de bibliotecas e mapeamento de caminhos locais sobre essas bases existentes.

## Goals / Non-Goals

**Goals:**
- Descobrir bibliotecas do Plex (tipo show) e persistir no banco local.
- Listar episódios de bibliotecas de séries com resolução de `local_path` via `MEDIA_ROOTS`.
- Expor dados de bibliotecas/episódios para a camada frontend.

**Non-Goals:**
- Sincronização contínua/agendada de bibliotecas (pertence a `plex-sync`).
- Download ou transcodificação de mídia.
- Alteração de especificação `plex-libraries` existente.

## Decisions

1. **Reutilizar cliente Plex existente (`plex-api-client`)** em vez de criar novo HTTP client.
   - Rationale: evita duplicação de código de autenticação/retry.
   - Alternativa considerada: cliente dedicado para bibliotecas. Descartado por redundância.
2. **Persistência no banco local existente** (tabela de bibliotecas).
   - Rationale: não introduz novo esquema; apenas consome tabela já definida.
3. **Resolução `MEDIA_ROOTS` no backend**, retornando `local_path` + `thumb` no contrato da API.
   - Rationale: caminho de disco é decisão de servidor; frontend não deve resolver caminhos locais.

## Risks / Trade-offs

- [Plex servidor indisponível → falha na descoberta] → erro explícito retornado; UI exibe estado de erro.
- [MEDIA_ROOTS ausente → `local_path` null] → episódio continua listado, sem caminho local.
- [Caminho resolvido não existe em disco] → log do arquivo ausente; ignora detecção de marcadores.

## Migration Plan

1. Implantar serviço de descoberta de bibliotecas.
2. Executar descoberta inicial contra Plex configurado em `PLEX_URL`.
3. Rollback: reverter commit do serviço; banco não sofre alteração de esquema.

## Open Questions

- Nenhuma.
