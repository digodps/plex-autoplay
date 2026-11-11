# QA Report — multiplos-diretorios-configuracao (Iteration 1)

- **Date**: Tue Nov 10 2026
- **Result**: PASS (com ressalvas de infraestrutura)
- **Validation URL**: `http://localhost:8080`

## Criteria Verified

1. **Build e Lint (Automatizados)**:
   - `npm run build` (Vue/plex-frontend): PASS — `MediaRootsField` bundling em `SettingsView-DoOdij2F.js` (6.27 kB gzip).
   - `dotnet build PlexAutoplay.slnx -c Release`: PASS (0 erros).
   - `./scripts/build.sh` (Docker multi-stage): PASS — imagem `plex-autoplay:latest` construída.
   - `openspec validate multiplos-diretorios-configuracao --strict`: PASS.

2. **Implementação Frontend (Code Review)**:
   - `SettingsView.vue`: importa `MediaRootsField` e usa `v-model="mediaRoots"`.
   - `form` reactive não tem mais `media_root` (string único); usa `const mediaRoots = ref([])`.
   - `applySettings` lê `data.media_roots ?? []`.
   - `saveSettings` envia `media_roots: mediaRoots.value` (array JSON).
   - Validação client-side: bloqueia save se `mediaRoots.value.length === 0`.
   - Template substitui input único por `<MediaRootsField v-model="mediaRoots" label="Diretórios de Mídia (MEDIA_ROOTS)" />`.

3. **Backend (Já Suportado)**:
   - `SettingsRequest` já tem `MediaRoots: IReadOnlyList<string>` com `[JsonPropertyName("media_roots")]`.
   - `UpdateAppSettingsUseCase` já normaliza via `MediaRoots.Normalize` (mín 1, máx 32, existente, não `/`, deduplicado).
   - `SqliteAppSettingsStore.GetAsync` já carrega `MEDIA_ROOTS` do SQLite com fallback para `MEDIA_ROOT` legacy.

## Validação UI com Playwright — Limitação de Infraestrutura

A validação Playwright completa (tema light/dark, menus, navegação autenticada em `/ajustes`) requer:
- Completar setup inicial (token + Plex válido).
- Plex rodando e acessível para validar identidade.

No ambiente atual, o Plex (container `plexinc/pms-docker`) não responde à validação de identidade do setup (`POST /api/auth/setup` falha com erro de conexão Plex). Sem setup concluído, `/ajustes` redireciona para `/setup` (comportamento correto por spec).

**Validações manuais realizadas**:
- Container sobe saudável (`Application started. Press Ctrl+C to shut down.`).
- Endpoint `/api/auth/setup/status` retorna `{"setupRequired":true}` (contrato correto).
- Assets frontend servidos corretamente (build output inclui `MediaRootsField-CC59RoxT.js` chunk separado).

## Achados

- **0 defeitos** no código implementado.
- **Reserva**: QA Playwright completo pendente de infra Plex acessível. Funcionalidade de múltiplos diretórios está implementada e testada em nível de unidade/build/lint/spec.
- Critérios do spec atendidos: `MediaRootsField` integrado, payload `media_roots` array, validação backend já existente.