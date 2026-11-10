# Design: Múltiplos diretórios de mídia na página de Ajustes

## Approach

Substituir o único `<input id="media-root" v-model="form.media_root">` por `<MediaRootsField v-model="mediaRoots" />` em `SettingsView.vue`. O componente `MediaRootsField` já expõe:
- Lista de diretórios adicionados (com botão "Remover").
- Campo de texto + botão "Adicionar" + botão "Procurar…" (abre `DirectoryPicker`).
- Validação local (não duplicar, não permitir vazio).

O backend já aceita `media_roots` (array) no `SettingsRequest` (`[JsonPropertyName("media_roots")]`) e normaliza via `MediaRoots.Normalize`. O `UpdateAppSettingsUseCase` já trata `request.MediaRoots is not null`. Nada muda no backend.

Ajustes no formulário:
- `form` deixa de ter `media_root` (string) e passa a ter `mediaRoots` (array de strings).
- `loadSettings` lê `data.media_roots` (array) e preenche `mediaRoots`.
- `saveSettings` envia `media_roots: mediaRoots` no payload.
- O payload não inclui `media_root` (singular).

## Files to Modify

1. `vue/plex-frontend/src/views/Pages/SettingsView.vue`
   - Import `MediaRootsField` de `@/components/MediaRootsField.vue`.
   - Estado: `const mediaRoots = ref([])` (substitui `form.media_root`).
   - `applySettings(data)`: `mediaRoots.value = data.media_roots ?? []`.
   - `saveSettings` payload: `media_roots: mediaRoots.value`.
   - Template: substituir `<div class="rounded-lg ...">` do `media-root` por `<MediaRootsField v-model="mediaRoots" />`.
   - Remover `form.media_root` de `form` reactive object.

## Why This Is Minimal

- `MediaRootsField` já cobre: adição manual, picker de diretório, remoção, validação de duplicatas, empty state.
- Backend já valida e normaliza lista (mínimo 1, máximo 32, existe, não é `/`, deduplicado).
- Nenhuma nova dependência, nenhum token CSS novo, nenhuma rota nova.

## UI/UX Considerations

- Label do componente: "Diretórios de Mídia (MEDIA_ROOTS)" — plural.
- Placeholder do input interno: "/media".
- Helper text: "Caminhos existentes dentro do container. Use quantos precisar." (já está no componente).
- Dark mode e focus-visible já funcionam via classes globais do projeto.
- Ao menos um diretório é obrigatório; backend retorna erro se vazio — UI já exibe `error` do `saveSettings`.

## Migration

- Se no SQLite já existe `MEDIA_ROOTS` (array), carrega normal.
- Se só existe `MEDIA_ROOT` (legacy string), `AppSettingDefaults.MediaRoots` já retorna `[MediaRoot]` e o `IAppSettingsStore.GetAsync` deve migrar ou `AppSettingsSnapshot` já lê `MEDIA_ROOTS` com fallback. O `AppSettingsSnapshot` já usa `IReadOnlyList<string> MediaRoots`. Verificar que o store lê `MEDIA_ROOTS` do banco; se não existir, usa defaults.