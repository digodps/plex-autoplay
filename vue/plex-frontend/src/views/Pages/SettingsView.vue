<script setup>
import { onMounted, reactive, ref } from "vue";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import { apiFetch, extractErrorMessage } from "@/api";

const form = reactive({
  plex_url: "",
  media_root: "",
  sync_interval_minutes: 5,
  sync_catalog_interval_minutes: 30,
});
const plexToken = ref("");
const plexTokenSet = ref(false);
const loading = ref(false);
const saving = ref(false);
const message = ref("");
const error = ref("");

function applySettings(data) {
  form.plex_url = data.plex_url ?? "";
  form.media_root = data.media_root ?? "";
  form.sync_interval_minutes = data.sync_interval_minutes ?? 5;
  form.sync_catalog_interval_minutes = data.sync_catalog_interval_minutes ?? 30;
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
  saving.value = true;
  message.value = "";
  error.value = "";
  try {
    const payload = {
      plex_url: form.plex_url,
      media_root: form.media_root,
      sync_interval_minutes: Number(form.sync_interval_minutes),
      sync_catalog_interval_minutes: Number(form.sync_catalog_interval_minutes),
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
  <DefaultLayout>
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div class="border-b border-gray-200 dark:border-gray-800 sticky top-0 z-20 bg-gray-50 dark:bg-gray-900">
        <div class="max-w-2xl mx-auto px-4 py-3">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Ajustes</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Configurações do Plex e sincronização, persistidas no banco.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8">
        <div class="max-w-2xl mx-auto space-y-6">
          <div v-if="loading" class="text-sm text-gray-500 dark:text-gray-400">Carregando...</div>

          <form v-else class="space-y-6" @submit.prevent="saveSettings">
            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="sync-interval">
                Intervalo de Sincronização (minutos)
              </label>
              <input
                id="sync-interval"
                v-model.number="form.sync_interval_minutes"
                type="number"
                min="1"
                max="1440"
                required
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
              <p class="text-xs text-gray-500 dark:text-gray-400">
                Intervalo entre varreduras automáticas das bibliotecas.
              </p>
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="catalog-interval">
                Intervalo de Sincronização do Catálogo (minutos)
              </label>
              <input
                id="catalog-interval"
                v-model.number="form.sync_catalog_interval_minutes"
                type="number"
                min="1"
                max="1440"
                required
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-url">
                URL do Plex
              </label>
              <input
                id="plex-url"
                v-model="form.plex_url"
                type="url"
                required
                placeholder="http://192.168.1.5:32400"
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-token">
                Token do Plex
              </label>
              <input
                id="plex-token"
                v-model="plexToken"
                type="password"
                autocomplete="new-password"
                :placeholder="plexTokenSet ? 'Token configurado — deixe vazio para manter' : 'Informe o token do Plex'"
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ plexTokenSet ? "Token já configurado. Preencha apenas para substituir." : "Nenhum token configurado." }}
              </p>
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="media-root">
                Diretório de Mídia (MEDIA_ROOT)
              </label>
              <input
                id="media-root"
                v-model="form.media_root"
                type="text"
                required
                placeholder="/media"
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
              <p class="text-xs text-gray-500 dark:text-gray-400">
                Caminho existente dentro do container.
              </p>
            </div>

            <p v-if="message" class="text-sm text-green-600 dark:text-green-400">{{ message }}</p>
            <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>

            <div class="flex items-center justify-end gap-2">
              <button
                type="button"
                class="rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600 disabled:opacity-50"
                :disabled="saving"
                @click="loadSettings"
              >
                Recarregar
              </button>
              <button
                type="submit"
                class="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition hover:opacity-90 disabled:opacity-50"
                :disabled="saving"
              >
                {{ saving ? "Salvando..." : "Salvar Configurações" }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </DefaultLayout>
</template>
