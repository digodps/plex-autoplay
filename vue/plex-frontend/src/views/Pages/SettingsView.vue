<script setup>
import { onMounted, reactive, ref } from "vue";
import { apiFetch, extractErrorMessage } from "@/api";
import AppIcon from "@/components/AppIcon.vue";
import MediaRootsField from "@/components/MediaRootsField.vue";

const form = reactive({
  plex_url: "",
  sync_interval_minutes: 5,
});
const mediaRoots = ref([]);
const plexToken = ref("");
const plexTokenSet = ref(false);
const loading = ref(false);
const saving = ref(false);
const message = ref("");
const error = ref("");
const showPlexToken = ref(false);
const plexTokenRef = ref(null);

function togglePlexToken() {
  showPlexToken.value = !showPlexToken.value;
  plexTokenRef.value?.focus();
}

function applySettings(data) {
  form.plex_url = data.plex_url ?? "";
  mediaRoots.value = data.media_roots ?? [];
  form.sync_interval_minutes = data.sync_interval_minutes ?? 5;
  plexTokenSet.value = Boolean(data.plex_token_set);
  plexToken.value = "";
}

async function loadSettings() {
  loading.value = true;
  error.value = "";
  try {
    const res = await apiFetch("/api/settings");
    if (!res.ok) throw new Error(await extractErrorMessage(res) || "Erro ao carregar configurações");
    applySettings(await res.json());
  } catch (err) {
    error.value = err.message;
  } finally {
    loading.value = false;
  }
}

async function saveSettings() {
  if (mediaRoots.value.length === 0) {
    error.value = "Adicione ao menos um diretório de mídia";
    return;
  }
  saving.value = true;
  message.value = "";
  error.value = "";
  try {
    const payload = {
      plex_url: form.plex_url,
      media_roots: mediaRoots.value,
      sync_interval_minutes: Number(form.sync_interval_minutes),
    };
    if (plexToken.value) payload.plex_token = plexToken.value;

    const res = await apiFetch("/api/settings", {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!res.ok) throw new Error(await extractErrorMessage(res) || "Erro ao salvar configurações");

    applySettings(await res.json());
    message.value = "Configurações salvas com sucesso";
  } catch (err) {
    error.value = err.message;
  } finally {
    saving.value = false;
  }
}

onMounted(loadSettings);
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-3">
    <div class="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 py-6 space-y-6">
      <div class="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <h1 class="text-2xl font-bold text-white">Ajustes</h1>
          <p class="mt-1 text-sm text-gray-400">
            Configurações do Plex e sincronização, persistidas no banco.
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark disabled:opacity-50"
            :disabled="saving"
            @click="loadSettings"
          >
            Recarregar
          </button>
          <button
            type="submit"
            form="settings-form"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark disabled:opacity-50"
            :disabled="saving"
          >
            {{ saving ? "Salvando..." : "Salvar Configurações" }}
          </button>
        </div>
      </div>

      <div v-if="loading" class="text-sm text-gray-400">Carregando...</div>

      <form v-else id="settings-form" class="space-y-6" @submit.prevent="saveSettings">
        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-300" for="sync-interval">
            Intervalo de Sincronização (minutos)
          </label>
          <input
            id="sync-interval"
            v-model.number="form.sync_interval_minutes"
            type="number"
            min="1"
            max="1440"
            required
            class="w-full rounded-lg border border-gray-700 bg-[#111827] text-white placeholder-gray-500 px-4 py-2.5 focus:border-amber-400 focus:outline-none focus:ring-1 focus:ring-amber-400"
          />
          <p class="text-xs text-gray-500">
            Intervalo entre varreduras automáticas das bibliotecas.
          </p>
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-300" for="plex-url">
            URL do Plex
          </label>
          <input
            id="plex-url"
            v-model="form.plex_url"
            type="url"
            required
            placeholder="http://192.168.1.5:32400"
            class="w-full rounded-lg border border-gray-700 bg-[#111827] text-white placeholder-gray-500 px-4 py-2.5 focus:border-amber-400 focus:outline-none focus:ring-1 focus:ring-amber-400"
          />
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-300" for="plex-token">
            Token do Plex
          </label>
          <div class="relative">
            <input
              id="plex-token"
              ref="plexTokenRef"
              v-model="plexToken"
              :type="showPlexToken ? 'text' : 'password'"
              autocomplete="new-password"
              :placeholder="plexTokenSet ? 'Token configurado — deixe vazio para manter' : 'Informe o token do Plex'"
              class="w-full rounded-lg border border-gray-700 bg-[#111827] text-white placeholder-gray-500 px-4 py-2.5 pr-12 focus:border-amber-400 focus:outline-none focus:ring-1 focus:ring-amber-400"
              data-testid="plex-token"
            />
            <button
              type="button"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-white transition"
              :aria-label="showPlexToken ? 'Ocultar token' : 'Mostrar token'"
              :aria-pressed="showPlexToken"
              data-testid="toggle-password-plex-token"
              @click="togglePlexToken"
            >
              <AppIcon :name="showPlexToken ? 'eye-off' : 'eye'" size="sm" />
            </button>
          </div>
          <p class="text-xs text-gray-500">
            {{ plexTokenSet ? "Token já configurado. Preencha apenas para substituir." : "Nenhum token configurado." }}
          </p>
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <MediaRootsField v-model="mediaRoots" label="Diretórios de Mídia (MEDIA_ROOTS)" />
        </div>

        <p v-if="message" class="text-sm text-green-400">{{ message }}</p>
        <p v-if="error" class="text-sm text-red-400">{{ error }}</p>
      </form>
    </div>
  </div>
</template>
