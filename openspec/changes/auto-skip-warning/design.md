# Design — Auto-Skip Authorization Warning (Netflix-style)

## Architecture

```
┌──────────────────────┐          ┌──────────────────────┐
│ SettingsView (Vue)   │  PUT /api/settings   │ AppSettings (.NET)  │
│  - Toggle            │  ─────────────────▶  │  - AutoSkipAuthorized│
│  - Modal Netflix     │  GET /api/settings   │  - SQLite persistence│
└──────────────────────┘  ◀──────────────────  └──────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│ PlexAutoSkipBackgroundService                                    │
│  - Poll sessions (2s)                                            │
│  - On marker hit: if (!settings.AutoSkipAuthorized) return;      │
│  - Else: execute existing seek logic                             │
└──────────────────────────────────────────────────────────────────┘
```

## 1. Backend Changes

### 1.1 AppSettings — Nova Chave

**Arquivo:** `src/PlexAutoplay.Application/AppSettings.cs`

```csharp
public static class AppSettingKeys
{
    // ... existing ...
    public const string AutoSkipAuthorized = "AUTO_SKIP_AUTHORIZED";
    public static readonly string[] All = [ ..., AutoSkipAuthorized ];
}

public static class AppSettingDefaults
{
    // ... existing ...
    public const bool AutoSkipAuthorized = false;
}

public sealed record AppSettingsSnapshot(
    string PlexUrl,
    string PlexToken,
    IReadOnlyList<string> MediaRoots,
    int SyncIntervalMinutes,
    int SyncCatalogIntervalMinutes,
    bool AutoSkipAuthorized)   // NOVO
{
    public bool PlexTokenSet => !string.IsNullOrEmpty(PlexToken);
    // ...
}
```

### 1.2 SqliteAppSettingsStore — Leitura/Escrita

**Arquivo:** `src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteAppSettingsStore.cs`

```csharp
// GetAsync(): adicionar na construção do snapshot
bool autoSkipAuthorized = GetBool(values, AppSettingKeys.AutoSkipAuthorized, AppSettingDefaults.AutoSkipAuthorized);

// Helper GetBool
private static bool GetBool(IReadOnlyDictionary<string, string> values, string key, bool fallback) =>
    values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

// SaveAsync(): incluir no dicionário values
[AppSettingKeys.AutoSkipAuthorized] = settings.AutoSkipAuthorized.ToString().ToLowerInvariant(),

// AppSettingsSeeder.SeedAsync(): adicionar no switch fallback
AppSettingKeys.AutoSkipAuthorized => AppSettingDefaults.AutoSkipAuthorized.ToString().ToLowerInvariant(),
```

### 1.3 UpdateAppSettingsUseCase — Validação

**Arquivo:** `src/PlexAutoplay.Application/UpdateAppSettingsUseCase.cs`

```csharp
public sealed record UpdateAppSettingsRequest(
    string? PlexUrl,
    string? PlexToken,
    IReadOnlyList<string>? MediaRoots,
    int? SyncIntervalMinutes,
    int? SyncCatalogIntervalMinutes,
    bool? AutoSkipAuthorized);   // NOVO

// ExecuteAsync():
var autoSkipAuthorized = request.AutoSkipAuthorized ?? current.AutoSkipAuthorized;
// passa para AppSettingsSnapshot
await _store.SaveAsync(new AppSettingsSnapshot(
    plexUrl, token, mediaRoots, syncInterval, catalogInterval, autoSkipAuthorized));
```

### 1.4 SettingsEndpoints — GET/PUT

**Arquivo:** `src/PlexAutoplay.Web/Endpoints/SettingsEndpoints.cs`

```csharp
// SettingsRequest
[JsonPropertyName("auto_skip_authorized")]
public bool? AutoSkipAuthorized { get; set; }

// ToResponse
private static object ToResponse(AppSettingsSnapshot settings) => new
{
    // ... existing ...
    auto_skip_authorized = settings.AutoSkipAuthorized
};
```

### 1.5 PlexAutoSkipBackgroundService — Gate de Autorização

**Arquivo:** `src/PlexAutoplay.Infrastructure/PlexAutoSkipBackgroundService.cs`

```csharp
private async Task PollAndSkipAsync(CancellationToken ct)
{
    var settings = await _settings.GetAsync();
    
    // NOVO: gate de autorização — se não autorizado, não faz nada
    if (!settings.AutoSkipAuthorized)
    {
        _logger.LogDebug("Auto-skip not authorized (auto_skip_authorized=false). Skipping cycle.");
        return;
    }
    
    // ... resto do código existente inalterado ...
}
```

---

## 2. Frontend Changes

### 2.1 SettingsView.vue — Toggle + Modal Netflix

**Arquivo:** `vue/plex-frontend/src/views/Pages/SettingsView.vue`

```vue
<script setup>
// ... imports existentes ...
const form = reactive({
    // ... existing ...
    auto_skip_authorized: false
})
const showAuthModal = ref(false)
const pendingAuthToggle = ref(false)

applySettings(data) {
    // ... existing ...
    form.auto_skip_authorized = data.auto_skip_authorized ?? false
}

async function saveSettings() {
    // ... existing ...
    // Enviar auto_skip_authorized no payload
}

async function toggleAutoSkipAuthorized() {
    if (!form.auto_skip_authorized) {
        // Tentando LIGAR — mostrar aviso Netflix
        showAuthModal.value = true
        pendingAuthToggle.value = true
    } else {
        // Desligando — sem aviso, direto
        await saveSettings()
    }
}

function confirmAuth() {
    form.auto_skip_authorized = true
    showAuthModal.value = false
    pendingAuthToggle.value = false
    saveSettings()
}

function cancelAuth() {
    form.auto_skip_authorized = false
    showAuthModal.value = false
    pendingAuthToggle.value = false
}
</script>

<template>
<!-- ... campos existentes ... -->

<!-- Toggle Autorizar Auto-Skip -->
<div class="mb-6">
    <label class="flex items-center gap-3 cursor-pointer">
        <input
            type="checkbox"
            v-model="form.auto_skip_authorized"
            @change="toggleAutoSkipAuthorized"
            class="w-5 h-5 text-orange-500 border-gray-300 rounded focus:ring-orange-500"
        >
        <span class="text-gray-900 dark:text-gray-100">Autorizar skip automático</span>
    </label>
    <p class="text-sm text-gray-500 dark:text-gray-400 mt-1 ml-8">
        Quando habilitado, o Plex pulará introduções e créditos automaticamente.
    </p>
</div>

<!-- Modal estilo Netflix -->
<Teleport to="body">
    <Transition name="fade">
        <div v-if="showAuthModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" @click.self="cancelAuth">
            <div class="bg-white dark:bg-gray-800 rounded-xl p-6 max-w-md w-full shadow-2xl animate-in slide-in-from-top-4 duration-200">
                <div class="flex items-center gap-3 mb-4">
                    <div class="w-12 h-12 bg-orange-100 dark:bg-orange-900/30 rounded-full flex items-center justify-center">
                        <svg class="w-6 h-6 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                    </div>
                    <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Autorizar skip automático?</h2>
                </div>
                
                <p class="text-gray-600 dark:text-gray-300 mb-6">
                    Ao autorizar, o sistema <strong>pulará automaticamente introduções e créditos</strong> 
                    durante a reprodução, sem pedir confirmação a cada episódio.
                </p>
                
                <div class="flex gap-3 justify-end">
                    <button 
                        @click="cancelAuth"
                        class="px-4 py-2 text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg font-medium transition-colors"
                    >
                        Cancelar
                    </button>
                    <button 
                        @click="confirmAuth"
                        class="px-4 py-2 bg-orange-500 text-white hover:bg-orange-600 rounded-lg font-medium transition-colors"
                    >
                        Autorizar
                    </button>
                </div>
            </div>
        </div>
    </Transition>
</Teleport>
</template>
```

---

## 3. Database Migration

- Tabela `AppSetting`: nova linha `AUTO_SKIP_AUTHORIZED` inserida por `AppSettingsSeeder.SeedAsync()` no primeiro start (default `false`)
- Nenhuma migração DDL necessária (chave-valor)

---

## 4. Testing Strategy

### Unit Tests (.NET)
- `AppSettingsSnapshot` inclui `AutoSkipAuthorized`
- `SqliteAppSettingsStore.GetAsync`/`SaveAsync` lê/escreve nova chave
- `UpdateAppSettingsUseCase` aceita/valida bool
- `PlexAutoSkipBackgroundService.PollAndSkipAsync`: `AutoSkipAuthorized=false` → retorna cedo sem seek

### E2E Tests (Playwright)
1. Login admin → Settings → toggle "Autorizar skip automático" OFF → modal aparece
2. Clicar "Cancelar" → toggle permanece OFF, GET `/api/settings` retorna `false`
3. Clicar "Autorizar" → toggle fica ON, GET `/api/settings` retorna `true`
4. Com ON → trigger marker no background service (mock) → seek executado
5. Com OFF → trigger marker → seek NÃO executado

---

## 5. Rollout / Compatibility

- **Default `false`** = auto-skip desabilitado para novos usuários (mais seguro); usuários existentes precisam autorizar uma vez
- Nenhuma migração de dados
- Endpoint protegido por auth admin + CSRF existente