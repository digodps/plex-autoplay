# Proposal: Múltiplos diretórios de mídia na página de Ajustes

## Problem

A página de `/ajustes` (`SettingsView.vue`) ainda expõe um único campo de texto `media_root` para o diretório de mídia. Ela não reaproveita o componente `MediaRootsField.vue` (já usado no `SetupView.vue`) e envia ao backend a chave `media_root` (singular), que o endpoint `PUT /api/settings` ignora: o backend aceita `media_roots` (array JSON). Resultado: o usuário só consegue informar um diretório na página de Ajustes e a alteração nem chega a ser persistida.

O `SetupView` já permite múltiplos diretórios via `MediaRootsField.vue`; o backend já normaliza `media_roots` via `MediaRoots.Normalize` (JSON array, máximo 32, deduplicado, rejeita `/`). Faltava apenas a UI de Ajustes refletir isso.

## Solution

Substituir o campo de texto único `media_root` de `SettingsView.vue` pelo componente `MediaRootsField.vue`:

- Form usa um array `mediaRoots` carregado de `data.media_roots`.
- `MediaRootsField v-model="mediaRoots"` permite adicionar/remover quantos diretórios o usuário quiser (com campo de texto + "Procurar" via DirectoryPicker).
- Ao salvar, envia `media_roots` (array) no `PUT /api/settings`; backend normaliza, valida e persiste `MEDIA_ROOTS` no SQLite.
- Mantém validação de ao menos um diretório e máximo de 32 (regras já em `MediaRoots.Normalize`).

## Scope

- `vue/plex-frontend/src/views/Pages/SettingsView.vue` — trocar input único por `MediaRootsField`.
- Sem novas dependências; `MediaRootsField.vue` e `DirectoryPicker.vue` já existem.

## Acceptance Criteria

1. Página de Ajustes exibe `MediaRootsField` permitindo adicionar múltiplos diretórios de mídia.
2. Carrega `media_roots` do GET `/api/settings` no carregamento da página.
3. Salva `media_roots` (array) no `PUT /api/settings`.
4. Botão "Remover" remove um diretório da lista.
5. Não permite lista vazia ao salvar (backend rejeita com erro `media_roots`).
6. Build passa: `npm run lint` e `npm run build` em `vue/plex-frontend`.
7. `openspec validate multiplos-diretorios-configuracao --strict` passa.

## Affected Specs

- `specs/settings/spec.md` (se existir) — diretórios de mídia.
- Backend `SettingsEndpoints` já aceita `media_roots`; nenhuma mudança de contrato necessária.
